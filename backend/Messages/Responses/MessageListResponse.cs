namespace Backend.Messages.Responses;

public record MessageListResponse(int Id, MessageKind Kind, string Text, DateTimeOffset Timestamp);