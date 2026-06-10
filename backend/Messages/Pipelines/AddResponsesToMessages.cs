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
        var toolCallResponse = context.Get<string>("toolCallResponse");
        var toolResultResponse = context.Get<string>("toolResultResponse");
        var finalResponse = context.Get<string>("finalResponse");

        if (reasoningResponse.Length > 0)
            chat.AddMessage(Message.ForReasoning(chat.Id, reasoningResponse));

        if (toolCallResponse.Length > 0)
            chat.AddMessage(Message.ForToolCall(chat.Id, toolCallResponse));

        if (toolResultResponse.Length > 0)
            chat.AddMessage(Message.ForToolResult(chat.Id, toolResultResponse));

        if (finalResponse.Length > 0)
            chat.AddMessage(Message.ForAssistant(chat.Id, finalResponse));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}