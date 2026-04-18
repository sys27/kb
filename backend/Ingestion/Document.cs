using Backend.Projects;

namespace Backend.Ingestion;

public class Document
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public required DateTime LastModifiedAt { get; init; }

    public required byte[] Hash { get; init; }

    public int ProjectId { get; init; }

    public Project? Project { get; init; }

    public ICollection<DocumentChunk> Chunks { get; init; } = new List<DocumentChunk>();
}