using Backend.Ingestion;
using Backend.Projects.Requests;
using Backend.Projects.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Projects;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects");

        group
            .MapProjects()
            .MapProjectChats()
            .MapProjectDocuments();

        return app;
    }

    private static IEndpointRouteBuilder MapProjects(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("", async (KbDbContext context, CancellationToken cancellationToken) =>
            {
                var projects = await context.Projects
                    .AsNoTracking()
                    .ToResponse()
                    .ToListAsync(cancellationToken);

                return Results.Ok(projects);
            })
            .Produces<List<ProjectListResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProjects")
            .WithSummary("Get all projects");

        builder.MapGet("{projectId:int}", async (
                int projectId,
                KbDbContext context,
                CancellationToken cancellationToken) =>
            {
                var project = await context.Projects
                    .AsNoTracking()
                    .ToResponse()
                    .FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);

                if (project is null)
                    return Results.NotFound();

                return Results.Ok(project);
            })
            .Produces<ProjectListResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProject")
            .WithSummary("Get project by ID");

        builder.MapPost("", async (
                CreateProjectRequest request,
                KbDbContext context,
                IOptions<IngestionOptions> ingestionOptions,
                CancellationToken cancellationToken) =>
            {
                var project = request.ToEntity();
                await context.Projects.AddAsync(project, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                var directoryName = project.GetDirectoryName();
                var directoryPath = Path.Combine(ingestionOptions.Value.Path, directoryName);
                Directory.CreateDirectory(directoryPath);

                return Results.CreatedAtRoute("GetProject", new { projectId = project.Id }, project);
            })
            .Produces<ProjectListResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("CreateProject")
            .WithSummary("Create a new project");

        builder.MapPut("{projectId:int}", async (
                int projectId,
                UpdateProjectRequest request,
                KbDbContext context,
                CancellationToken cancellationToken) =>
            {
                var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);
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

        builder.MapDelete("{projectId:int}", async (
                int projectId,
                KbDbContext context,
                CancellationToken cancellationToken) =>
            {
                var project = await context.Projects.FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);
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

        return builder;
    }

    private static IEndpointRouteBuilder MapProjectChats(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/{projectId:int}/chats");

        group.MapGet("", async (int projectId, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chats = await context.Chats
                    .Where(x => x.ProjectId == projectId)
                    .Select(x => new ProjectChatListResponse(
                        x.Id,
                        x.Name,
                        x.Messages.OrderByDescending(m => m.Id).FirstOrDefault()!.Text,
                        x.LastMessageAt))
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return Results.Ok(chats);
            })
            .Produces<List<ProjectChatListResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProjectChats")
            .WithSummary("Get all chats for a project");

        return builder;
    }

    private static IEndpointRouteBuilder MapProjectDocuments(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/{projectId:int}/documents");

        group.MapGet("", async (int projectId, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var documents = await context.Documents
                    .Where(d => d.ProjectId == projectId)
                    .AsNoTracking()
                    .ToResponse()
                    .ToListAsync(cancellationToken);

                return Results.Ok(documents);
            })
            .Produces<List<ProjectDocumentListResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetProjectDocuments")
            .WithSummary("Get all documents for a project");

        group.MapPost("/upload", async (
                int projectId,
                ILoggerFactory loggerFactory,
                KbDbContext context,
                IOptions<IngestionOptions> ingestionOptions,
                IFormFile? file,
                CancellationToken cancellationToken) =>
            {
                var logger = loggerFactory.CreateLogger("DocumentUpload");
                if (file is null || file.Length == 0)
                {
                    logger.LogWarning("File is empty");
                    return Results.BadRequest();
                }

                var project = await context.Projects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);

                if (project is null)
                {
                    logger.LogWarning("Project not found: {projectId}", projectId);
                    return Results.NotFound();
                }

                var filePath = Path.Combine(ingestionOptions.Value.Path, project.GetDirectoryName(), file.FileName);
                if (File.Exists(filePath))
                {
                    logger.LogWarning("File already exists: {filePath}", filePath);
                    return Results.BadRequest();
                }

                await using var stream = File.Open(filePath, FileMode.CreateNew, FileAccess.Write);
                await file.CopyToAsync(stream, cancellationToken);

                return Results.Ok();
            })
            .DisableAntiforgery()
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("UploadProjectDocument")
            .WithSummary("Upload a new document for a project");

        return builder;
    }
}