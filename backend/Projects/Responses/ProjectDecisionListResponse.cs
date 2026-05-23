namespace Backend.Projects.Responses;

public record ProjectDecisionListResponse(int Id, string Name, IEnumerable<ProjectDecisionResponse> Decisions);