using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Ingestion;

public partial class DocumentDiscoveryBackgroundService : BackgroundService
{
    private readonly IngestionOptions options;
    private readonly ILogger<DocumentDiscoveryBackgroundService> logger;
    private readonly IServiceScopeFactory scopeFactory;

    private readonly string[] supportedFileExtensions =
    [
        ".txt",
        ".md",
        ".pdf",
    ];

    [GeneratedRegex(@"project-(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex GetProjectRegex();

    public DocumentDiscoveryBackgroundService(
        IOptions<IngestionOptions> options,
        ILogger<DocumentDiscoveryBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        this.options = options.Value;
        this.logger = logger;
        this.scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.IsDocumentDiscoveryEnabled)
            return;

        if (!Directory.Exists(options.Path))
        {
            logger.LogError("Directory '{Path}' does not exist", options.Path);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Starting document discovery...");

            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KbDbContext>();

            var filesToProcess = GetFilesToProcess();
            foreach (var (projectId, files) in filesToProcess)
            {
                var project = dbContext.Projects
                    .Include(x => x.Documents)
                    .FirstOrDefault(p => p.Id == projectId);

                if (project is null)
                {
                    logger.LogWarning("Project '{ProjectName}' not found in database", projectId);
                    continue;
                }

                foreach (var document in project.Documents.ToArray())
                    if (!File.Exists(Path.Combine(options.Path, project.GetDirectoryName(), document.Name)))
                        project.Documents.Remove(document);

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
                        project.Documents.Add(new Document
                        {
                            Name = fileInfo.Name,
                            Status = DocumentStatus.Pending,
                            ProjectId = project.Id,
                            Project = project,
                        });
                    }
                    else if (document.IsIngested)
                    {
                        if (document.LastModifiedAt == fileInfo.LastWriteTimeUtc)
                            continue;

                        await using var stream = File.OpenRead(file);
                        var hash = await SHA256.HashDataAsync(stream, stoppingToken);

                        if (document.Hash != hash)
                            document.MarkAsToProcess();
                    }
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }

            logger.LogInformation("Document discovery completed.");

            await Task.Delay(options.DocumentDiscoveryDelay, stoppingToken);
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
            var match = GetProjectRegex().Match(di.Name);
            if (!match.Success)
            {
                logger.LogWarning("Skipping directory '{DirectoryName}'", di.Name);
                continue;
            }

            if (!int.TryParse(match.Groups[1].Value, out var projectId))
            {
                logger.LogWarning("Project ID not found in directory name '{DirectoryName}'", di.Name);
                continue;
            }

            var files = Directory.EnumerateFiles(directory, "*", enumerationOptions).ToArray();
            if (files.Length == 0)
                logger.LogWarning("No files found in directory '{DirectoryName}'", di.Name);

            filesToProcess.Add(new DirectoryFile(projectId, files));
        }

        return filesToProcess;
    }

    private readonly record struct DirectoryFile(int ProjectId, string[] Files);
}