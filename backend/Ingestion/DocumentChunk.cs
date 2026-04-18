using Microsoft.Extensions.VectorData;

namespace Backend.Ingestion;

public class DocumentChunk
{
    [VectorStoreKey]
    public int Id { get; init; }

    [VectorStoreData]
    public required string Content { get; set; }

    // TODO: other values?
    [VectorStoreVector(1024, DistanceFunction = DistanceFunction.CosineDistance)]
    public required string Embedding { get; init; }

    [VectorStoreData]
    public required int Start { get; init; }

    [VectorStoreData]
    public required int Length { get; init; }

    [VectorStoreData]
    public int DocumentId { get; init; }

    public Document? Document { get; init; }
}