using Backend.Projects;

namespace Backend.Ingestion;

public class Document
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public DateTime? LastModifiedAt { get; set; }

    public byte[] Hash { get; set; } = [];

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

    public int ProjectId { get; init; }

    public Project? Project { get; init; }

    public ICollection<DocumentChunk> DocumentChunks { get; init; } = new List<DocumentChunk>();

    public void MarkAsToProcess()
        => Status = DocumentStatus.Pending;

    public void MarkAsProcessed()
        => Status = DocumentStatus.Ingested;

    public void MarkAsFailed()
        => Status = DocumentStatus.Failed;
}