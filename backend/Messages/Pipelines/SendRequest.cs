using Backend.Chats;
using Microsoft.Extensions.AI;

namespace Backend.Messages.Pipelines;

public class SendRequest : IConversationPipelineStep
{
    private readonly IChatClient chatClient;

    public SendRequest(IChatClient chatClient)
        => this.chatClient = chatClient;

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
            // TODO: implement web search tool
            chatOptions.Tools.Add(
                AIFunctionFactory.Create(
                    (string query) => "",
                    "web_search",
                    "Search the web for information on the given topic."));
        }

        var streamingResponse = chatClient.GetStreamingResponseAsync(
            chatMessages,
            chatOptions,
            cancellationToken);
        context.Set("streamingResponse", streamingResponse);

        return Task.CompletedTask;
    }
}