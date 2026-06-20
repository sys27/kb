using Backend.Chats;
using Backend.Ingestion;

namespace Backend.Projects;

public class Project
{
    public int Id { get; init; }

    public required string Name { get; set; }

    public ICollection<Chat> Chats { get; init; } = [];

    public ICollection<Document> Documents { get; init; } = [];

    public string GetDirectoryName()
        => $"project-{Id}";

    public void AddDocument(Document document)
        => Documents.Add(document);
}