namespace Backend.Ingestion;

public class DocumentSection
{
    public int Id { get; init; }

    public string? Header { get; set; }

    public int DocumentId { get; init; }

    public Document? Document { get; init; }

    public ICollection<DocumentChunk> DocumentChunks { get; init; } = new List<DocumentChunk>();
}