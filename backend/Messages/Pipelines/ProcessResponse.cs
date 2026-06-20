using System.Text;
using System.Text.Json;
using Backend.Chats;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Backend.Messages.Pipelines;

public class ProcessResponse : IConversationPipelineStep
{
    private readonly ILogger<ProcessResponse> logger;
    private readonly KbDbContext dbContext;
    private readonly HttpContext httpContext;
    private readonly JsonOptions jsonOptions;

    public ProcessResponse(
        ILogger<ProcessResponse> logger,
        IHttpContextAccessor httpContextAccessor,
        IOptions<JsonOptions> jsonOptions,
        KbDbContext dbContext)
    {
        this.logger = logger;
        this.httpContext = httpContextAccessor.HttpContext ??
                           throw new ArgumentNullException(nameof(httpContextAccessor));

        this.jsonOptions = jsonOptions.Value;
        this.dbContext = dbContext;
    }

    public async Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        await WriteSseHeaders(cancellationToken);

        var chat = context.Get<Chat>("chat");
        var streamingResponse = context.Get<IAsyncEnumerable<ChatResponseUpdate>>("streamingResponse");

        var messages = new List<Message>();
        var reasoningResponse = new StringBuilder();
        var finalResponse = new StringBuilder();
        await foreach (var chatResponse in streamingResponse.WithCancellation(cancellationToken))
        {
            foreach (var content in chatResponse.Contents)
            {
                if (content is TextReasoningContent textReasoning)
                {
                    reasoningResponse.Append(textReasoning.Text);
                    await WriteReasoning(textReasoning.Text, cancellationToken);
                }
                else if (content is TextContent text)
                {
                    finalResponse.Append(text.Text);
                    await WriteText(text.Text, cancellationToken);
                }
                else if (content is FunctionCallContent functionCall)
                {
                    var toolCallMessage = Message.ForToolCall(chat.Id, functionCall, jsonOptions.SerializerOptions);
                    messages.Add(toolCallMessage);

                    await WriteToolCall(toolCallMessage.Text, cancellationToken);
                }
                else if (content is FunctionResultContent functionResult)
                {
                    var toolResultMessage = Message.ForToolResult(chat.Id, functionResult, jsonOptions.SerializerOptions);
                    messages.Add(toolResultMessage);

                    await WriteToolResult(toolResultMessage.Text, cancellationToken);
                }
                else
                {
                    logger.LogWarning("Skipping unknown content type: {ContentType}", content.GetType().Name);
                }
            }
        }

        foreach (var message in messages)
            chat.AddMessage(message);

        if (reasoningResponse.Length > 0)
            chat.AddMessage(Message.ForReasoning(chat.Id, reasoningResponse.ToString()));

        if (finalResponse.Length > 0)
            chat.AddMessage(Message.ForAssistant(chat.Id, finalResponse.ToString()));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task WriteSseHeaders(CancellationToken cancellationToken)
    {
        httpContext.Response.Headers.ContentType = "text/event-stream";
        httpContext.Response.Headers.CacheControl = "no-cache,no-store";
        httpContext.Response.Headers.Pragma = "no-cache";
        httpContext.Response.Headers.Connection = "keep-alive";
        httpContext.Response.Headers.ContentEncoding = "identity";

        await httpContext.Response.Body.FlushAsync(cancellationToken);
    }

    private async Task WriteSse<T>(T message, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(message, jsonOptions.SerializerOptions);

        await httpContext.Response.WriteAsync("data: ", cancellationToken);
        await httpContext.Response.WriteAsync(json, cancellationToken);
        await httpContext.Response.WriteAsync("\n\n", cancellationToken);
        await httpContext.Response.Body.FlushAsync(cancellationToken);
    }

    private async Task WriteReasoning(string text, CancellationToken cancellationToken)
    {
        var message = new MessageSse(MessageType.AssistantReasoningId, text);

        await WriteSse(message, cancellationToken);
    }

    private async Task WriteText(string text, CancellationToken cancellationToken)
    {
        var message = new MessageSse(MessageType.AssistantAnswerId, text);

        await WriteSse(message, cancellationToken);
    }

    private async Task WriteToolCall(string text, CancellationToken cancellationToken)
    {
        var message = new MessageSse(MessageType.ToolCallId, text);

        await WriteSse(message, cancellationToken);
    }

    private async Task WriteToolResult(string text, CancellationToken cancellationToken)
    {
        var message = new MessageSse(MessageType.ToolResultId, text);

        await WriteSse(message, cancellationToken);
    }

    private readonly record struct MessageSse(int MessageTypeId, string Text);
}