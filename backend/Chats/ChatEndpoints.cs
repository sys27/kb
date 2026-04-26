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
                    .OrderByDescending(x => x.Id)
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
            .Produces(404)
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
            .ProducesProblem(400)
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

        return app;
    }
}