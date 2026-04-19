using System.Text.Json;
using Backend.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Backend.Chats;

public class SummarizationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<SummarizationBackgroundService> logger;
    private readonly SummarizationOptions options;
    private readonly IChatClient chatClient;

    public SummarizationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SummarizationBackgroundService> logger,
        IOptions<SummarizationOptions> options,
        IChatClient chatClient)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
        this.options = options.Value;
        this.chatClient = chatClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KbDbContext>();
            var chats = await dbContext.Chats
                .Include(x => x.Messages.OrderBy(m => m.Id))
                .Where(x => x.Messages.Count > 0 &&
                            (x.LastSummaryUpdate == null ||
                             x.LastSummaryUpdate < x.Messages.OrderByDescending(m => m.Id).First().Timestamp))
                .ToListAsync(stoppingToken);

            foreach (var chat in chats)
            {
                var messages = chat.Messages
                    .Where(x => x.Role is MessageRole.Assistant or MessageRole.User &&
                                x.Kind is MessageKind.Text)
                    .Select(x => new Message(x.Role, x.Text, x.Timestamp))
                    .ToList();

                var conversation = new Conversation(messages);
                var json = JsonSerializer.Serialize(conversation, JsonSerializerOptions.Web);
                var prompt = $$"""
                               Analyze the following conversation and produce a structured summary.

                               Rules:
                               - Be concise and information-dense
                               - Ignore small talk
                               - Extract only meaningful technical or conceptual content
                               - Do not invent information

                               Fields:
                               - summary: short paragraph (2–5 sentences)
                               - topics: 3–8 concise tags
                               - keyPoints: list of important facts or insights
                               - decisions: only if a clear decision was made
                               - importance: 0.0–1.0 based on long-term usefulness

                               Output in JSON format with the following structure:
                               {
                                 "summary": "string",
                                 "tags": ["string"],
                                 "decisions": [{
                                   "decision": "string",
                                   "reason": "string"
                                 }],
                                 "facts": ["string"],
                                 "importance": "number"
                               }

                               Conversation:
                               {{json}}
                               """;

                // TODO: summary of summaries?
                var summary = await chatClient.GetResponseAsync(
                    new ChatMessage(ChatRole.User, prompt),
                    null,
                    stoppingToken);

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
                await dbContext.SaveChangesAsync(stoppingToken);
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

        public List<string> Tags { get; set; } = [];

        public List<DecisionResponse> Decisions { get; set; } = [];

        public List<string> Facts { get; set; } = [];

        public double? Importance { get; set; }
    }
}