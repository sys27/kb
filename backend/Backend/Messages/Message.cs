using System.Text.Json;
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

    public static Message ForToolCall(int chatId, FunctionCallContent content, JsonSerializerOptions jsonOptions)
    {
        var arguments = new Dictionary<string, string>();
        if (content.Arguments is not null)
            foreach (var (parameter, value) in content.Arguments)
                if (value is JsonElement element)
                    arguments[parameter] = element.GetRawText();

        var toolCall = new ToolCallContent(
            content.CallId,
            content.Name,
            arguments,
            content.Exception?.Message);

        var json = JsonSerializer.Serialize(toolCall, jsonOptions);

        return new Message
        {
            MessageTypeId = MessageType.ToolCallId,
            Text = json,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };
    }

    public static Message ForToolResult(int chatId, FunctionResultContent content, JsonSerializerOptions jsonOptions)
    {
        var result = content switch
        {
            { Exception: not null } => content.Exception.Message,
            { Result: JsonElement element } => element.GetRawText(),
            _ => content.Result,
        };

        var toolResult = new ToolResultContent(content.CallId, result);
        var json = JsonSerializer.Serialize(toolResult, jsonOptions);

        return new Message
        {
            MessageTypeId = MessageType.ToolResultId,
            Text = json,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };
    }

    public static Message ForDocument(int chatId, string fileName, JsonSerializerOptions options)
    {
        var content = new AddSourceContent(SourceType.Document, fileName);
        var json = JsonSerializer.Serialize(content, options);

        return new Message
        {
            MessageTypeId = MessageType.AddSource,
            Text = json,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };
    }

    public static Message ForWebSite(int chatId, string uri, JsonSerializerOptions options)
    {
        var content = new AddSourceContent(SourceType.WebSite, uri);
        var json = JsonSerializer.Serialize(content, options);

        return new Message
        {
            MessageTypeId = MessageType.AddSource,
            Text = json,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        };
    }

    public ChatMessage ToChatMessage()
    {
        var role = MessageTypeId switch
        {
            MessageType.SystemId => ChatRole.System,
            MessageType.AssistantAnswerId => ChatRole.Assistant,
            MessageType.UserContextId or MessageType.UserRequestId => ChatRole.User,

            _ => throw new InvalidOperationException("Invalid message type"),
        };

        return new ChatMessage(role, Text);
    }
}