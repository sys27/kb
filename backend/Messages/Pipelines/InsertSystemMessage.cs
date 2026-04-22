using Backend.Chats;

namespace Backend.Messages.Pipelines;

public class InsertSystemMessage : IConversationPipelineStep
{
    public Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        var chat = context.Get<Chat>("chat");
        if (chat.Messages.Count == 0)
            chat.AddMessage(Message.ForSystem(chat.Id, "You are a helpful assistant."));

        return Task.CompletedTask;
    }
}