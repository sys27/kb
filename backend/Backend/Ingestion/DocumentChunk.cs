namespace Backend.Ingestion;

public class DocumentChunk
{
    public int Id { get; init; }

    public required string Content { get; set; }

    public required int Start { get; init; }

    public required int Length { get; init; }

    public int DocumentSectionId { get; init; }

    public DocumentSection? DocumentSection { get; init; }
}