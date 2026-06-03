using System.Runtime.CompilerServices;
using System.Text;
using Backend.Chats;
using Backend.Llama;
using Backend.Vectors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.VectorData;

namespace Backend.Messages.Pipelines;

public class GatherKnowledge : IConversationPipelineStep
{
    private readonly ILogger<GatherKnowledge> logger;
    private readonly KbDbContext dbContext;
    private readonly VectorStoreCollection<int, Embeddings> vectorCollection;
    private readonly LlamaCppClient llamaCppClient;

    public GatherKnowledge(
        ILogger<GatherKnowledge> logger,
        KbDbContext dbContext,
        VectorStoreCollection<int, Embeddings> vectorCollection,
        LlamaCppClient llamaCppClient)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.vectorCollection = vectorCollection;
        this.llamaCppClient = llamaCppClient;
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
        var vectorSearchResults = await VectorSearch(chat.ProjectId, requestText, cancellationToken)
            .Concat(BestMatching25Search(chat.ProjectId, requestText))
            .DistinctBy(x => x.Id)
            .Select(x => x.Content)
            .ToListAsync(cancellationToken);

        if (vectorSearchResults.Count <= 0)
            return;

        var reranked = await llamaCppClient
            .Rerank(requestText, vectorSearchResults, new RerankOptions { TopK = 5 }, cancellationToken)
            .ToArrayAsync(cancellationToken);

        if (reranked.Length <= 0)
            return;

        combinedMessage.AppendLine("### Related Documents (external knowledge, may be partial or noisy)");

        for (var i = 0; i < reranked.Length; i++)
            combinedMessage
                .Append('[')
                .Append(i + 1)
                .Append("] ")
                .AppendLine(reranked[i]);
    }

    private async IAsyncEnumerable<DocumentSearchResult> VectorSearch(
        int? projectId,
        string query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var vectorOptions = new VectorSearchOptions<Embeddings>
        {
            Filter = e => e.ProjectId == projectId &&
                          e.SourceType == (int)EmbeddingSourceType.DocumentChunk,
        };
        var vectorSearchResults = vectorCollection.SearchAsync(query, 10, vectorOptions, cancellationToken);

        await foreach (var result in vectorSearchResults)
        {
            var summary = await dbContext.GetEmbeddingsContent(result.Record, cancellationToken);
            if (summary is null)
            {
                logger.LogWarning("No content found for embeddings record {RecordId}", result.Record.Id);
                continue;
            }

            yield return new DocumentSearchResult(result.Record.Id, summary);
        }
    }

    private static string EscapeFtsQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "\"\"";

        var sanitized = query
            .Replace("\"", " ")
            .Replace("'", " ")
            .Replace("*", " ")
            .Replace("(", " ")
            .Replace(")", " ")
            .Replace("^", " ")
            .Replace("-", " ")
            .Replace("+", " ")
            .Replace(":", " ")
            .Trim();

        if (string.IsNullOrWhiteSpace(sanitized))
            return "\"\"";

        var tokens = sanitized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => $"\"{t}\"");

        return string.Join(" ", tokens);
    }

    private IAsyncEnumerable<DocumentSearchResult> BestMatching25Search(int? projectId, string query)
    {
        var escapedQuery = EscapeFtsQuery(query);

        return dbContext.Database
            .SqlQuery<DocumentSearchResult>(
                $"""
                 SELECT FTI.rowid AS Id, FTI.Content AS Content
                 FROM FullTextIndex AS FTI
                          INNER JOIN DocumentChunks AS DC ON DC.Id = FTI.rowid
                          INNER JOIN Documents AS D ON D.Id = DC.DocumentId
                 WHERE ({projectId} IS NULL OR D.ProjectId = {projectId})
                   AND FullTextIndex MATCH {escapedQuery}
                 ORDER BY bm25(FullTextIndex)
                 LIMIT 10
                 """)
            .AsAsyncEnumerable();
    }

    private record DocumentSearchResult(int Id, string Content);
}