using System.Text;
using System.Text.Json;
using Backend.Messages;
using Backend.Vectors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace Backend.Chats;

public class SummarizationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<SummarizationBackgroundService> logger;
    private readonly SummarizationOptions options;
    private readonly IChatClient chatClient;
    private readonly VectorStoreCollection<int, Embeddings> vectorCollection;

    private const string SummaryPrompt =
        """
        Analyze the following conversation and produce a structured summary.

        Rules:
        - Be concise and information-dense
        - Ignore small talk
        - Extract only meaningful technical or conceptual content
        - Do not invent information

        Fields:
        - summary: short paragraph (2-5 sentences)
        - topics: 3-8 concise tags
        - facts: list of important facts or insights
        - decisions: only if a clear decision was made
        - userPreferences: stable user facts, represent long-term preferences, constraints, or goals

        Output in JSON format with the following structure:
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

        Conversation:
        {0}
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

            foreach (var chat in chats)
            {
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

                // TODO: summary of summaries?
                var chatMessage = new ChatMessage(ChatRole.User, prompt);
                var summary = await chatClient.GetResponseAsync(chatMessage, null, stoppingToken);

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
                            x => (x.SourceType == (int)EmbeddingSourceType.ChatSummary ||
                                  x.SourceType == (int)EmbeddingSourceType.ChatFact ||
                                  x.SourceType == (int)EmbeddingSourceType.ChatDecision ||
                                  x.SourceType == (int)EmbeddingSourceType.ChatUserPreference) &&
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
}