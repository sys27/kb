using System.Security.Cryptography;
using Backend.ContentExtractors;
using Backend.Vectors;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace Backend.Ingestion;

public partial class IngestionBackgroundService : BackgroundService
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
                .Include(x => x.DocumentSections)
                .ThenInclude(x => x.DocumentChunks)
                .Include(x => x.Project)
                .Where(x => x.Status == DocumentStatus.Pending)
                .AsSplitQuery()
                .ToListAsync(stoppingToken);

            foreach (var document in documentsToProcess)
            {
                var filePath = Path.Combine(options.Path, document.Project!.GetDirectoryName(), document.Name);

                try
                {
                    var chunkIds = document.DocumentSections
                        .SelectMany(x => x.DocumentChunks)
                        .Select(x => x.Id)
                        .ToArray();

                    await vectorCollection.DeleteAsync(chunkIds, stoppingToken);
                    document.DocumentSections.Clear();
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug(
                            "Deleted chunks (Ids: {Ids}) for document (Id: {DocumentId}).",
                            string.Join(", ", chunkIds),
                            document.Id);
                    }

                    var fileInfo = new FileInfo(filePath);
                    await using var stream = File.OpenRead(filePath);
                    var hash = await SHA256.HashDataAsync(stream, stoppingToken);
                    document.LastModifiedAt = fileInfo.LastWriteTimeUtc;
                    document.Hash = hash;

                    stream.Position = 0;
                    if (!contentTypeProvider.TryGetContentType(filePath, out var contentType))
                    {
                        contentType = "text/plain";
                        LogFailedToDetermineContentType(filePath);
                    }

                    var extractor = contentExtractorFactory.Create(contentType);
                    var content = await extractor.Extract(filePath, stream, stoppingToken);
                    document.Title = content.Title;

                    var sections = content.Sections
                        .Where(x => !string.IsNullOrWhiteSpace(x.Content))
                        .Select(x => (x, Chunks: chunker.Split(x.Content)));

                    foreach (var (contentSection, chunks) in sections)
                    {
                        if (chunks.Count == 0)
                        {
                            logger.LogWarning("No chunks found in a section for '{FilePath}' document. Skipping.", filePath);
                            continue;
                        }

                        var section = new DocumentSection
                        {
                            Header = contentSection.Header,
                            Document = document,
                        };

                        foreach (var chunk in chunks)
                        {
                            var text = contentSection.Content.Substring(chunk.Start, chunk.Length);
                            var documentChunk = new DocumentChunk
                            {
                                Content = text,
                                Start = chunk.Start,
                                Length = chunk.Length,
                                DocumentSection = section,
                            };
                            section.DocumentChunks.Add(documentChunk);
                        }

                        document.DocumentSections.Add(section);
                    }

                    document.MarkAsProcessed();

                    // TODO: transaction?
                    await dbContext.SaveChangesAsync(stoppingToken);

                    var embeddings = document.DocumentSections
                        .SelectMany(x => x.DocumentChunks)
                        .Select(x => Embeddings.ForDocumentChunk(document.ProjectId, x.Id, x.Content));

                    await vectorCollection.UpsertAsync(embeddings, stoppingToken);

                    LogDocumentProcessed(filePath);
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

    [LoggerMessage(LogLevel.Debug, "Failed to determine content type for '{FileName}'. Using 'text/plain'.")]
    private partial void LogFailedToDetermineContentType(string fileName);

    [LoggerMessage(LogLevel.Information, "Document '{FileName}' processed.")]
    private partial void LogDocumentProcessed(string fileName);
}