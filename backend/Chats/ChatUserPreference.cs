namespace Backend.Chats;

public class ChatUserPreference
{
    public int Id { get; init; }

    public required string Preference { get; init; }

    public int ChatId { get; init; }

    public Chat? Chat { get; init; }
}