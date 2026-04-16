namespace Backend.Messages.Responses;

public record MessageListResponse(int Id, string Role, string Kind, string Text, DateTimeOffset Timestamp);