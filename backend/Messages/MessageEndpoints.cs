using Backend.Messages.Requests;
using Backend.Messages.Responses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Messages;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/chat/{chatId:int}/messages");

        group.MapGet("", async (int chatId, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var messages = await context.Messages
                    .Where(m => m.ChatId == chatId)
                    .OrderByDescending(m => m.Timestamp)
                    .ToResponse()
                    .ToListAsync(cancellationToken);

                return Results.Ok(messages);
            })
            .Produces<List<MessageListResponse>>()
            .ProducesProblem(500)
            .WithName("GetMessages")
            .WithSummary("Get all messages for a chat");

        group.MapPost("", async (
                int chatId,
                SendMessageRequest request,
                KbDbContext context,
                CancellationToken cancellationToken) =>
            {
                var message = new Message
                {
                    Kind = MessageKind.User,
                    Text = request.Text,
                    ChatId = chatId,
                    Timestamp = DateTimeOffset.UtcNow
                };
                await context.Messages.AddAsync(message, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                var response = message.ToResponse();
                return Results.Ok(response);
            })
            .Produces<MessageListResponse>()
            .ProducesProblem(400)
            .ProducesProblem(500)
            .WithName("SendMessage")
            .WithSummary("Send a message to a chat");

        return app;
    }
}