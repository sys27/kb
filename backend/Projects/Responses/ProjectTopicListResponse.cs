namespace Backend.Projects.Responses;

public record ProjectTopicListResponse(int Id, string Topic, IdNameResponse? Chat);