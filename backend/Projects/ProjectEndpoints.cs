using Backend.Projects.Responses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Projects;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects");

        group.MapGet("", async (KbDbContext context, CancellationToken cancellationToken) =>
            {
                var projects = await context.Projects
                    .ToResponse()
                    .ToListAsync(cancellationToken);

                return Results.Ok(projects);
            })
            .Produces<List<ProjectListResponse>>()
            .ProducesProblem(500)
            .WithName("GetProjects")
            .WithSummary("Get all projects");

        return app;
    }
}