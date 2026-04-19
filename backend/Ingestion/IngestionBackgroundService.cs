using System.Security.Cryptography;
using System.Text;
using Backend.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace Backend.Ingestion;

public class IngestionBackgroundService : BackgroundService
{
    private readonly IngestionOptions options;
    private readonly ILogger<IngestionBackgroundService> logger;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly VectorStore vectorStore;
    private readonly ChunkerFactory chunkerFactory;

    private readonly string[] supportedFileExtensions =
    [
        ".txt",
        // TODO: ".md",
    ];

    private const string ProjectPrefix = "project-";

    public IngestionBackgroundService(
        IOptions<IngestionOptions> options,
        ILogger<IngestionBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        VectorStore vectorStore,
        ChunkerFactory chunkerFactory)
    {
        this.options = options.Value;
        this.logger = logger;
        this.scopeFactory = scopeFactory;
        this.vectorStore = vectorStore;
        this.chunkerFactory = chunkerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Directory.Exists(options.Path))
        {
            logger.LogError("Directory '{Path}' does not exist", options.Path);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KbDbContext>();

            var filesToProcess = GetFilesToProcess();
            var documentChanges = await GetDocumentChanges(filesToProcess, dbContext, stoppingToken);

            // process files
            using var collection = vectorStore.GetCollection<int, DocumentChunk>("DocumentChunks");
            await collection.EnsureCollectionExistsAsync(stoppingToken);

            foreach (var (project, file) in documentChanges.DocumentsToAdd)
            {
                try
                {
                    var document = new Document
                    {
                        Name = Path.GetFileName(file),
                        ProjectId = project.Id,
                    };

                    await ProcessFileAndChunks(document, file, stoppingToken);

                    // TODO: transaction?
                    await dbContext.AddAsync(document, stoppingToken);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    await collection.UpsertAsync(document.DocumentChunks, stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error adding document: '{FileName}'", file);
                }
            }

            foreach (var (document, file) in documentChanges.DocumentsToUpdate)
            {
                try
                {
                    await collection.DeleteAsync(document.DocumentChunks.Select(x => x.Id), stoppingToken);

                    foreach (var chunk in document.DocumentChunks)
                        dbContext.Entry(chunk).State = EntityState.Detached;

                    document.DocumentChunks.Clear();

                    await ProcessFileAndChunks(document, file, stoppingToken);

                    // TODO: transaction?
                    dbContext.Update(document);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    await collection.UpsertAsync(document.DocumentChunks, stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error updating document: '{FileName}'", file);
                }
            }

            foreach (var document in documentChanges.DocumentsToRemove)
            {
                try
                {
                    // TODO: transaction?
                    await collection.DeleteAsync(document.DocumentChunks.Select(x => x.Id), stoppingToken);

                    foreach (var chunk in document.DocumentChunks)
                        dbContext.Entry(chunk).State = EntityState.Detached;

                    document.DocumentChunks.Clear();

                    dbContext.Documents.Remove(document);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error removing document: '{DocumentName}'", document.Name);
                }
            }

            await Task.Delay(options.Delay, stoppingToken);
        }
    }

    private List<DirectoryFile> GetFilesToProcess()
    {
        var filesToProcess = new List<DirectoryFile>();

        var enumerationOptions = new EnumerationOptions
        {
            IgnoreInaccessible = true,
            RecurseSubdirectories = false,
            ReturnSpecialDirectories = false,
        };
        var directories = Directory.EnumerateDirectories(options.Path, "*", enumerationOptions);
        foreach (var directory in directories)
        {
            var di = new DirectoryInfo(directory);
            if (!di.Name.StartsWith(ProjectPrefix, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Skipping directory '{DirectoryName}'", di.Name);
                continue;
            }

            var files = Directory.EnumerateFiles(directory, "*", enumerationOptions).ToArray();
            if (files.Length == 0)
            {
                logger.LogWarning("No files found in directory '{DirectoryName}'", di.Name);
                continue;
            }

            filesToProcess.Add(new DirectoryFile(di.Name, files));
        }

        return filesToProcess;
    }

    private async Task<DocumentChanges> GetDocumentChanges(
        List<DirectoryFile> filesToProcess,
        KbDbContext dbContext,
        CancellationToken stoppingToken)
    {
        var documentsToAdd = new List<DocumentToAdd>();
        var documentsToUpdate = new List<DocumentUpdate>();
        var documentsToRemove = new List<Document>();

        foreach (var (directoryName, files) in filesToProcess)
        {
            // TODO: better way to map projects to directories
            var projectName = directoryName[ProjectPrefix.Length..];
            var project = dbContext.Projects
                .Include(x => x.Documents)
                .ThenInclude(x => x.DocumentChunks)
                .AsSplitQuery()
                .FirstOrDefault(p => p.Name == projectName);

            if (project is null)
            {
                logger.LogWarning("Project '{ProjectName}' not found in database", projectName);
                continue;
            }

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (!supportedFileExtensions.Contains(fileInfo.Extension))
                {
                    logger.LogWarning("The file '{FileName}' is not supported.", fileInfo.Name);
                    continue;
                }

                var document = project.Documents.FirstOrDefault(d => d.Name == fileInfo.Name);
                if (document is null)
                {
                    documentsToAdd.Add(new DocumentToAdd(project, file));
                }
                else
                {
                    if (document.LastModifiedAt == fileInfo.LastWriteTimeUtc)
                        continue;

                    await using var stream = File.OpenRead(file);
                    var hash = await SHA256.HashDataAsync(stream, stoppingToken);

                    if (document.Hash != hash)
                        documentsToUpdate.Add(new DocumentUpdate(document, file));
                }
            }

            foreach (var document in project.Documents)
                if (!File.Exists(Path.Combine(options.Path, directoryName, document.Name)))
                    documentsToRemove.Add(document);
        }

        return new DocumentChanges(documentsToAdd, documentsToUpdate, documentsToRemove);
    }

    private async Task ProcessFileAndChunks(Document document, string file, CancellationToken stoppingToken)
    {
        var fileInfo = new FileInfo(file);
        await using var stream = File.OpenRead(file);
        var hash = await SHA256.HashDataAsync(stream, stoppingToken);
        document.LastModifiedAt = fileInfo.LastWriteTimeUtc;
        document.Hash = hash;

        stream.Position = 0;
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        var content = await streamReader.ReadToEndAsync(stoppingToken);

        var chunker = chunkerFactory.Create(file);
        var chunks = chunker.Split(content);
        foreach (var chunk in chunks)
        {
            var text = content.Substring(chunk.Start, chunk.Length);
            var documentChunk = new DocumentChunk
            {
                DocumentId = document.Id,
                Content = text,
                Embedding = text,
                Start = chunk.Start,
                Length = chunk.Length,
            };
            document.DocumentChunks.Add(documentChunk);
        }
    }

    private readonly record struct DirectoryFile(string DirectoryName, string[] Files);

    private readonly record struct DocumentChanges(
        List<DocumentToAdd> DocumentsToAdd,
        List<DocumentUpdate> DocumentsToUpdate,
        List<Document> DocumentsToRemove);

    private readonly record struct DocumentToAdd(Project Project, string File);

    private readonly record struct DocumentUpdate(Document Document, string File);
}