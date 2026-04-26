using Backend.Projects.Requests;
using Backend.Projects.Responses;
using Riok.Mapperly.Abstractions;

namespace Backend.Projects;

[Mapper(
    EnumMappingIgnoreCase = true,
    EnumMappingStrategy = EnumMappingStrategy.ByValueCheckDefined,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class Mapper
{
    public static partial ProjectListResponse ToResponse(this Project project);

    public static partial IQueryable<ProjectListResponse> ToResponse(this IQueryable<Project> project);

    [MapperIgnoreTarget(nameof(Project.Id))]
    [MapperIgnoreTarget(nameof(Project.Chats))]
    [MapperIgnoreTarget(nameof(Project.Documents))]
    public static partial Project ToEntity(this CreateProjectRequest request);
}