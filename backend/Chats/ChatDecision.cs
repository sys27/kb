namespace Backend.Chats;

public class ChatDecision
{
    public int Id { get; init; }

    public required string Decision { get; init; }

    public required string Reason { get; init; }

    public int ChatId { get; init; }

    public Chat? Chat { get; init; }
}