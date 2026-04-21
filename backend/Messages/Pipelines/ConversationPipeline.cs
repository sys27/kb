namespace Backend.Messages.Pipelines;

public class ConversationPipeline
{
    private readonly IEnumerable<IConversationPipelineStep> steps;

    public ConversationPipeline(IEnumerable<IConversationPipelineStep> steps)
        => this.steps = steps;

    public async Task Run(ConversationPipelineContext context, CancellationToken cancellationToken = default)
    {
        foreach (var step in steps)
            await step.ExecuteAsync(context, cancellationToken);
    }
}