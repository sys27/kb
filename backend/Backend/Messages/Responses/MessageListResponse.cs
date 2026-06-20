namespace Backend.Messages.Responses;

public record MessageListResponse(int Id, int MessageTypeId, string Text, DateTime Timestamp);