using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.AI;

namespace Backend.Messages.Pipelines;

public class ProcessResponse : IConversationPipelineStep
{
    private readonly HttpResponse httpResponse;

    public ProcessResponse(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext is null)
            throw new ArgumentNullException(nameof(httpContextAccessor));

        this.httpResponse = httpContextAccessor.HttpContext.Response;
    }

    public async Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        await WriteSseHeaders(httpResponse, cancellationToken);

        var streamingResponse = context.Get<IAsyncEnumerable<ChatResponseUpdate>>("streamingResponse");

        var reasoningResponse = new StringBuilder();
        var toolResponse = new StringBuilder();
        var finalResponse = new StringBuilder();
        await foreach (var chatResponse in streamingResponse)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var content in chatResponse.Contents)
            {
                if (content is TextReasoningContent textReasoning)
                {
                    reasoningResponse.Append(textReasoning.Text);
                    await WriteSse(httpResponse, textReasoning.Text, cancellationToken);
                }
                else if (content is TextContent text)
                {
                    finalResponse.Append(text.Text);
                    await WriteSse(httpResponse, text.Text, cancellationToken);
                }
                else if (content is FunctionCallContent functionCall)
                {
                    var toolCall = $"Calling tool: {functionCall.Name}({functionCall.Arguments})";
                    toolResponse.AppendLine(toolCall);
                    await WriteSse(httpResponse, toolCall, cancellationToken);
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

    private static async Task WriteSseHeaders(HttpResponse response, CancellationToken cancellationToken)
    {
        response.Headers.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";
        await response.Body.FlushAsync(cancellationToken);
    }

    private static async Task WriteSse(HttpResponse response, string text, CancellationToken cancellationToken)
    {
        await response.WriteAsync("data: ", cancellationToken);
        await response.WriteAsync(text, cancellationToken);
        await response.WriteAsync("\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}