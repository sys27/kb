namespace Backend.Projects.Responses;

public record ProjectFactListResponse(int Id, string Name, IEnumerable<IdNameResponse> Facts);