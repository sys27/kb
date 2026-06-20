namespace Backend.Projects.Responses;

public record ProjectDocumentListResponse(int Id, string Name, DateTime? LastModifiedAt, string Status);