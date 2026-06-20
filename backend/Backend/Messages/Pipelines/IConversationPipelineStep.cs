namespace Backend.Messages.Pipelines;

public interface IConversationPipelineStep
{
    Task ExecuteAsync(ConversationPipelineContext context, CancellationToken cancellationToken = default);
}