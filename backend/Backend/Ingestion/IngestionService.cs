using System.Security.Cryptography;
using Backend.ContentExtractors;
using Backend.Vectors;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace Backend.Ingestion;

public partial class IngestionService
{
    private readonly IngestionOptions options;
    private readonly ILogger<IngestionService> logger;
    private readonly IContentTypeProvider contentTypeProvider;
    private readonly KbDbContext dbContext;
    private readonly VectorStoreCollection<int, Embeddings> vectorCollection;
    private readonly TextChunker chunker;
    private readonly ContentExtractorFactory contentExtractorFactory;

    public IngestionService(
        IOptions<IngestionOptions> options,
        ILogger<IngestionService> logger,
        IContentTypeProvider contentTypeProvider,
        KbDbContext dbContext,
        VectorStoreCollection<int, Embeddings> vectorCollection,
        TextChunker chunker,
        ContentExtractorFactory contentExtractorFactory)
    {
        this.options = options.Value;
        this.logger = logger;
        this.contentTypeProvider = contentTypeProvider;
        this.dbContext = dbContext;
        this.vectorCollection = vectorCollection;
        this.chunker = chunker;
        this.contentExtractorFactory = contentExtractorFactory;
    }

    public async Task Ingest(Document document, CancellationToken cancellationToken = default)
    {
        try
        {
            await RemoveOldSections(document, cancellationToken);

            var filePath = document.GetPath(options.Path);
            var fileInfo = new FileInfo(filePath);
            await using var stream = File.OpenRead(filePath);
            var hash = await SHA256.HashDataAsync(stream, cancellationToken);
            document.LastModifiedAt = fileInfo.LastWriteTimeUtc;
            document.Hash = hash;

            stream.Position = 0;
            if (!contentTypeProvider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "text/plain";
                LogFailedToDetermineContentType(filePath);
            }

            var extractor = contentExtractorFactory.Create(contentType);
            var content = await extractor.Extract(filePath, stream, cancellationToken);
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

            // TODO: remove
            await dbContext.SaveChangesAsync(cancellationToken);

            // TODO: use EF Core
            var embeddings = document.DocumentSections
                .SelectMany(x => x.DocumentChunks)
                .Select(x => Embeddings.ForDocumentChunk(document, x.Id, x.Content));

            await vectorCollection.UpsertAsync(embeddings, cancellationToken);
        }
        catch (Exception)
        {
            document.MarkAsFailed();

            throw;
        }
    }

    private async Task RemoveOldSections(Document document, CancellationToken cancellationToken = default)
    {
        var chunkIds = document.DocumentSections
            .SelectMany(x => x.DocumentChunks)
            .Select(x => x.Id)
            .ToArray();

        await vectorCollection.DeleteAsync(chunkIds, cancellationToken);
        document.DocumentSections.Clear();

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Deleted chunks (Ids: {Ids}) for document (Id: {DocumentId}).",
                string.Join(", ", chunkIds),
                document.Id);
        }
    }

    public async Task UploadDocument(
        Document document,
        Stream file,
        CancellationToken cancellationToken = default)
    {
        var filePath = document.GetPath(options.Path);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            LogCreatingDirectory(directory);
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(filePath))
            throw new InvalidOperationException($"File already exists: {filePath}");

        await using var stream = File.Open(filePath, FileMode.CreateNew, FileAccess.Write);
        await file.CopyToAsync(stream, cancellationToken);
    }

    [LoggerMessage(LogLevel.Debug, "Failed to determine content type for '{FileName}'. Using 'text/plain'.")]
    private partial void LogFailedToDetermineContentType(string fileName);

    [LoggerMessage(LogLevel.Debug, "Creating directory: {directory}")]
    private partial void LogCreatingDirectory(string directory);
}