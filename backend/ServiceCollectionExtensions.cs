using System.ClientModel;
using Backend.Ingestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
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

        services.AddSqliteVectorStore(
            provider => provider.GetRequiredService<IOptions<IngestionOptions>>().Value.ConnectionString,
            provider => new SqliteVectorStoreOptions
            {
                EmbeddingGenerator = provider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>()
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

        services.AddTransient<IEmbeddingGenerator<string, Embedding<float>>>(provider =>
        {
            var llmOptions = provider.GetRequiredService<IOptions<LlmOptions>>();
            var client = provider.GetRequiredService<OpenAIClient>();

            return client.GetEmbeddingClient(llmOptions.Value.EmbeddingModel).AsIEmbeddingGenerator();
        });

        services.AddSingleton<Tokenizer>(_ =>
        {
            var bpeOptions = new BpeOptions("Tokenizers/qwen3/vocab.json", "Tokenizers/qwen3/merges.txt");
            var tokenizer = BpeTokenizer.Create(bpeOptions);

            return tokenizer;
        });

        return services;
    }

    public static IServiceCollection AddIngestion(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<IngestionOptions>, IngestionOptions>();
        services.Configure<IngestionOptions>(configuration.GetSection(IngestionOptions.Section));
        services.AddHostedService<IngestionBackgroundService>();

        services.AddSingleton<TextChunker>();
        services.AddSingleton<MarkdownChunker>();
        services.AddSingleton<ChunkerFactory>();

        return services;
    }
}