using Backend.Chats;
using Backend.Messages.Tools.WebFetch;
using Backend.Messages.Tools.WebSearch;
using Microsoft.Extensions.AI;

namespace Backend.Messages.Pipelines;

public class SendRequest : IConversationPipelineStep
{
    private readonly IChatClient chatClient;
    private readonly WebSearchService webSearchService;
    private readonly WebFetchService webFetchService;

    public SendRequest(IChatClient chatClient, WebSearchService webSearchService, WebFetchService webFetchService)
    {
        this.chatClient = chatClient;
        this.webSearchService = webSearchService;
        this.webFetchService = webFetchService;
    }

    public Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        var chat = context.Get<Chat>("chat");
        var requestText = context.Get<string>("requestText");
        var enableWebSearch = context.Get<bool>("enableWebSearch");

        chat.AddMessage(Message.ForUser(chat.Id, requestText));

        // TODO: try developer role
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

        var chatOptions = new ChatOptions
        {
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = [],
        };

        if (enableWebSearch)
        {
            chatOptions.Tools.Add(
                AIFunctionFactory.Create(
                    typeof(WebSearchService).GetMethod(nameof(WebSearchService.Search))!,
                    webSearchService,
                    "web_search",
                    "Search the web for information on the given topic."));

            chatOptions.Tools.Add(
                AIFunctionFactory.Create(
                    typeof(WebFetchService).GetMethod(nameof(WebFetchService.Fetch))!,
                    webFetchService,
                    "web_fetch",
                    "Fetch the content of a web page at the given URL."));
        }

        var streamingResponse = chatClient.GetStreamingResponseAsync(
            chatMessages,
            chatOptions,
            cancellationToken);
        context.Set("streamingResponse", streamingResponse);

        return Task.CompletedTask;
    }
}