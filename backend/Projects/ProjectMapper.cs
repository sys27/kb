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

    public static partial ProjectDocumentListResponse ToResponse(this Document document);

    public static partial IQueryable<ProjectDocumentListResponse> ToResponse(this IQueryable<Document> document);
}