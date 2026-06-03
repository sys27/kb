using Backend.Chats;
using Backend.Messages.Tools.WebSearch;
using Microsoft.Extensions.AI;

namespace Backend.Messages.Pipelines;

public class SendRequest : IConversationPipelineStep
{
    private readonly IChatClient chatClient;
    private readonly WebSearchService webSearchService;

    public SendRequest(IChatClient chatClient, WebSearchService webSearchService)
    {
        this.chatClient = chatClient;
        this.webSearchService = webSearchService;
    }

    public Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        var chat = context.Get<Chat>("chat");
        var requestText = context.Get<string>("requestText");
        var enableWebSearch = context.Get<bool>("enableWebSearch");

        chat.AddMessage(Message.ForUserRequest(chat.Id, requestText));

        var chatMessages = chat.Messages
            .Where(x => x.MessageTypeId != MessageType.AssistantReasoningId)
            .Select(x => x.ToChatMessage());

        var chatOptions = new ChatOptions
        {
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = [],
        };

        if (enableWebSearch)
            AddWebSearchTool(chatOptions);

        var streamingResponse = chatClient.GetStreamingResponseAsync(
            chatMessages,
            chatOptions,
            cancellationToken);
        context.Set("streamingResponse", streamingResponse);

        return Task.CompletedTask;
    }

    private void AddWebSearchTool(ChatOptions chatOptions)
    {
        chatOptions.Instructions =
            """
            You have access to two tools:

            1. web_search(query)
               - Use to find relevant web pages.
               - Returns a list of results with title, url, and chunks (extracted relevant text).

            Use web_search when:
            - The question requires up-to-date information
            - The answer is not in the provided context
            - The topic is likely external or unknown

            Do NOT use web_search if:
            - The answer is already known or provided
            - The question is simple or conversational

            When calling web_search:
            - Rewrite the user query into a concise, specific search query
            - Prefer keywords over full sentences
            - Include technical terms when relevant

            You may:
            - Perform at most 3 web_search calls

            If results are insufficient:
            - Refine the query and try again
            - Otherwise, answer with best available information

            After retrieving content:
            - Extract the relevant information
            - Combine information across sources if needed

            When using web content:
            - Prefer facts from retrieved pages
            - Avoid hallucinating missing details

            When possible, reference the source URLs in your answer.
            """;

        chatOptions.Tools!.Add(
            AIFunctionFactory.Create(
                typeof(WebSearchService).GetMethod(nameof(WebSearchService.Search))!,
                webSearchService,
                "web_search",
                "Search the web for information on the given topic."));
    }
}