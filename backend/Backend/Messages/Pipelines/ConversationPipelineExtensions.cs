using Backend.Messages.Tools.WebSearch;
using Backend.WebFetchHandlers;
using Microsoft.Extensions.Options;

namespace Backend.Messages.Pipelines;

public static class ConversationPipelineExtensions
{
    public static IServiceCollection AddConversationPipeline(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddTransient<GetChat>();
        services.AddTransient<InsertSystemMessage>();
        services.AddTransient<GatherKnowledge>();
        services.AddTransient<SendRequest>();
        services.AddTransient<ProcessResponse>();

        services.AddTransient(provider => new ConversationPipeline([
            provider.GetRequiredService<GetChat>(),
            provider.GetRequiredService<InsertSystemMessage>(),
            provider.GetRequiredService<GatherKnowledge>(),
            provider.GetRequiredService<SendRequest>(),
            provider.GetRequiredService<ProcessResponse>(),
        ]));

        services.AddSingleton<IValidateOptions<WebSearchOptions>, WebSearchOptions>();
        services.Configure<WebSearchOptions>(configuration.GetSection(WebSearchOptions.Section));
        services
            .AddHttpClient<WebSearchService>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<WebSearchOptions>>();

                client.BaseAddress = new Uri(options.Value.BaseUrl!);
            })
            .AddStandardResilienceHandler();

        services
            .AddHttpClient<IWebFetchHandler, WikipediaHandler>((_, client) =>
                client.DefaultRequestHeaders.UserAgent.ParseAdd("KnowledgeBase (sys2712@gmail.com)"))
            .AddStandardResilienceHandler();
        services
            .AddHttpClient<IWebFetchHandler, DefaultHandler>()
            .AddStandardResilienceHandler();
        services.AddTransient<WebFetchHandlerFactory>();

        return services;
    }
}