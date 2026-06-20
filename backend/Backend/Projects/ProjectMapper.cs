using Backend.Chats;
using Backend.Ingestion;
using Backend.Projects.Requests;
using Backend.Projects.Responses;
using Riok.Mapperly.Abstractions;

namespace Backend.Projects;

[Mapper(
    EnumMappingIgnoreCase = true,
    EnumMappingStrategy = EnumMappingStrategy.ByValueCheckDefined,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ProjectMapper
{
    public static partial ProjectListResponse ToResponse(this Project project);

    public static partial IQueryable<ProjectListResponse> ToResponse(this IQueryable<Project> project);

    [MapperIgnoreTarget(nameof(Project.Id))]
    [MapperIgnoreTarget(nameof(Project.Chats))]
    [MapperIgnoreTarget(nameof(Project.Documents))]
    public static partial Project ToEntity(this CreateProjectRequest request);

    public static partial IQueryable<ProjectDocumentListResponse> ToResponse(this IQueryable<Document> document);

    [MapProperty(nameof(ChatTopic.Topic), nameof(IdNameResponse.Name))]
    public static partial IdNameResponse ToResponse(ChatTopic topic);

    public static partial IQueryable<ProjectTopicListResponse> ToTopicsResponse(this IQueryable<Chat> chats);

    [MapProperty(nameof(ChatFact.Fact), nameof(IdNameResponse.Name))]
    public static partial IdNameResponse ToResponse(ChatFact topic);

    public static partial IQueryable<ProjectFactListResponse> ToFactResponse(this IQueryable<Chat> facts);

    public static partial IQueryable<ProjectDecisionListResponse> ToDecisionResponse(this IQueryable<Chat> decisions);

    [MapProperty(nameof(ChatUserPreference.Preference), nameof(IdNameResponse.Name))]
    public static partial IdNameResponse ToResponse(ChatUserPreference topic);

    public static partial IQueryable<ProjectPreferenceListResponse> ToPreferenceResponse(this IQueryable<Chat> preferences);
}