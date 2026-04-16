using Backend.Messages;
using Backend.Projects;

namespace Backend.Chats;

public class Chat
{
    public int Id { get; init; }

    public required string Title { get; set; }

    public ICollection<Message> Messages { get; init; } = new List<Message>();

    public int? ProjectId { get; init; }

    public Project? Project { get; init; }
}