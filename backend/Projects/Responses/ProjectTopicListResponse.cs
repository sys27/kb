namespace Backend.Projects.Responses;

public record ProjectTopicListResponse(int Id, string Name, IEnumerable<IdNameResponse> Topics);