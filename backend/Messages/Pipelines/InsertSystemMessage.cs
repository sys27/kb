using Backend.Chats;

namespace Backend.Messages.Pipelines;

public class InsertSystemMessage : IConversationPipelineStep
{
    public Task ExecuteAsync(
        ConversationPipelineContext context,
        CancellationToken cancellationToken = default)
    {
        var chat = context.Get<Chat>("chat");
        if (chat.Messages.Count == 0 || chat.Messages.All(m => m.MessageTypeId != MessageType.SystemId))
            chat.InsertMessage(0, Message.ForSystem(chat.Id, "You are a helpful assistant."));

        return Task.CompletedTask;
    }
}