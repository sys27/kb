namespace Backend.Messages;

public class MessageType
{
    public const int SystemId = 1;
    public const int AssistantReasoningId = 2;
    public const int AssistantAnswerId = 3;
    public const int UserContextId = 4;
    public const int UserRequestId = 5;
    public const int ToolCallId = 6;
    public const int ToolResultId = 7;
    public const int AddSource = 8;

    public required int Id { get; init; }

    public required string Role { get; init; }

    public required string Kind { get; init; }

    public ICollection<Message> Messages { get; init; } = [];
}