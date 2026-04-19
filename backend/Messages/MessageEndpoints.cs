using System.Diagnostics;
using System.Text;
using Backend.Ingestion;
using Backend.Messages.Requests;
using Backend.Messages.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

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
                    .OrderByDescending(m => m.Id)
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
                VectorStore vectorStore,
                IChatClient chatClient,
                HttpResponse httpResponse,
                CancellationToken cancellationToken) =>
            {
                var chat = await context.Chats
                    .Include(x => x.Messages)
                    .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
                if (chat is null)
                    return Results.NotFound();

                if (chat.Messages.Count == 0)
                {
                    var systemMessage = Message.ForSystem(chatId, "You are a helpful assistant.");
                    chat.AddMessage(systemMessage);
                }

                // TODO: use LLM to generate query?
                var collection = vectorStore.GetCollection<int, DocumentChunk>("DocumentChunks");
                var vectorSearchOptions = new VectorSearchOptions<DocumentChunk>
                {
                    ScoreThreshold = 0.5, // TODO: configurable?
                };
                var similarDocuments = await collection
                    .SearchAsync(request.Text, 3, vectorSearchOptions, cancellationToken)
                    .ToListAsync(cancellationToken);

                if (similarDocuments.Count > 0)
                {
                    var contextPrompt = new StringBuilder(
                        """
                        Use the provided context to answer the user.
                        If the context is insufficient, say you don't know.

                        <context>

                        """);

                    for (var i = 0; i < similarDocuments.Count; i++)
                    {
                        contextPrompt
                            .Append('[')
                            .Append(i + 1)
                            .Append("] ")
                            .AppendLine(similarDocuments[i].Record.Content);
                    }

                    contextPrompt.Append("</context>");

                    // TODO: use other role?
                    var similarDocumentsMessage = Message.ForUser(chatId, contextPrompt.ToString());
                    chat.AddMessage(similarDocumentsMessage);
                }

                // TODO: BM25
                var userMessage = Message.ForUser(chatId, request.Text);
                chat.AddMessage(userMessage);

                var chatMessages = chat.Messages
                    .Where(x => x.Kind != MessageKind.Reasoning)
                    .Select(x => new ChatMessage(
                        x.Role switch
                        {
                            MessageRole.System => ChatRole.System,
                            MessageRole.User => ChatRole.User,
                            MessageRole.Assistant => ChatRole.Assistant,
                            MessageRole.Tool => ChatRole.Tool,
                            _ => throw new ArgumentOutOfRangeException(),
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
                    var reasoningMessage = Message.ForReasoning(chatId, reasoningResponse.ToString());
                    chat.AddMessage(reasoningMessage);
                }

                var assistantMessage = Message.ForAssistant(chatId, finalResponse.ToString());
                chat.AddMessage(assistantMessage);

                await context.SaveChangesAsync(cancellationToken);

                return Results.Empty;
            })
            .Produces(StatusCodes.Status200OK, null, "text/event-stream")
            .ProducesProblem(400)
            .ProducesProblem(404)
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