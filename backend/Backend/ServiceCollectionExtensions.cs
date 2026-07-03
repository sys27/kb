using System.ClientModel;
using Backend.Chats;
using Backend.ContentExtractors;
using Backend.Ingestion;
using Backend.Knowledge;
using Backend.Vectors;
using LlamaCpp;
using Microsoft.AspNetCore.StaticFiles;
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
            provider => provider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")!,
            provider => new SqliteVectorStoreOptions
            {
                EmbeddingGenerator = provider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>()
            });

        services.AddSqliteCollection<int, Embeddings>(
            "Embeddings",
            provider => provider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection")!,
            provider => new SqliteCollectionOptions
            {
                EmbeddingGenerator = provider.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>()
            });

        return services;
    }

    public static IServiceCollection AddAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<LlmOptions>, LlmOptions>();
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.Section));

        services.AddSingleton<OpenAIClient>(provider =>
        {
            var llmOptions = provider.GetRequiredService<IOptions<LlmOptions>>();

            return new OpenAIClient(
                new ApiKeyCredential(llmOptions.Value.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(llmOptions.Value.Endpoint),
                    NetworkTimeout = TimeSpan.FromMinutes(5),
                });
        });

        services
            .AddChatClient(provider =>
            {
                var llmOptions = provider.GetRequiredService<IOptions<LlmOptions>>();
                var client = provider.GetRequiredService<OpenAIClient>();

                return client.GetChatClient(llmOptions.Value.Model).AsIChatClient();
            })
            .UseFunctionInvocation()
            .UseLogging();

        services.AddTransient<IEmbeddingGenerator<string, Embedding<float>>>(provider =>
        {
            var llmOptions = provider.GetRequiredService<IOptions<LlmOptions>>();
            var client = provider.GetRequiredService<OpenAIClient>();

            return client.GetEmbeddingClient(llmOptions.Value.EmbeddingModel).AsIEmbeddingGenerator();
        });

        services
            .AddHttpClient<LlamaCppClient, LlamaCppClient>((client, provider) =>
            {
                var options = provider.GetRequiredService<IOptions<LlmOptions>>();

                client.BaseAddress = new Uri(options.Value.Endpoint);

                return new LlamaCppClient(client, new LlamaCppClientOptions
                {
                    Model = options.Value.Model,
                    EmbeddingsModel = options.Value.EmbeddingModel,
                    RerankingModel = options.Value.RerankingModel,
                });
            })
            .AddStandardResilienceHandler(options =>
            {
                options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(1);
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(2);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(3);
            });

        services.AddSingleton<Tokenizer>(provider =>
        {
            var llmOptions = provider.GetRequiredService<IOptions<LlmOptions>>().Value;
            var bpeOptions = new BpeOptions(
                $"Tokenizers/{llmOptions.Tokenizer}/vocab.json",
                $"Tokenizers/{llmOptions.Tokenizer}/merges.txt");

            return BpeTokenizer.Create(bpeOptions);
        });

        services.AddTransient<ChatGptImporter>();

        return services;
    }

    public static IServiceCollection AddIngestion(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<IngestionOptions>, IngestionOptions>();
        services.Configure<IngestionOptions>(configuration.GetSection(IngestionOptions.Section));
        services.AddHostedService<DocumentDiscoveryBackgroundService>();
        services.AddHostedService<IngestionBackgroundService>();
        services.AddScoped<IngestionService>();

        services.AddSingleton<FileExtensionContentTypeProvider>();
        services.AddSingleton<IContentTypeProvider, ContentTypeProvider>();

        services.AddSingleton<EpubContentExtractor>();
        services.AddSingleton<HtmlContentExtractor>();
        services.AddSingleton<MarkdownContentExtractor>();
        services.AddSingleton<PdfContentExtractor>();
        services.AddSingleton<PlainTextContentExtractor>();
        services.AddSingleton<ContentExtractorFactory>();
        services.AddSingleton<TextChunker>();

        services.AddSingleton<IValidateOptions<SummarizationOptions>, SummarizationOptions>();
        services.Configure<SummarizationOptions>(configuration.GetSection(SummarizationOptions.Section));
        services.AddHostedService<SummarizationBackgroundService>();

        services.AddSingleton<IValidateOptions<RemoveDanglingEmbeddingsOptions>, RemoveDanglingEmbeddingsOptions>();
        services.Configure<RemoveDanglingEmbeddingsOptions>(configuration.GetSection(RemoveDanglingEmbeddingsOptions.Section));
        services.AddHostedService<RemoveDanglingEmbeddingsService>();

        return services;
    }

    public static IServiceCollection AddKnowledge(this IServiceCollection services)
    {
        services.AddScoped<KnowledgeService>();

        return services;
    }
}