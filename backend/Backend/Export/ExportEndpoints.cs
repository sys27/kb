using System.Text;
using System.Text.Json;
using Backend.Chats;
using Backend.Messages;
using Backend.Projects;
using Microsoft.EntityFrameworkCore;

namespace Backend.Export;

public static class ExportEndpoints
{
    public static IEndpointRouteBuilder MapExportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/export", async (
                KbDbContext context,
                CancellationToken cancellationToken) =>
            {
                var projects = await context.Projects
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var chats = await context.Chats
                    .Include(x => x.Messages)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var exportModel = new ExportModel(
                    projects.Select(x => new ProjectModel(x.Id, x.Name)).ToArray(),
                    chats.Select(x => new ChatModel(
                            x.Name,
                            x.ProjectId,
                            x.Messages.Select(m => new MessageModel(m.Text, m.MessageTypeId, m.Timestamp)).ToArray()))
                        .ToArray());

                var json = JsonSerializer.Serialize(exportModel);

                return Results.Bytes(
                    Encoding.UTF8.GetBytes(json),
                    "application/json",
                    "export.json",
                    true);
            })
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("ExportChats")
            .WithSummary("Export chats to a file");

        app.MapPost("/import", async (
                IFormFile? file,
                ILoggerFactory loggerFactory,
                KbDbContext context,
                CancellationToken cancellationToken) =>
            {
                var logger = loggerFactory.CreateLogger("ChatImport");

                if (file is null)
                {
                    logger.LogWarning("No file was uploaded.");
                    return Results.BadRequest();
                }

                if (file.Length == 0)
                {
                    logger.LogWarning("'{FileFileName}' is empty.", file.FileName);
                    return Results.BadRequest();
                }

                if (file.ContentType != "application/json")
                {
                    logger.LogWarning("'{FileFileName}' is not a JSON file.", file.FileName);
                    return Results.BadRequest();
                }

                await using var stream = file.OpenReadStream();
                var exportModel = JsonSerializer.Deserialize<ExportModel>(stream);
                if (exportModel is null)
                {
                    logger.LogError("Failed to deserialize JSON.");
                    return Results.BadRequest();
                }

                var projectIdMap = new Dictionary<int, int>(exportModel.Projects.Count);

                foreach (var projectToImport in exportModel.Projects)
                {
                    var project = new Project
                    {
                        Name = projectToImport.Name,
                    };

                    await context.Projects.AddAsync(project, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);

                    projectIdMap[projectToImport.Id] = project.Id;
                }

                foreach (var chatToImport in exportModel.Chats)
                {
                    var chat = new Chat
                    {
                        Name = chatToImport.Name,
                        ProjectId = chatToImport.ProjectId is not null
                            ? projectIdMap[chatToImport.ProjectId.Value]
                            : null,
                    };

                    foreach (var messageToImport in chatToImport.Messages)
                    {
                        var message = new Message
                        {
                            Text = messageToImport.Text,
                            MessageTypeId = messageToImport.MessageTypeId,
                            Timestamp = messageToImport.Timestamp,
                        };

                        chat.AddMessage(message);
                    }

                    await context.Chats.AddAsync(chat, cancellationToken);
                }

                await context.SaveChangesAsync(cancellationToken);

                return Results.Ok();
            })
            .DisableAntiforgery()
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("ImportChats")
            .WithSummary("Import chats from a file");

        return app;
    }

    private record ExportModel(IReadOnlyList<ProjectModel> Projects, IReadOnlyList<ChatModel> Chats);

    private record ProjectModel(int Id, string Name);

    private record ChatModel(string Name, int? ProjectId, IReadOnlyList<MessageModel> Messages);

    private record MessageModel(string Text, int MessageTypeId, DateTime Timestamp);
}