namespace Backend.Projects.Responses;

public record ProjectPreferenceListResponse(int Id, string Preference, IdNameResponse? Chat);