using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Backend.Llama;
using Backend.Messages;
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

    private const string QueryPrompt =
        """
        You are generating retrieval queries for a RAG system.

        Your task is to analyze the conversation and determine whether external information retrieval is needed to answer the user's latest message.

        You must also resolve references (pronouns, entities, and omitted context) using the conversation history.

        ---

        Rules:

        - Use the latest user message as the primary intent.
        - Resolve all references using the conversation history (e.g., "he", "she", "that company", "it", etc.).
        - If the latest message can be fully answered using only the conversation history, set:
          - vectorQuery = ""
          - bm25Query = ""
          - finalQuery = ""
        - Otherwise, generate retrieval queries as described below.
        - Do NOT include explanations or reasoning.
        - Output valid JSON only.

        ---

        Field definitions:

        finalQuery:
        - A fully resolved, standalone version of the user's latest intent.
        - Must include all resolved entities and context from the conversation.
        - Written in natural language.
        - NOT optimized for search engines.
        - This is the canonical interpretation of the user's question.

        vectorQuery:
        - Optimized for semantic / vector search.
        - Natural language question or statement.
        - Should closely follow finalQuery but can be slightly more explicit.

        bm25Query:
        - Optimized for lexical / keyword search.
        - Concise keyword-focused query.
        - Should include key entities, terms, and constraints.

        ---

        Output schema:

        {{
          "finalQuery": "string",
          "vectorQuery": "string",
          "bm25Query": "string"
        }}

        ---

        User's Query:

        ```
        {0}
        ```

        Conversation:

        ```
        {1}
        ```
        """;

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

    public async Task<IReadOnlyList<KnowledgeEntry>> Search(
        KnowledgeSource source,
        string query,
        int chatId,
        int? projectId,
        CancellationToken cancellationToken = default)
    {
        var queryResult = await GenerateQuery(chatId, query, cancellationToken);
        if (string.IsNullOrWhiteSpace(queryResult.VectorQuery) ||
            string.IsNullOrWhiteSpace(queryResult.Bm25Query) ||
            string.IsNullOrWhiteSpace(queryResult.FinalQuery))
            return [];

        var result = new List<KnowledgeEntry>();

        if (source.HasFlag(KnowledgeSource.DocumentChunk))
        {
            var documents = await SearchSingleSource(
                KnowledgeSource.DocumentChunk,
                queryResult,
                projectId,
                cancellationToken);

            result.AddRange(documents);
        }

        if (source.HasFlag(KnowledgeSource.ChatSummary))
        {
            var summaries = await SearchSingleSource(
                KnowledgeSource.ChatSummary,
                queryResult,
                projectId,
                cancellationToken);

            result.AddRange(summaries);
        }

        if (source.HasFlag(KnowledgeSource.ChatFact))
        {
            var facts = await SearchSingleSource(
                KnowledgeSource.ChatFact,
                queryResult,
                projectId,
                cancellationToken);

            result.AddRange(facts);
        }

        if (source.HasFlag(KnowledgeSource.ChatDecision))
        {
            var decisions = await SearchSingleSource(
                KnowledgeSource.ChatDecision,
                queryResult,
                projectId,
                cancellationToken);

            result.AddRange(decisions);
        }

        if (source.HasFlag(KnowledgeSource.ChatUserPreference))
        {
            var preferences = await SearchSingleSource(
                KnowledgeSource.ChatUserPreference,
                queryResult,
                projectId,
                cancellationToken);

            result.AddRange(preferences);
        }

        return result;
    }

    private async Task<IReadOnlyList<KnowledgeEntry>> SearchSingleSource(
        KnowledgeSource source,
        QueryResult query,
        int? projectId,
        CancellationToken cancellationToken = default)
    {
        Debug.Assert(BitOperations.PopCount((uint)source) == 1, "source must be a single source");

        var result = await VectorSearch(source, query.VectorQuery, projectId, cancellationToken)
            .Concat(BestMatching25Search(source, query.Bm25Query, projectId, cancellationToken))
            .DistinctBy(x => (x.SourceId, x.SourceType))
            .ToArrayAsync(cancellationToken);

        if (result.Length == 0)
            return [];

        var reranked = await llamaCppClient
            .Rerank(
                query.FinalQuery,
                result,
                x => x.Content.GetContent(),
                new RerankOptions { TopK = 5 },
                cancellationToken)
            .ToArrayAsync(cancellationToken);

        return reranked;
    }

    private async Task<QueryResult> GenerateQuery(int chatId, string query, CancellationToken cancellationToken)
    {
        var messages = await dbContext.Messages
            .Where(x => x.ChatId == chatId &&
                        (x.MessageTypeId == MessageType.AssistantAnswerId ||
                         x.MessageTypeId == MessageType.UserRequestId))
            .OrderByDescending(x => x.Id)
            .Take(4) // last two turns
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return new QueryResult(query, query, query);

        var conversation = new Conversation(
            messages
                .Select(m => new ConversationMessage(
                    m.MessageTypeId == MessageType.UserRequestId ? "user" : "assistant",
                    m.Text))
                .ToList());
        var json = JsonSerializer.Serialize(conversation, JsonSerializerOptions.Web);
        var prompt = string.Format(QueryPrompt, query, json);

        var response = await llamaCppClient.GetResponse(
            LlamaMessage.ForUser(prompt),
            GetResponseOptions.NoThinking,
            cancellationToken);

        var queryResult = JsonSerializer.Deserialize<QueryResult>(response, JsonSerializerOptions.Web);

        return queryResult;
    }

    private async IAsyncEnumerable<KnowledgeEntry> VectorSearch(
        KnowledgeSource source,
        string query,
        int? projectId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            yield break;

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

    private async IAsyncEnumerable<KnowledgeEntry> BestMatching25Search(
        KnowledgeSource source,
        string query,
        int? projectId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
            yield break;

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

    private async Task<KnowledgeEntry?> GetKnowledge(
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

            return new KnowledgeEntry(
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

            return new KnowledgeEntry(
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

            return new KnowledgeEntry(
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

            return new KnowledgeEntry(
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

            return new KnowledgeEntry(
                KnowledgeSource.ChatUserPreference,
                sourceId,
                new ChatUserPreferenceKnowledge(preference.Preference));
        }

        throw new ArgumentOutOfRangeException(nameof(sourceType), "Unknown source type");
    }

    private readonly record struct QueryResult(
        string VectorQuery,
        string Bm25Query,
        string FinalQuery);

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