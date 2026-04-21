namespace Backend.Messages.Pipelines;

public static class ConversationPipelineExtensions
{
    public static IServiceCollection AddConversationPipeline(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddTransient<GetChat>();
        services.AddTransient<InsertSystemMessage>();
        services.AddTransient<GatherKnowledge>();
        services.AddTransient<SendRequest>();
        services.AddTransient<ProcessResponse>();
        services.AddTransient<AddResponsesToMessages>();

        services.AddTransient(provider => new ConversationPipeline([
            provider.GetRequiredService<GetChat>(),
            provider.GetRequiredService<InsertSystemMessage>(),
            provider.GetRequiredService<GatherKnowledge>(),
            provider.GetRequiredService<SendRequest>(),
            provider.GetRequiredService<ProcessResponse>(),
            provider.GetRequiredService<AddResponsesToMessages>(),
        ]));

        return services;
    }
}