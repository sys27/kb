namespace Backend.Chats;

public class ChatTopic
{
    public int Id { get; set; }

    public required string Topic { get; set; }

    public int ChatId { get; set; }

    public Chat? Chat { get; set; }
}