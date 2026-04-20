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

    public static Embeddings ForDocumentChunk(int documentChunkId, string content)
        => new Embeddings
        {
            Embedding = content,
            SourceId = documentChunkId,
            SourceType = (int)EmbeddingSourceType.DocumentChunk,
        };

    public static Embeddings ForChat(int chatId, string content)
        => new Embeddings
        {
            Embedding = content,
            SourceId = chatId,
            SourceType = (int)EmbeddingSourceType.Chat,
        };
}