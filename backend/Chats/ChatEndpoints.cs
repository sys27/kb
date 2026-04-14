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
                    .ToResponse()
                    .ToListAsync(cancellationToken);

                return chats;
            })
            .Produces<List<ChatListResponse>>()
            .ProducesProblem(500)
            .WithName("GetChats")
            .WithSummary("Get all chats");

        group.MapPost("", async (CreateChatRequest request, KbDbContext context, CancellationToken cancellationToken) =>
            {
                // TODO: Validate request
                var chat = request.ToEntity();
                await context.Chats.AddAsync(chat, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                return Results.Created($"/chats/{chat.Id}", chat.ToResponse());
            })
            .Produces<ChatListResponse>()
            .ProducesProblem(400)
            .ProducesProblem(500)
            .WithName("CreateChat")
            .WithSummary("Create a new chat");

        group.MapGet("/{id:int}", async (int id, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
                if (chat is null)
                    return Results.NotFound();

                return Results.Ok(chat.ToResponse());
            })
            .Produces<ChatListResponse>()
            .Produces(404)
            .ProducesProblem(500)
            .WithName("GetChat")
            .WithSummary("Get a chat by id");

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
            .Produces(204)
            .ProducesProblem(500)
            .WithName("DeleteChat")
            .WithSummary("Delete a chat by id");

        return app;
    }
}