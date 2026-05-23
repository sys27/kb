namespace Backend.Projects.Responses;

public record ProjectPreferenceListResponse(int Id, string Name, IEnumerable<IdNameResponse> UserPreferences);