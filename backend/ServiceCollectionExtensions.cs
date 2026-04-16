using System.ClientModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;

namespace Backend;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        services.AddDbContext<KbDbContext>(options =>
        {
            options
                .UseSqlite(configuration.GetConnectionString("DefaultConnection"))
                .EnableDetailedErrors(env.IsDevelopment())
                .EnableSensitiveDataLogging(env.IsDevelopment())
                .ConfigureWarnings(w =>
                {
#if DEBUG
                    w.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
#endif
                });
        });

        return services;
    }

    public static IServiceCollection AddAiClient(this IServiceCollection services)
    {
        services.AddSingleton<OpenAIClient>(provider =>
        {
            var llmOptions = provider.GetRequiredService<IOptions<LlmOptions>>();

            return new OpenAIClient(
                new ApiKeyCredential(llmOptions.Value.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(llmOptions.Value.Endpoint),
                });
        });
        services.AddTransient<IChatClient>(provider =>
        {
            var llmOptions = provider.GetRequiredService<IOptions<LlmOptions>>();
            var client = provider.GetRequiredService<OpenAIClient>();

            return client.GetChatClient(llmOptions.Value.Model).AsIChatClient();
        });
        services.AddTransient<IEmbeddingGenerator>(provider =>
        {
            var llmOptions = provider.GetRequiredService<IOptions<LlmOptions>>();
            var client = provider.GetRequiredService<OpenAIClient>();

            return client.GetEmbeddingClient(llmOptions.Value.EmbeddingModel).AsIEmbeddingGenerator();
        });

        return services;
    }
}