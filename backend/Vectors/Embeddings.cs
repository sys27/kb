using Backend.Knowledge;
using Microsoft.Extensions.VectorData;

namespace Backend.Vectors;

public class Embeddings
{
    [VectorStoreKey]
    public int Id { get; init; }

    // TODO: other values?
    [VectorStoreVector(1024, DistanceFunction = DistanceFunction.CosineDistance)]
    public required string Embedding { get; init; }

    [VectorStoreData(IsIndexed = true)]
    public required int SourceId { get; init; }

    [VectorStoreData(IsIndexed = true)]
    public required int SourceType { get; init; }

    [VectorStoreData(IsIndexed = true)]
    public int? ProjectId { get; init; }

    public static Embeddings ForDocumentChunk(int? projectId, int documentChunkId, string content)
        => new Embeddings
        {
            Embedding = content,
            SourceId = documentChunkId,
            ProjectId = projectId,
            SourceType = (int)KnowledgeSource.DocumentChunk,
        };

    public static Embeddings ForChatSummary(int? projectId, int chatId, string content)
        => new Embeddings
        {
            Embedding = content,
            SourceId = chatId,
            ProjectId = projectId,
            SourceType = (int)KnowledgeSource.ChatSummary,
        };

    public static Embeddings ForChatFact(int? projectId, int factId, string content)
        => new Embeddings
        {
            Embedding = content,
            SourceId = factId,
            ProjectId = projectId,
            SourceType = (int)KnowledgeSource.ChatFact,
        };

    public static Embeddings ForChatDecision(int? projectId, int decisionId, string content)
        => new Embeddings
        {
            Embedding = content,
            SourceId = decisionId,
            ProjectId = projectId,
            SourceType = (int)KnowledgeSource.ChatDecision,
        };

    public static Embeddings ForChatUserPreference(int? projectId, int preferenceId, string content)
        => new Embeddings
        {
            Embedding = content,
            SourceId = preferenceId,
            ProjectId = projectId,
            SourceType = (int)KnowledgeSource.ChatUserPreference,
        };
}