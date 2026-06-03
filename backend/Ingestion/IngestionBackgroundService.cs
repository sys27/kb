using System.Security.Cryptography;
using Backend.ContentExtractors;
using Backend.Vectors;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace Backend.Ingestion;

public class IngestionBackgroundService : BackgroundService
{
    private readonly IngestionOptions options;
    private readonly ILogger<IngestionBackgroundService> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IContentTypeProvider contentTypeProvider;
    private readonly VectorStoreCollection<int, Embeddings> vectorCollection;
    private readonly TextChunker chunker;
    private readonly ContentExtractorFactory contentExtractorFactory;

    public IngestionBackgroundService(
        IOptions<IngestionOptions> options,
        ILogger<IngestionBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IContentTypeProvider contentTypeProvider,
        VectorStoreCollection<int, Embeddings> vectorCollection,
        TextChunker chunker,
        ContentExtractorFactory contentExtractorFactory)
    {
        this.options = options.Value;
        this.logger = logger;
        this.scopeFactory = scopeFactory;
        this.contentTypeProvider = contentTypeProvider;
        this.vectorCollection = vectorCollection;
        this.chunker = chunker;
        this.contentExtractorFactory = contentExtractorFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.IsIngestionEnabled)
            return;

        if (!Directory.Exists(options.Path))
        {
            logger.LogError("Directory '{Path}' does not exist", options.Path);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Starting ingestion...");

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KbDbContext>();

            var documentsToProcess = await dbContext.Documents
                .Include(x => x.DocumentChunks)
                .Include(x => x.Project)
                .Where(x => x.Status == DocumentStatus.Pending)
                .ToListAsync(stoppingToken);

            foreach (var document in documentsToProcess)
            {
                var filePath = Path.Combine(options.Path, document.Project!.GetDirectoryName(), document.Name);

                try
                {
                    await vectorCollection.DeleteAsync(document.DocumentChunks.Select(x => x.Id), stoppingToken);
                    document.DocumentChunks.Clear();

                    var fileInfo = new FileInfo(filePath);
                    await using var stream = File.OpenRead(filePath);
                    var hash = await SHA256.HashDataAsync(stream, stoppingToken);
                    document.LastModifiedAt = fileInfo.LastWriteTimeUtc;
                    document.Hash = hash;

                    stream.Position = 0;
                    if (!contentTypeProvider.TryGetContentType(filePath, out var contentType))
                        contentType = "text/plain";

                    var extractor = contentExtractorFactory.Create(contentType);
                    var content = await extractor.Extract(filePath, stream, stoppingToken);

                    var chunks = chunker.Split(content);
                    foreach (var chunk in chunks)
                    {
                        var text = content.Substring(chunk.Start, chunk.Length);
                        var documentChunk = new DocumentChunk
                        {
                            DocumentId = document.Id,
                            Content = text,
                            Start = chunk.Start,
                            Length = chunk.Length,
                        };
                        document.DocumentChunks.Add(documentChunk);
                    }

                    document.MarkAsProcessed();

                    // TODO: transaction?
                    await dbContext.SaveChangesAsync(stoppingToken);

                    await vectorCollection.UpsertAsync(
                        document.DocumentChunks.Select(x =>
                            Embeddings.ForDocumentChunk(document.ProjectId, x.Id, x.Content)),
                        stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error updating document: '{FileName}'", filePath);

                    try
                    {
                        using var failScope = scopeFactory.CreateScope();
                        var failDbContext = failScope.ServiceProvider.GetRequiredService<KbDbContext>();

                        await failDbContext.Documents
                            .Where(x => x.Id == document.Id)
                            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, DocumentStatus.Failed), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to mark document as failed: '{FileName}'", filePath);
                    }
                }
            }

            logger.LogInformation("Ingestion completed.");

            await Task.Delay(options.IngestionDelay, stoppingToken);
        }
    }
}