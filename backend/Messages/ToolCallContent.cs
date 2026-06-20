namespace Backend.Messages;

public record ToolCallContent(
    string CallId,
    string Function,
    Dictionary<string, string> Arguments,
    string? Exception);