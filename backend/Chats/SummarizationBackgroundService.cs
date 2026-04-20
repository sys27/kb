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
        - keyPoints: list of important facts or insights
        - decisions: only if a clear decision was made
        - importance: 0.0-1.0 based on long-term usefulness

        Output in JSON format with the following structure:
        {{
          "summary": "string",
          "topics": ["string"],
          "facts": ["string"],
          "decisions": [{{
            "decision": "string",
            "reason": "string"
          }}],
          "importance": "number"
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
        while (!stoppingToken.IsCancellationRequested)
        {
            var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KbDbContext>();
            var chats = await dbContext.Chats
                .Include(x => x.Messages)
                .Include(x => x.Topics)
                .Include(x => x.Facts)
                .Include(x => x.Decisions)
                .Where(x => x.Messages.Count > 0 &&
                            ((x.LastSummaryUpdate == null &&
                              x.LastMessageAt < DateTime.UtcNow.AddMinutes(-10)) ||
                             (x.LastSummaryUpdate < DateTime.UtcNow.AddMinutes(-10) &&
                              x.LastSummaryUpdate < x.LastMessageAt)))
                .AsSplitQuery()
                .ToListAsync(stoppingToken);

            foreach (var chat in chats)
            {
                var messages = chat.Messages
                    .Where(x => x.Role is MessageRole.Assistant or MessageRole.User &&
                                x.Kind is MessageKind.Text)
                    .OrderBy(x => x.Id)
                    .Select(x => new Message(x.Role, x.Text, x.Timestamp))
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
                chat.Importance = summaryResponse.Importance;

                await dbContext.SaveChangesAsync(stoppingToken);

                while (true)
                {
                    var embeddings = await vectorCollection
                        .GetAsync(
                            x => x.SourceType == (int)EmbeddingSourceType.Chat && x.SourceId == chat.Id,
                            10,
                            null,
                            stoppingToken)
                        .Select(x => x.Id)
                        .ToListAsync(stoppingToken);

                    if (embeddings.Count == 0)
                        break;

                    await vectorCollection.DeleteAsync(embeddings, stoppingToken);
                }

                var summaryEmbeddings = Embeddings.ForChat(chat.Id, summaryResponse.Summary);
                await vectorCollection.UpsertAsync(summaryEmbeddings, stoppingToken);

                foreach (var topic in summaryResponse.Topics)
                {
                    var topicEmbeddings = Embeddings.ForChat(chat.Id, topic);
                    await vectorCollection.UpsertAsync(topicEmbeddings, stoppingToken);
                }

                foreach (var fact in summaryResponse.Facts)
                {
                    var factEmbeddings = Embeddings.ForChat(chat.Id, fact);
                    await vectorCollection.UpsertAsync(factEmbeddings, stoppingToken);
                }

                foreach (var (decision, reason) in summaryResponse.Decisions)
                {
                    var decisionEmbeddings = Embeddings.ForChat(chat.Id, $"Decision: {decision}. Reason: {reason}.");
                    await vectorCollection.UpsertAsync(decisionEmbeddings, stoppingToken);
                }
            }

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
            return JsonSerializer.Deserialize<SummaryResponse>(span[7..^10], JsonSerializerOptions.Web);

        if (span.StartsWith("```"))
            return JsonSerializer.Deserialize<SummaryResponse>(span[3..^6], JsonSerializerOptions.Web);

        return null;
    }

    private record Conversation(List<Message> Messages);

    private record Message(MessageRole Role, string Text, DateTime Timestamp);

    private record DecisionResponse(string Decision, string Reason);

    private class SummaryResponse
    {
        public string? Summary { get; set; }

        public List<string> Topics { get; set; } = [];

        public List<string> Facts { get; set; } = [];

        public List<DecisionResponse> Decisions { get; set; } = [];

        public float? Importance { get; set; }
    }
}