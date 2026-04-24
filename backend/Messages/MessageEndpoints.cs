using Backend.Messages.Pipelines;
using Backend.Messages.Requests;
using Backend.Messages.Responses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Messages;

public static class MessageEndpoints
{
    public static IEndpointRouteBuilder MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/chats/{chatId:int}/messages");

        group.MapGet("", async (int chatId, KbDbContext context, CancellationToken cancellationToken) =>
            {
                var messages = await context.Messages
                    .Where(m => m.ChatId == chatId)
                    .OrderBy(m => m.Id)
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
                ConversationPipeline conversationPipeline,
                CancellationToken cancellationToken) =>
            {
                var pipelineContext = new ConversationPipelineContext();
                pipelineContext.Set("chatId", chatId);
                pipelineContext.Set("requestText", request.Text);
                pipelineContext.Set("enableWebSearch", request.EnableWebSearch);

                await conversationPipeline.Run(pipelineContext, cancellationToken);
            })
            .Produces(StatusCodes.Status200OK, null, "text/event-stream")
            .ProducesProblem(400)
            .ProducesProblem(404)
            .ProducesProblem(500)
            .WithName("SendMessage")
            .WithSummary("Send a message to a chat");

        return app;
    }
}