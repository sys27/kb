using Backend.Chats;

namespace Backend.Messages;

public class Message
{
    public int Id { get; init; }

    public MessageKind Kind { get; init; }

    public required string Text { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public int ChatId { get; init; }

    public Chat? Chat { get; init; }
}