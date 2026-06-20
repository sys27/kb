using Backend.Chats;
using Backend.Projects;

namespace Backend.Ingestion;

public class Document
{
    public int Id { get; init; }

    public required string Name { get; init; }

    public string? Title { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public byte[] Hash { get; set; } = [];

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

    public int? ProjectId { get; init; }

    public Project? Project { get; init; }

    public int? ChatId { get; init; }

    public Chat? Chat { get; init; }

    public ICollection<DocumentSection> DocumentSections { get; init; } = new List<DocumentSection>();

    public bool IsPending
        => Status == DocumentStatus.Pending;

    public bool IsIngested
        => Status == DocumentStatus.Ingested;

    public bool IsFailed
        => Status == DocumentStatus.Failed;

    public void MarkAsToProcess()
        => Status = DocumentStatus.Pending;

    public void MarkAsProcessed()
        => Status = DocumentStatus.Ingested;

    public void MarkAsFailed()
    {
        DocumentSections.Clear();
        Status = DocumentStatus.Failed;
    }

    public string GetPath(string rootPath)
    {
        var directory = Project?.GetDirectoryName() ??
                        Chat?.GetDirectoryName() ??
                        throw new InvalidOperationException("Document must belong to a project or a chat");

        return Path.Combine(rootPath, directory, Name);
    }
}