using System.Text;
using System.Text.Json;
using Backend.Chats;
using Backend.Knowledge;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Backend.Messages.Pipelines;

public class GatherKnowledge : IConversationPipelineStep
{
    private readonly KnowledgeService knowledgeService;
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public GatherKnowledge(KnowledgeService knowledgeService, IOptions<JsonOptions> jsonOptions)
    {
        this.knowledgeService = knowledgeService;
        this.jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
    }

    public async Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        var chat = context.Get<Chat>("chat");
        var requestText = context.Get<string>("requestText");
        var combinedMessage = new StringBuilder();

        var knowledge = await knowledgeService.Search(
            KnowledgeSource.All,
            requestText,
            chat.ProjectId,
            chat.Id,
            cancellationToken);

        AddUserPreferences(knowledge, combinedMessage);
        AddFacts(knowledge, combinedMessage);
        AddDecisions(knowledge, combinedMessage);
        AddSummaries(knowledge, combinedMessage);
        AddDocuments(knowledge, combinedMessage);

        if (combinedMessage.Length > 0)
        {
            combinedMessage.Insert(0,
                """
                Use the provided context only if it is relevant to the user’s request.

                - Do not force usage of context
                - Prefer precise facts over general summaries
                - If context conflicts, prefer more specific or recent information

                ---

                ## Relevant Knowledge

                """);

            combinedMessage.AppendLine(
                """
                ---

                ## Instructions

                - Use User Profile only if clearly relevant
                - Use **Facts** when directly relevant
                - Use **Decisions** to maintain consistency with past choices
                - Use **Summary** only for background understanding
                - Use **Documents** as supporting material, not ground truth

                If the context is not relevant, ignore it completely.
                """);

            chat.AddMessage(Message.ForUserContext(chat.Id, combinedMessage.ToString()));
        }
    }

    private void AddUserPreferences(
        IEnumerable<KnowledgeEntry> knowledgeEntries,
        StringBuilder combinedMessage)
    {
        var preferences = knowledgeEntries
            .Where(x => x.SourceType == KnowledgeSource.ChatUserPreference)
            .ToArray();

        if (preferences.Length == 0)
            return;

        var json = JsonSerializer.Serialize(preferences, jsonSerializerOptions);

        combinedMessage
            .AppendLine("### User Profile (long-term preferences, may be outdated)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }

    private void AddFacts(
        IEnumerable<KnowledgeEntry> knowledgeEntries,
        StringBuilder combinedMessage)
    {
        var facts = knowledgeEntries
            .Where(x => x.SourceType == KnowledgeSource.ChatFact)
            .ToArray();

        if (facts.Length == 0)
            return;

        var json = JsonSerializer.Serialize(facts, jsonSerializerOptions);

        combinedMessage
            .AppendLine("### Facts (high confidence, atomic)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }

    private void AddDecisions(
        IEnumerable<KnowledgeEntry> knowledgeEntries,
        StringBuilder combinedMessage)
    {
        var decisions = knowledgeEntries
            .Where(x => x.SourceType == KnowledgeSource.ChatDecision)
            .ToArray();

        if (decisions.Length == 0)
            return;

        var json = JsonSerializer.Serialize(decisions, jsonSerializerOptions);

        combinedMessage
            .AppendLine("### Decisions (high confidence, atomic)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }

    private void AddSummaries(
        IEnumerable<KnowledgeEntry> knowledgeEntries,
        StringBuilder combinedMessage)
    {
        var summaries = knowledgeEntries
            .Where(x => x.SourceType == KnowledgeSource.ChatSummary)
            .ToArray();

        if (summaries.Length == 0)
            return;

        var json = JsonSerializer.Serialize(summaries, jsonSerializerOptions);

        combinedMessage
            .AppendLine("### Summary (general overview, may be incomplete)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }

    private void AddDocuments(
        IEnumerable<KnowledgeEntry> knowledgeEntries,
        StringBuilder combinedMessage)
    {
        var documents = knowledgeEntries
            .Where(x => x.SourceType == KnowledgeSource.DocumentChunk)
            .ToArray();

        if (documents.Length == 0)
            return;

        var json = JsonSerializer.Serialize(documents, jsonSerializerOptions);

        combinedMessage
            .AppendLine("### Related Documents (external knowledge, may be partial or noisy)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }
}