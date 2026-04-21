using System.Text;
using Backend.Chats;
using Backend.Vectors;
using Microsoft.Extensions.VectorData;

namespace Backend.Messages.Pipelines;

public class GatherKnowledge : IConversationPipelineStep
{
    private readonly KbDbContext dbContext;
    private readonly VectorStoreCollection<int, Embeddings> vectorCollection;

    public GatherKnowledge(KbDbContext dbContext, VectorStoreCollection<int, Embeddings> vectorCollection)
    {
        this.dbContext = dbContext;
        this.vectorCollection = vectorCollection;
    }

    public async Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        // TODO: use LLM to generate query?
        // TODO: deduplicate?
        // TODO: ranking?
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

                ---

                ## User Request
                """);

            combinedMessage.AppendLine(requestText);

            context.Set("requestText", combinedMessage.ToString());
        }
    }

    private async Task AddUserPreferences(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var vectorOptions = new VectorSearchOptions<Embeddings>
        {
            ScoreThreshold = 0.5,
            Filter = e => e.ProjectId == chat.ProjectId &&
                          e.SourceType == (int)EmbeddingSourceType.ChatUserPreference,
        };
        var vectorSearchResults = await vectorCollection
            .SearchAsync(requestText, 3, vectorOptions, cancellationToken)
            .ToListAsync(cancellationToken);

        if (vectorSearchResults.Count > 0)
        {
            combinedMessage.AppendLine("### User Profile (long-term preferences, may be outdated)");

            foreach (var result in vectorSearchResults)
            {
                var preference = await dbContext.GetEmbeddingsContent(result.Record, cancellationToken);

                combinedMessage.Append("- ").AppendLine(preference);
            }
        }
    }

    private async Task AddFacts(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var vectorOptions = new VectorSearchOptions<Embeddings>
        {
            ScoreThreshold = 0.5,
            Filter = e => e.ProjectId == chat.ProjectId &&
                          e.SourceType == (int)EmbeddingSourceType.ChatFact,
        };
        var vectorSearchResults = await vectorCollection
            .SearchAsync(requestText, 3, vectorOptions, cancellationToken)
            .ToListAsync(cancellationToken);

        if (vectorSearchResults.Count > 0)
        {
            combinedMessage.AppendLine("### Facts (high confidence, atomic)");

            foreach (var result in vectorSearchResults)
            {
                var fact = await dbContext.GetEmbeddingsContent(result.Record, cancellationToken);

                combinedMessage.Append("- ").AppendLine(fact);
            }
        }
    }

    private async Task AddDecisions(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var vectorOptions = new VectorSearchOptions<Embeddings>
        {
            ScoreThreshold = 0.5,
            Filter = e => e.ProjectId == chat.ProjectId &&
                          e.SourceType == (int)EmbeddingSourceType.ChatDecision,
        };
        var vectorSearchResults = await vectorCollection
            .SearchAsync(requestText, 3, vectorOptions, cancellationToken)
            .ToListAsync(cancellationToken);

        if (vectorSearchResults.Count > 0)
        {
            combinedMessage.AppendLine("### Decisions (high confidence, atomic)");

            foreach (var result in vectorSearchResults)
            {
                var decision = await dbContext.GetEmbeddingsContent(result.Record, cancellationToken);

                combinedMessage.Append("- ").AppendLine(decision);
            }
        }
    }

    private async Task AddSummaries(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var vectorOptions = new VectorSearchOptions<Embeddings>
        {
            ScoreThreshold = 0.5,
            Filter = e => e.ProjectId == chat.ProjectId &&
                          e.SourceType == (int)EmbeddingSourceType.ChatSummary,
        };
        var vectorSearchResults = await vectorCollection
            .SearchAsync(requestText, 3, vectorOptions, cancellationToken)
            .ToListAsync(cancellationToken);

        if (vectorSearchResults.Count > 0)
        {
            combinedMessage.AppendLine("### Summary (general overview, may be incomplete)");

            foreach (var result in vectorSearchResults)
            {
                var summary = await dbContext.GetEmbeddingsContent(result.Record, cancellationToken);

                combinedMessage.Append("- ").AppendLine(summary);
            }
        }
    }

    private async Task AddDocuments(
        Chat chat,
        string requestText,
        StringBuilder combinedMessage,
        CancellationToken cancellationToken)
    {
        var vectorOptions = new VectorSearchOptions<Embeddings>
        {
            ScoreThreshold = 0.5,
            Filter = e => e.ProjectId == chat.ProjectId &&
                          e.SourceType == (int)EmbeddingSourceType.DocumentChunk,
        };
        var vectorSearchResults = await vectorCollection
            .SearchAsync(requestText, 3, vectorOptions, cancellationToken)
            .ToListAsync(cancellationToken);

        if (vectorSearchResults.Count > 0)
        {
            combinedMessage.AppendLine("### Related Documents (external knowledge, may be partial or noisy)");

            for (var i = 0; i < vectorSearchResults.Count; i++)
            {
                var result = vectorSearchResults[i];
                var summary = await dbContext.GetEmbeddingsContent(result.Record, cancellationToken);

                combinedMessage
                    .Append('[')
                    .Append(i + 1)
                    .Append("] ")
                    .AppendLine(summary);
            }
        }
    }
}