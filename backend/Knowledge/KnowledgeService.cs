using System.Runtime.CompilerServices;
using Backend.Llama;
using Backend.Vectors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.VectorData;

namespace Backend.Knowledge;

public partial class KnowledgeService
{
    private const int MaxKnowledgeSources = 25;

    private readonly ILogger<KnowledgeService> logger;
    private readonly KbDbContext dbContext;
    private readonly VectorStoreCollection<int, Embeddings> vectorCollection;
    private readonly LlamaCppClient llamaCppClient;

    public KnowledgeService(
        ILogger<KnowledgeService> logger,
        KbDbContext dbContext,
        VectorStoreCollection<int, Embeddings> vectorCollection,
        LlamaCppClient llamaCppClient)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.vectorCollection = vectorCollection;
        this.llamaCppClient = llamaCppClient;
    }

    public async Task<IReadOnlyList<Knowledge>> Search(
        KnowledgeSource source,
        string query,
        int? projectId,
        CancellationToken cancellationToken = default)
    {
        var result = await VectorSearch(source, query, projectId, cancellationToken)
            .Concat(BestMatching25Search(source, query, projectId, cancellationToken))
            .DistinctBy(x => (x.SourceId, x.SourceType))
            .ToArrayAsync(cancellationToken);

        if (result.Length == 0)
            return [];

        var reranked = await llamaCppClient
            .Rerank(
                query,
                result,
                x => x.Content.GetContent(),
                new RerankOptions { TopK = 5 },
                cancellationToken)
            .ToArrayAsync(cancellationToken);

        return reranked;
    }

    private async IAsyncEnumerable<Knowledge> VectorSearch(
        KnowledgeSource source,
        string query,
        int? projectId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var sourceId = (int)source;
        var vectorOptions = new VectorSearchOptions<Embeddings>
        {
            Filter = projectId == null
                ? e => e.ProjectId == null && e.SourceType == sourceId
                : e => e.ProjectId == projectId && e.SourceType == sourceId,
        };
        var vectorSearchResults = vectorCollection.SearchAsync(
            query,
            MaxKnowledgeSources,
            vectorOptions,
            cancellationToken);

        await foreach (var result in vectorSearchResults)
        {
            var knowledge = await GetKnowledge(
                (KnowledgeSource)result.Record.SourceType,
                result.Record.SourceId,
                cancellationToken);

            if (knowledge is null)
            {
                logger.LogWarning("No knowledge found for embeddings record {RecordId}", result.Record.Id);
                continue;
            }

            yield return knowledge;
        }
    }

    private async IAsyncEnumerable<Knowledge> BestMatching25Search(
        KnowledgeSource source,
        string query,
        int? projectId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var sourceId = (int)source;
        var escapedQuery = EscapeFtsQuery(query);
        var ids = dbContext.Database
            .SqlQuery<int>(
                $"""
                 SELECT FTIV.ROWID AS Id
                 FROM FullTextIndexVirt AS FTIV
                          INNER JOIN FullTextIndex AS FTI ON FTIV.ROWID = FTI.Id
                 WHERE ({projectId} IS NULL AND FTI.ProjectId IS NULL OR FTI.ProjectId = {projectId})
                   AND FTI.SourceType = {sourceId}
                   AND FullTextIndexVirt MATCH {escapedQuery}
                 ORDER BY bm25(FullTextIndexVirt)
                 LIMIT {MaxKnowledgeSources}
                 """);

        foreach (var id in ids)
        {
            var knowledge = await GetKnowledge(source, id, cancellationToken);
            if (knowledge is null)
            {
                logger.LogWarning("No {Source} found for {Id}", source, id);
                continue;
            }

            yield return knowledge;
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

    private async Task<Knowledge?> GetKnowledge(
        KnowledgeSource sourceType,
        int sourceId,
        CancellationToken cancellationToken = default)
    {
        if (sourceType == KnowledgeSource.DocumentChunk)
        {
            var chunk = await dbContext.DocumentChunks
                .Include(x => x.DocumentSection!)
                .ThenInclude(x => x.Document)
                .FirstOrDefaultAsync(x => x.Id == sourceId, cancellationToken);

            if (chunk is null)
            {
                LogNoDocumentChunkFound(sourceId);
                return null;
            }

            return new Knowledge(
                KnowledgeSource.DocumentChunk,
                sourceId,
                new DocumentChunkKnowledge(
                    chunk.DocumentSection!.Document!.Title,
                    chunk.DocumentSection!.Header,
                    chunk.Content));
        }

        if (sourceType == KnowledgeSource.ChatSummary)
        {
            var chat = await dbContext.Chats
                .FirstOrDefaultAsync(x => x.Id == sourceId, cancellationToken);

            if (chat is null)
            {
                LogNoChatFound(sourceId);
                return null;
            }

            return new Knowledge(
                KnowledgeSource.ChatSummary,
                sourceId,
                new ChatSummaryKnowledge(chat.Summary));
        }

        if (sourceType == KnowledgeSource.ChatFact)
        {
            var fact = await dbContext.ChatFacts
                .FirstOrDefaultAsync(x => x.Id == sourceId, cancellationToken);

            if (fact is null)
            {
                LogNoChatFactFound(sourceId);
                return null;
            }

            return new Knowledge(
                KnowledgeSource.ChatFact,
                sourceId,
                new ChatFactKnowledge(fact.Fact));
        }

        if (sourceType == KnowledgeSource.ChatDecision)
        {
            var decision = await dbContext.ChatDecisions
                .FirstOrDefaultAsync(x => x.Id == sourceId, cancellationToken);

            if (decision is null)
            {
                LogNoChatDecisionFound(sourceId);
                return null;
            }

            return new Knowledge(
                KnowledgeSource.ChatDecision,
                sourceId,
                new ChatDecisionKnowledge(decision.Decision, decision.Reason));
        }

        if (sourceType == KnowledgeSource.ChatUserPreference)
        {
            var preference = await dbContext.ChatUserPreferences
                .FirstOrDefaultAsync(x => x.Id == sourceId, cancellationToken);

            if (preference is null)
            {
                LogNoChatUserPreferenceFound(sourceId);
                return null;
            }

            return new Knowledge(
                KnowledgeSource.ChatUserPreference,
                sourceId,
                new ChatUserPreferenceKnowledge(preference.Preference));
        }

        throw new ArgumentOutOfRangeException(nameof(sourceType), "Unknown source type");
    }

    [LoggerMessage(LogLevel.Debug, "No document chunk found for id {ChunkId}")]
    private partial void LogNoDocumentChunkFound(int chunkId);

    [LoggerMessage(LogLevel.Debug, "No chat found for id {ChatId}")]
    private partial void LogNoChatFound(int chatId);

    [LoggerMessage(LogLevel.Debug, "No chat fact found for id {FactId}")]
    private partial void LogNoChatFactFound(int factId);

    [LoggerMessage(LogLevel.Debug, "No chat decision found for id {DecisionId}")]
    private partial void LogNoChatDecisionFound(int decisionId);

    [LoggerMessage(LogLevel.Debug, "No chat user preference found for id {PreferenceId}")]
    private partial void LogNoChatUserPreferenceFound(int preferenceId);
}