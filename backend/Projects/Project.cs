using Backend.Chats;

namespace Backend.Projects;

public class Project
{
    public int Id { get; init; }

    public required string Name { get; set; }

    public ICollection<Chat> Chats { get; init; } = new List<Chat>();

    public ICollection<Document> Documents { get; init; } = new List<Document>();
}