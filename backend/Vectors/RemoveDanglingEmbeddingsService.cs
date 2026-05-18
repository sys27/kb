using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Vectors;

public partial class RemoveDanglingEmbeddingsService : BackgroundService
{
    private readonly RemoveDanglingEmbeddingsOptions embeddingsOptions;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<RemoveDanglingEmbeddingsService> logger;

    public RemoveDanglingEmbeddingsService(
        IOptions<RemoveDanglingEmbeddingsOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<RemoveDanglingEmbeddingsService> logger)
    {
        this.embeddingsOptions = options.Value;
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!embeddingsOptions.Enabled)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Starting remove dangling embeddings...");

            using var scope = scopeFactory.CreateScope();
            await using var dbContext = scope.ServiceProvider.GetRequiredService<KbDbContext>();

            var lastSeenId = default(int?);
            while (true)
            {
                // TODO: migrate to EF?
                var sql =
                    $"""
                     SELECT e.Id
                     FROM Embeddings e
                     WHERE (@lastSeenId IS NULL OR e.Id > @lastSeenId)
                       AND ((e.SourceType = {(int)EmbeddingSourceType.DocumentChunk}
                         AND NOT EXISTS (SELECT 1
                                         FROM DocumentChunks dc
                                         WHERE dc.Id = e.SourceId))
                         OR (e.SourceType = {(int)EmbeddingSourceType.ChatSummary}
                             AND NOT EXISTS (SELECT 1
                                             FROM Chats c
                                             WHERE c.Id = e.SourceId))
                         OR (e.SourceType = {(int)EmbeddingSourceType.ChatFact}
                             AND NOT EXISTS (SELECT 1
                                             FROM ChatFacts cf
                                             WHERE cf.Id = e.SourceId))
                         OR (e.SourceType = {(int)EmbeddingSourceType.ChatDecision}
                             AND NOT EXISTS (SELECT 1
                                             FROM ChatDecisions CD
                                             WHERE CD.Id = e.SourceId))
                         OR (e.SourceType = {(int)EmbeddingSourceType.ChatUserPreference}
                             AND NOT EXISTS (SELECT 1
                                             FROM ChatUserPreferences cup
                                             WHERE cup.Id = e.SourceId)))
                     ORDER BY e.Id
                     LIMIT {embeddingsOptions.BatchSize}
                     """;

                var embeddingIds = await dbContext.Database
                    .SqlQueryRaw<int>(
                        sql,
                        new SqliteParameter("@lastSeenId", lastSeenId ?? (object)DBNull.Value) { IsNullable = true })
                    .ToListAsync(stoppingToken);

                if (embeddingIds.Count == 0)
                    break;

                await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
                try
                {
                    var count = await dbContext.Database.ExecuteSqlAsync(
                        $"""
                         DELETE FROM vec_Embeddings AS ve
                         WHERE ve.Id IN ({embeddingIds});

                         DELETE FROM Embeddings AS ve
                         WHERE ve.Id IN ({embeddingIds})
                         """,
                        stoppingToken);

                    await transaction.CommitAsync(stoppingToken);

                    LogRemovedDanglingEmbeddings(count);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to remove dangling embeddings.");
                    await transaction.RollbackAsync(CancellationToken.None);
                    throw;
                }

                if (embeddingIds.Count < embeddingsOptions.BatchSize)
                    break;

                lastSeenId = embeddingIds[^1];
            }

            logger.LogInformation("Removing dangling embeddings completed.");

            await Task.Delay(embeddingsOptions.Delay, stoppingToken);
        }
    }

    [LoggerMessage(LogLevel.Information, "Removed {Count} dangling embeddings.")]
    private partial void LogRemovedDanglingEmbeddings(int count);
}