using Backend.Projects.Requests;
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
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProjects")
            .WithSummary("Get all projects");

        group.MapGet("{id:int}", async (int id, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var project = await context.Projects
                    .ToResponse()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (project is null)
                    return Results.NotFound();

                return Results.Ok(project);
            })
            .Produces<ProjectListResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProject")
            .WithSummary("Get project by ID");

        group.MapPost("", async (CreateProjectRequest request, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var project = request.ToEntity();
                await context.Projects.AddAsync(project, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return Results.CreatedAtRoute("GetProject", new { id = project.Id }, project);
            })
            .Produces<ProjectListResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("CreateProject")
            .WithSummary("Create a new project");

        group.MapPut("{id:int}", async (
                int id,
                UpdateProjectRequest request,
                KbDbContext context,
                CancellationToken cancellationToken) =>
            {
                var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (project is null)
                    return Results.NotFound();

                project.Name = request.Name;
                await context.SaveChangesAsync(cancellationToken);

                var response = project.ToResponse();

                return Results.Ok(response);
            })
            .Produces<ProjectListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("UpdateProject")
            .WithSummary("Update an existing project");

        group.MapDelete("{id:int}", async (int id, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (project is not null)
                {
                    context.Projects.Remove(project);
                    await context.SaveChangesAsync(cancellationToken);
                }

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("DeleteProject")
            .WithSummary("Delete a project by ID");

        return app;
    }
}