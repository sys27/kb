using System.Diagnostics;
using System.Text;
using Backend.Messages.Requests;
using Backend.Messages.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

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
                IChatClient chatClient,
                HttpResponse httpResponse,
                CancellationToken cancellationToken) =>
            {
                var message = Message.ForUser(chatId, request.Text);
                await context.Messages.AddAsync(message, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                var messages = await context.Messages
                    .Where(m => m.ChatId == chatId)
                    .ToListAsync(cancellationToken);
                var chatMessages = messages.Select(x =>
                    new ChatMessage(
                        x.Role switch
                        {
                            MessageRole.System => ChatRole.System,
                            MessageRole.User => ChatRole.User,
                            MessageRole.Assistant => ChatRole.Assistant,
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        x.Text));

                var streamingResponse = chatClient.GetStreamingResponseAsync(chatMessages, null, cancellationToken);
                await httpResponse.WriteSseHeaders(cancellationToken);

                var reasoningResponse = new StringBuilder();
                var finalResponse = new StringBuilder();
                await foreach (var chatResponse in streamingResponse)
                {
                    Debug.Assert(chatResponse.Role == ChatRole.Assistant);

                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var content in chatResponse.Contents)
                    {
                        if (content is TextReasoningContent textReasoning)
                        {
                            reasoningResponse.Append(textReasoning.Text);
                            await httpResponse.WriteSse(textReasoning.Text, cancellationToken);
                        }
                        else if (content is TextContent text)
                        {
                            finalResponse.Append(text.Text);
                            await httpResponse.WriteSse(text.Text, cancellationToken);
                        }
                        else
                        {
                            Debug.WriteLine($"Skipping content type: {content.GetType().Name}");
                        }
                    }
                }

                if (reasoningResponse.Length > 0)
                {
                    message = Message.ForReasoning(chatId, reasoningResponse.ToString());
                    await context.Messages.AddAsync(message, cancellationToken);
                }

                message = Message.ForAssistant(chatId, finalResponse.ToString());
                await context.Messages.AddAsync(message, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            })
            .Produces(StatusCodes.Status200OK, null, "text/event-stream")
            .ProducesProblem(400)
            .ProducesProblem(500)
            .WithName("SendMessage")
            .WithSummary("Send a message to a chat");

        return app;
    }

    private static async Task WriteSseHeaders(this HttpResponse response, CancellationToken cancellationToken)
    {
        response.Headers.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";
        await response.Body.FlushAsync(cancellationToken);
    }

    private static async Task WriteSse(
        this HttpResponse response,
        string text,
        CancellationToken cancellationToken)
    {
        await response.WriteAsync("data: ", cancellationToken);
        await response.WriteAsync(text, cancellationToken);
        await response.WriteAsync("\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}