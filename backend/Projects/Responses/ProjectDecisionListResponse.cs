namespace Backend.Projects.Responses;

public record ProjectDecisionListResponse(
    int Id,
    string Decision,
    string Reason,
    IdNameResponse? Chat);