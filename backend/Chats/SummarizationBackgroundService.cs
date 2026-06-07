using System.Text;
using System.Text.Json;
using Backend.Knowledge;
using Backend.Messages;
using Backend.Vectors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace Backend.Chats;

public partial class SummarizationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<SummarizationBackgroundService> logger;
    private readonly SummarizationOptions options;
    private readonly IChatClient chatClient;
    private readonly VectorStoreCollection<int, Embeddings> vectorCollection;

    private const string SummaryPrompt =
        """
        Analyze the following conversation and produce a structured summary.

        Important source rules:

        - Be concise and information-dense.
        - Ignore small talk.
        - The conversation contains messages from both User and Assistant.
        - Assistant messages are proposals, explanations, or suggestions.
        - Do NOT treat Assistant statements as facts, decisions, or user preferences unless the User explicitly agrees with them, confirms them, adopts them, or acts on them.
        - When extracting facts, decisions, and userPreferences, prioritize information that originated from the User.
        - If something was only suggested by the Assistant and never accepted by the User, do not include it.
        - Output JSON only. Do not include any other text.

        Fields:

        summary:
        - High-level summary of what was discussed.
        - May reference both User and Assistant messages.

        topics:
        - Main discussion topics.

        facts:
        - Only facts explicitly provided by the User.
        - Include confirmed outcomes only if the User explicitly acknowledged them.
        - Exclude Assistant explanations, recommendations, assumptions, and generated content.

        decisions:
        - Include only decisions explicitly made by the User.
        - A decision requires evidence that the User chose, adopted, rejected, or committed to something.
        - Do not treat Assistant recommendations as decisions.

        userPreferences:
        - Include only stable preferences, constraints, goals, habits, or recurring requirements explicitly stated by the User.
        - Do not infer preferences from Assistant suggestions.
        - Ignore temporary intentions unless they indicate a long-term pattern.

        Output JSON:
        ```
        {{
          "summary": "string",
          "topics": ["string"],
          "facts": ["string"],
          "decisions": [{{
            "decision": "string",
            "reason": "string"
          }}],
          "userPreferences": ["string"]
        }}
        ```

        Conversation:
        ```
        {0}
        ```
        """;

    public SummarizationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SummarizationBackgroundService> logger,
        IOptions<SummarizationOptions> options,
        IChatClient chatClient,
        VectorStoreCollection<int, Embeddings> vectorCollection)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.options = options.Value;
        this.chatClient = chatClient;
        this.vectorCollection = vectorCollection;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Starting summarization...");

            var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KbDbContext>();
            var summaryInactivityWindow = DateTime.UtcNow.Add(-options.SummaryInactivityWindow);
            var chats = await dbContext.Chats
                .Include(x => x.Messages)
                .ThenInclude(x => x.MessageType)
                .Include(x => x.Topics)
                .Include(x => x.Facts)
                .Include(x => x.Decisions)
                .Include(x => x.UserPreferences)
                .Where(x => x.Messages.Count > 0 &&
                            ((x.LastSummaryUpdate == null && x.LastMessageAt < summaryInactivityWindow) ||
                             (x.LastSummaryUpdate < summaryInactivityWindow && x.LastSummaryUpdate < x.LastMessageAt)))
                .AsSplitQuery()
                .ToListAsync(stoppingToken);

            LogFoundChatsToSummarize(chats.Count);

            foreach (var chat in chats)
            {
                LogSummarizingChat(chat.Id);

                var messages = chat.Messages
                    .Where(x => x.MessageTypeId is
                        MessageType.AssistantAnswerId or
                        MessageType.UserContextId or
                        MessageType.UserRequestId)
                    .OrderBy(x => x.Id)
                    .Select(x => new Message(x.MessageType!.Role, x.Text, x.Timestamp))
                    .ToList();


                var conversation = new Conversation(messages);
                var json = JsonSerializer.Serialize(conversation, JsonSerializerOptions.Web);
                var prompt = string.Format(SummaryPrompt, json);
                LogSummarizationPrompt(prompt);

                // TODO: summary of summaries?
                var chatMessage = new ChatMessage(ChatRole.User, prompt);
                var summary = await chatClient.GetResponseAsync(chatMessage, null, stoppingToken);

                LogSummaryForChat(chat.Id, summary.Text);

                var summaryResponse = ParseSummaryResponse(summary.Text);
                if (summaryResponse is null)
                {
                    logger.LogError("Failed to parse summary response: {Response}", summary.Text);
                    continue;
                }

                if (summaryResponse.Summary is null)
                {
                    logger.LogError("Summary is missing in response: {Response}", summary.Text);
                    continue;
                }

                chat.UpdateSummary(summaryResponse.Summary);
                chat.UpdateTopics(summaryResponse.Topics);
                chat.UpdateFacts(summaryResponse.Facts);
                chat.UpdateDecisions(summaryResponse.Decisions.Select(x => (x.Decision, x.Reason)));
                chat.UpdateUserPreferences(summaryResponse.UserPreferences);

                await dbContext.SaveChangesAsync(stoppingToken);

                while (true)
                {
                    var embeddings = await vectorCollection
                        .GetAsync(
                            x => (x.SourceType == (int)KnowledgeSource.ChatSummary ||
                                  x.SourceType == (int)KnowledgeSource.ChatFact ||
                                  x.SourceType == (int)KnowledgeSource.ChatDecision ||
                                  x.SourceType == (int)KnowledgeSource.ChatUserPreference) &&
                                 x.SourceId == chat.Id,
                            10,
                            null,
                            stoppingToken)
                        .Select(x => x.Id)
                        .ToListAsync(stoppingToken);

                    if (embeddings.Count == 0)
                        break;

                    await vectorCollection.DeleteAsync(embeddings, stoppingToken);
                }

                var summaryAndTopics = new StringBuilder(chat.Summary);
                if (chat.Topics.Count > 0)
                {
                    summaryAndTopics.AppendLine("\nTopics:");

                    foreach (var topic in chat.Topics)
                        summaryAndTopics.Append("- ").AppendLine(topic.Topic);
                }

                var summaryEmbeddings = Embeddings.ForChatSummary(chat.ProjectId, chat.Id, summaryAndTopics.ToString());
                await vectorCollection.UpsertAsync(summaryEmbeddings, stoppingToken);

                foreach (var fact in chat.Facts)
                {
                    var factEmbeddings = Embeddings.ForChatFact(chat.ProjectId, fact.Id, fact.Fact);
                    await vectorCollection.UpsertAsync(factEmbeddings, stoppingToken);
                }

                foreach (var decision in chat.Decisions)
                {
                    var decisionEmbeddings = Embeddings.ForChatDecision(
                        chat.ProjectId,
                        decision.Id,
                        $"Decision: {decision.Decision}. Reason: {decision.Reason}.");
                    await vectorCollection.UpsertAsync(decisionEmbeddings, stoppingToken);
                }

                foreach (var preference in chat.UserPreferences)
                {
                    var preferenceEmbeddings = Embeddings.ForChatUserPreference(
                        chat.ProjectId,
                        preference.Id,
                        preference.Preference);
                    await vectorCollection.UpsertAsync(preferenceEmbeddings, stoppingToken);
                }

                LogSummarizationForChatCompleted(chat.Id);
            }

            logger.LogInformation("Summarization completed.");

            await Task.Delay(options.Delay, stoppingToken);
        }
    }

    private SummaryResponse? ParseSummaryResponse(string response)
    {
        if (response.Length <= 2)
            return null;

        var span = response.AsSpan();
        if (span[0] == '{')
            return JsonSerializer.Deserialize<SummaryResponse>(span, JsonSerializerOptions.Web);

        if (span.StartsWith("```json"))
            return JsonSerializer.Deserialize<SummaryResponse>(span[7..^3], JsonSerializerOptions.Web);

        if (span.StartsWith("```"))
            return JsonSerializer.Deserialize<SummaryResponse>(span[3..^3], JsonSerializerOptions.Web);

        return null;
    }

    private record Conversation(List<Message> Messages);

    private record Message(string Role, string Text, DateTime Timestamp);

    private record DecisionResponse(string Decision, string Reason);

    private class SummaryResponse
    {
        public string? Summary { get; set; }

        public List<string> Topics { get; set; } = [];

        public List<string> Facts { get; set; } = [];

        public List<DecisionResponse> Decisions { get; set; } = [];

        public List<string> UserPreferences { get; set; } = [];
    }

    [LoggerMessage(LogLevel.Debug, "Found {Count} chats to summarize.")]
    private partial void LogFoundChatsToSummarize(int count);

    [LoggerMessage(LogLevel.Debug, "Summarizing chat (Id: {ChatId}).")]
    private partial void LogSummarizingChat(int chatId);

    [LoggerMessage(LogLevel.Debug, "Summarization prompt: {Prompt}.")]
    private partial void LogSummarizationPrompt(string prompt);

    [LoggerMessage(LogLevel.Debug, "Summary for chat (Id: {ChatId}): {Response}")]
    private partial void LogSummaryForChat(int chatId, string response);

    [LoggerMessage(LogLevel.Debug, "Summarization for chat (Id: {ChatId}) completed.")]
    private partial void LogSummarizationForChatCompleted(int chatId);
}