namespace Backend.Chats;

public class ChatFact
{
    public int Id { get; init; }

    public required string Fact { get; init; }

    public int ChatId { get; init; }

    public Chat? Chat { get; init; }
}