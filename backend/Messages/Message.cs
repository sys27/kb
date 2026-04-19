using Backend.Chats;

namespace Backend.Messages;

public class Message
{
    public int Id { get; init; }

    public required MessageRole Role { get; init; }

    public required MessageKind Kind { get; init; }

    public required string Text { get; init; }

    public DateTimeOffset Timestamp { get; init; }

    public int ChatId { get; init; }

    public Chat? Chat { get; init; }

    public static Message ForSystem(int chatId, string text)
        => new Message
        {
            Role = MessageRole.System,
            Kind = MessageKind.Text,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTimeOffset.UtcNow
        };

    public static Message ForUser(int chatId, string text)
        => new Message
        {
            Role = MessageRole.User,
            Kind = MessageKind.Text,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTimeOffset.UtcNow
        };

    public static Message ForAssistant(int chatId, string text)
        => new Message
        {
            Role = MessageRole.Assistant,
            Kind = MessageKind.Text,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTimeOffset.UtcNow
        };

    public static Message ForReasoning(int chatId, string text)
        => new Message
        {
            Role = MessageRole.Assistant,
            Kind = MessageKind.Reasoning,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTimeOffset.UtcNow
        };

    public static Message ForTool(int chatId, string text)
        => new Message
        {
            Role = MessageRole.Tool,
            Kind = MessageKind.Text,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTimeOffset.UtcNow
        };
}