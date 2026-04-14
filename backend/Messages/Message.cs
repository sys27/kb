using Backend.Chats;

namespace Backend.Messages;

public record Message(int Id, MessageKind Kind, string Text, DateTimeOffset Timestamp)
{
    public required Chat Chat { get; init; }
}