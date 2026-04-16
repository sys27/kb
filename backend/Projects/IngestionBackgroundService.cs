using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace Backend.Projects;

public class IngestionBackgroundService : BackgroundService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator;

    public IngestionBackgroundService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        this.embeddingGenerator = embeddingGenerator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var vectorStore = new SqliteVectorStore(
            "Data Source=kb.db",
            new SqliteVectorStoreOptions { EmbeddingGenerator = embeddingGenerator });
        var collection = vectorStore.GetCollection<int, Document>("documents");
        await collection.EnsureCollectionExistsAsync(stoppingToken);

        await collection.UpsertAsync(
            new Document
            {
                Id = 1,
                Name = "The Lion King",
                Content = "An animated film about a young lion prince",
                ProjectId = 1
            },
            stoppingToken);
        await collection.UpsertAsync(
            new Document
            {
                Id = 2,
                Name = "Inception",
                Content = "A thief who steals corporate secrets through dream-sharing technology",
                ProjectId = 1
            },
            stoppingToken);
        await collection.UpsertAsync(
            new Document
            {
                Id = 3,
                Name = "Finding Nemo",
                Content = "A fish searches the ocean for his lost son",
                ProjectId = 1
            },
            stoppingToken);

        await foreach (var result in collection.SearchAsync("thief", 2, null, stoppingToken))
        {
            Console.WriteLine($"{result.Record.Name} (score: {result.Score})");
        }
    }
}