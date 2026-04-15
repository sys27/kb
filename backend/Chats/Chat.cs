using Backend.Messages;

namespace Backend.Chats;

public class Chat
{
    public int Id { get; init; }

    public required string Title { get; set; }

    public ICollection<Message> Messages { get; init; } = new List<Message>();
}