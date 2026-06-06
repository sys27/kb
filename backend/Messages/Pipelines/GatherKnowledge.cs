using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Chats;
using Backend.Knowledge;

namespace Backend.Messages.Pipelines;

public class GatherKnowledge : IConversationPipelineStep
{
    private readonly KnowledgeService knowledgeService;
    private readonly JsonSerializerOptions jsonOptions;

    public GatherKnowledge(KnowledgeService knowledgeService)
    {
        this.knowledgeService = knowledgeService;
        this.jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }

    public async Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        // TODO: use LLM to generate query?
        var chat = context.Get<Chat>("chat");
        var requestText = context.Get<string>("requestText");
        var combinedMessage = new StringBuilder();

        await AddUserPreferences(chat, requestText, combinedMessage, cancellationToken);
        await AddFacts(chat, requestText, combinedMessage, cancellationToken);
        await AddDecisions(chat, requestText, combinedMessage, cancellationToken);
        await AddSummaries(chat, requestText, combinedMessage, cancellationToken);
        await AddDocuments(chat, requestText, combinedMessage, cancellationToken);

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

    private async Task AddUserPreferences(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var preferences = await knowledgeService.Search(
            KnowledgeSource.ChatUserPreference,
            requestText,
            chat.ProjectId,
            cancellationToken);

        if (preferences.Count == 0)
            return;

        var json = JsonSerializer.Serialize(preferences, JsonSerializerOptions.Web);

        combinedMessage
            .AppendLine("### User Profile (long-term preferences, may be outdated)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }

    private async Task AddFacts(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var facts = await knowledgeService.Search(
            KnowledgeSource.ChatFact,
            requestText,
            chat.ProjectId,
            cancellationToken);

        if (facts.Count == 0)
            return;

        var json = JsonSerializer.Serialize(facts, JsonSerializerOptions.Web);

        combinedMessage
            .AppendLine("### Facts (high confidence, atomic)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }

    private async Task AddDecisions(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var decisions = await knowledgeService.Search(
            KnowledgeSource.ChatDecision,
            requestText,
            chat.ProjectId,
            cancellationToken);

        if (decisions.Count == 0)
            return;

        var json = JsonSerializer.Serialize(decisions, JsonSerializerOptions.Web);

        combinedMessage
            .AppendLine("### Decisions (high confidence, atomic)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }

    private async Task AddSummaries(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var summaries = await knowledgeService.Search(
            KnowledgeSource.ChatSummary,
            requestText,
            chat.ProjectId,
            cancellationToken);

        if (summaries.Count == 0)
            return;

        var json = JsonSerializer.Serialize(summaries, JsonSerializerOptions.Web);

        combinedMessage
            .AppendLine("### Summary (general overview, may be incomplete)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }

    private async Task AddDocuments(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var documents = await knowledgeService.Search(
            KnowledgeSource.DocumentChunk,
            requestText,
            chat.ProjectId,
            cancellationToken);

        if (documents.Count == 0)
            return;

        var json = JsonSerializer.Serialize(documents, jsonOptions);

        combinedMessage
            .AppendLine("### Related Documents (external knowledge, may be partial or noisy)")
            .AppendLine("```")
            .AppendLine(json)
            .AppendLine("```");
    }
}