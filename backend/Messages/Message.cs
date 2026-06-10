using Backend.Chats;
using Microsoft.Extensions.AI;

namespace Backend.Messages;

public class Message
{
    public int Id { get; init; }

    public required string Text { get; init; }

    public DateTime Timestamp { get; init; }

    public required int MessageTypeId { get; init; }

    public MessageType? MessageType { get; init; }

    public int ChatId { get; init; }

    public Chat? Chat { get; init; }

    public static Message ForSystem(int chatId, string text)
        => new Message
        {
            MessageTypeId = MessageType.SystemId,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };

    public static Message ForUserRequest(int chatId, string text)
        => new Message
        {
            MessageTypeId = MessageType.UserRequestId,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };

    public static Message ForUserContext(int chatId, string text)
        => new Message
        {
            MessageTypeId = MessageType.UserContextId,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };

    public static Message ForAssistant(int chatId, string text)
        => new Message
        {
            MessageTypeId = MessageType.AssistantAnswerId,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };

    public static Message ForReasoning(int chatId, string text)
        => new Message
        {
            MessageTypeId = MessageType.AssistantReasoningId,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };

    public static Message ForToolCall(int chatId, string text)
        => new Message
        {
            MessageTypeId = MessageType.ToolCallId,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };

    public static Message ForToolResult(int chatId, string text)
        => new Message
        {
            MessageTypeId = MessageType.ToolResultId,
            Text = text,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };

    public ChatMessage ToChatMessage()
    {
        var role = MessageTypeId switch
        {
            MessageType.SystemId => ChatRole.System,
            MessageType.AssistantAnswerId => ChatRole.Assistant,
            MessageType.UserContextId or MessageType.UserRequestId => ChatRole.User,

            _ => throw new InvalidOperationException(),
        };

        return new ChatMessage(role, Text);
    }
}