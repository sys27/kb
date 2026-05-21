using Backend.Chats.Requests;
using Backend.Chats.Responses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Chats;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/chats");

        group.MapGet("", async (KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chats = await context.Chats
                    .OrderBy(x => x.LastMessageAt != null)
                    .ThenByDescending(x => x.LastMessageAt)
                    .AsNoTracking()
                    .ToResponse()
                    .ToListAsync(cancellationToken);

                return chats;
            })
            .Produces<List<ChatListResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetChats")
            .WithSummary("Get all chats");

        group.MapGet("/{id:int}", async (int id, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
                if (chat is null)
                    return Results.NotFound();

                return Results.Ok(chat.ToResponse());
            })
            .Produces<ChatListResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("GetChat")
            .WithSummary("Get a chat by id");

        group.MapPost("", async (CreateChatRequest request, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chat = request.ToEntity();
                await context.Chats.AddAsync(chat, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return Results.Created($"/chats/{chat.Id}", chat.ToResponse());
            })
            .Produces<ChatListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("CreateChat")
            .WithSummary("Create a new chat");

        group.MapPut("/{id:int}", async (
                int id,
                UpdateChatRequest request,
                KbDbContext context,
                CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (chat is null)
                    return Results.NotFound();

                chat.Name = request.Name;
                chat.UpdateProject(request.ProjectId);
                await context.SaveChangesAsync(cancellationToken);

                var response = chat.ToResponse();

                return Results.Ok(response);
            })
            .Produces<ChatListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("UpdateChat")
            .WithSummary("Update a chat by id");

        group.MapDelete("/{id:int}", async (int id, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
                if (chat is not null)
                {
                    context.Chats.Remove(chat);
                    await context.SaveChangesAsync(cancellationToken);
                }

                return Results.NoContent();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithName("DeleteChat")
            .WithSummary("Delete a chat by id");

        group.MapPost("/import", async (
                IFormFile? file,
                ILoggerFactory loggerFactory,
                ChatGptImporter importer,
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
                await importer.Import(stream, cancellationToken);

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
}