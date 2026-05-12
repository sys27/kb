using Backend.Chats;

namespace Backend.Messages.Pipelines;

public class AddResponsesToMessages : IConversationPipelineStep
{
    private readonly KbDbContext dbContext;

    public AddResponsesToMessages(KbDbContext dbContext)
        => this.dbContext = dbContext;

    public async Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        var chat = context.Get<Chat>("chat");
        var reasoningResponse = context.Get<string>("reasoningResponse");
        var toolResponse = context.Get<string>("toolResponse");
        var finalResponse = context.Get<string>("finalResponse");

        if (reasoningResponse.Length > 0)
            chat.AddMessage(Message.ForReasoning(chat.Id, reasoningResponse));

        if (toolResponse.Length > 0)
        {
            // TODO: Add tool message
            // chat.AddMessage(Message.ForTool(chat.Id, toolResponse));
        }

        chat.AddMessage(Message.ForAssistant(chat.Id, finalResponse));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}