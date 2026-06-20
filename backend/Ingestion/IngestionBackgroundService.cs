using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Ingestion;

public partial class IngestionBackgroundService : BackgroundService
{
    private readonly IngestionOptions options;
    private readonly ILogger<IngestionBackgroundService> logger;
    private readonly IServiceScopeFactory scopeFactory;

    public IngestionBackgroundService(
        IOptions<IngestionOptions> options,
        ILogger<IngestionBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        this.options = options.Value;
        this.logger = logger;
        this.scopeFactory = scopeFactory;
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
            var ingestionService = scope.ServiceProvider.GetRequiredService<IngestionService>();

            var documentsToProcess = await dbContext.Documents
                .Include(x => x.DocumentSections)
                .ThenInclude(x => x.DocumentChunks)
                .Include(x => x.Project)
                .Where(x => x.Status == DocumentStatus.Pending)
                .AsSplitQuery()
                .ToListAsync(stoppingToken);

            foreach (var document in documentsToProcess)
            {
                try
                {
                    await ingestionService.Ingest(document, stoppingToken);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    LogDocumentProcessed(document.Name);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error updating document: '{FileName}'", document.Name);

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
                        logger.LogError(ex, "Failed to mark document as failed: '{FileName}'", document.Name);
                    }
                }
            }

            logger.LogInformation("Ingestion completed.");

            await Task.Delay(options.IngestionDelay, stoppingToken);
        }
    }

    [LoggerMessage(LogLevel.Information, "Document '{FileName}' processed.")]
    private partial void LogDocumentProcessed(string fileName);
}