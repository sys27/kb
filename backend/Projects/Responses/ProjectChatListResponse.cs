namespace Backend.Projects.Responses;

public record ProjectChatListResponse(int Id, string Name, string? LastMessage, DateTime? LastMessageAt);