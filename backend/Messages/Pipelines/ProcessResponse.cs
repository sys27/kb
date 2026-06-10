using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace Backend.Messages.Pipelines;

public class ProcessResponse : IConversationPipelineStep
{
    private readonly ILogger<ProcessResponse> logger;
    private readonly HttpContext httpContext;
    private readonly JsonOptions jsonOptions;

    public ProcessResponse(
        ILogger<ProcessResponse> logger,
        IHttpContextAccessor httpContextAccessor,
        IOptions<JsonOptions> jsonOptions)
    {
        this.logger = logger;
        this.httpContext = httpContextAccessor.HttpContext ??
                           throw new ArgumentNullException(nameof(httpContextAccessor));

        this.jsonOptions = jsonOptions.Value;
    }

    public async Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        await WriteSseHeaders(cancellationToken);

        var streamingResponse = context.Get<IAsyncEnumerable<ChatResponseUpdate>>("streamingResponse");

        var reasoningResponse = new StringBuilder();
        var finalResponse = new StringBuilder();
        var toolCallResponse = new StringBuilder();
        var toolResultResponse = new StringBuilder();
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
                    var toolCallJson = GetToolCall(functionCall);
                    toolCallResponse.AppendLine(toolCallJson);
                    await WriteToolCall(toolCallJson, cancellationToken);
                }
                else if (content is FunctionResultContent functionResult)
                {
                    var toolResultJson = GetToolResult(functionResult);
                    toolResultResponse.AppendLine(toolResultJson);
                    await WriteToolResult(toolResultJson, cancellationToken);
                }
                else
                {
                    logger.LogWarning("Skipping unknown content type: {ContentType}", content.GetType().Name);
                }
            }
        }

        context.Set("reasoningResponse", reasoningResponse.ToString());
        context.Set("finalResponse", finalResponse.ToString());
        context.Set("toolCallResponse", toolCallResponse.ToString());
        context.Set("toolResultResponse", toolResultResponse.ToString());
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

    private string GetToolCall(FunctionCallContent functionCall)
    {
        var arguments = new Dictionary<string, string>();
        if (functionCall.Arguments is not null)
            foreach (var (parameter, value) in functionCall.Arguments)
                if (value is JsonElement element)
                    arguments[parameter] = element.GetRawText();

        var exception = functionCall.Exception?.Message;

        var toolCall = new
        {
            callId = functionCall.CallId,
            function = functionCall.Name,
            arguments,
            exception,
        };
        var json = JsonSerializer.Serialize(toolCall, jsonOptions.SerializerOptions);

        return json;
    }

    private string GetToolResult(FunctionResultContent functionResult)
    {
        var result = functionResult switch
        {
            { Exception: not null } => functionResult.Exception.Message,
            { Result: JsonElement element } => element.GetRawText(),
            _ => functionResult.Result,
        };

        var toolResult = new
        {
            callId = functionResult.CallId,
            result,
        };
        var json = JsonSerializer.Serialize(toolResult, jsonOptions.SerializerOptions);

        return json;
    }

    private readonly record struct MessageSse(int MessageTypeId, string Text);
}