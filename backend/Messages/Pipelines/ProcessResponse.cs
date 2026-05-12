using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Backend.Messages.Pipelines;

public class ProcessResponse : IConversationPipelineStep
{
    private readonly HttpContext httpContext;
    private readonly JsonOptions jsonOptions;

    public ProcessResponse(IHttpContextAccessor httpContextAccessor, IOptions<JsonOptions> jsonOptions)
    {
        this.httpContext = httpContextAccessor.HttpContext ??
                           throw new ArgumentNullException(nameof(httpContextAccessor));

        this.jsonOptions = jsonOptions.Value;
    }

    public async Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        await WriteSseHeaders(cancellationToken);

        var streamingResponse = context.Get<IAsyncEnumerable<ChatResponseUpdate>>("streamingResponse");

        var reasoningResponse = new StringBuilder();
        var toolResponse = new StringBuilder();
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
                    var toolCall = $"Calling tool: {functionCall.Name}({functionCall.Arguments})";
                    toolResponse.AppendLine(toolCall);
                    // TODO: write tool call to SSE
                    // await WriteSse(toolCall, cancellationToken);
                }
                else
                {
                    Debug.WriteLine($"Skipping content type: {content.GetType().Name}");
                }
            }
        }

        context.Set("reasoningResponse", reasoningResponse.ToString());
        context.Set("toolResponse", toolResponse.ToString());
        context.Set("finalResponse", finalResponse.ToString());
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

    private readonly record struct MessageSse(int MessageTypeId, string Text);
}