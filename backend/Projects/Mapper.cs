using Backend.Projects.Responses;
using Riok.Mapperly.Abstractions;

namespace Backend.Projects;

[Mapper(
    EnumMappingIgnoreCase = true,
    EnumMappingStrategy = EnumMappingStrategy.ByValue,
    RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class Mapper
{
    public static partial ProjectListResponse ToResponse(this Project project);

    public static partial IQueryable<ProjectListResponse> ToResponse(this IQueryable<Project> project);
}