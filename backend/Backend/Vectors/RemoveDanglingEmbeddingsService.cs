using Backend.Knowledge;
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

            while (true)
            {
                // TODO: migrate to EF?
                var sql =
                    $"""
                     SELECT e.Id
                     FROM Embeddings e
                     WHERE (e.SourceType = {(int)KnowledgeSource.DocumentChunk}
                         AND NOT EXISTS (SELECT 1
                                         FROM DocumentChunks dc
                                         WHERE dc.Id = e.SourceId))
                         OR (e.SourceType = {(int)KnowledgeSource.ChatSummary}
                             AND NOT EXISTS (SELECT 1
                                             FROM Chats c
                                             WHERE c.Id = e.SourceId))
                         OR (e.SourceType = {(int)KnowledgeSource.ChatFact}
                             AND NOT EXISTS (SELECT 1
                                             FROM ChatFacts cf
                                             WHERE cf.Id = e.SourceId))
                         OR (e.SourceType = {(int)KnowledgeSource.ChatDecision}
                             AND NOT EXISTS (SELECT 1
                                             FROM ChatDecisions CD
                                             WHERE CD.Id = e.SourceId))
                         OR (e.SourceType = {(int)KnowledgeSource.ChatUserPreference}
                             AND NOT EXISTS (SELECT 1
                                             FROM ChatUserPreferences cup
                                             WHERE cup.Id = e.SourceId))
                     ORDER BY e.Id
                     LIMIT {embeddingsOptions.BatchSize}
                     """;

                var embeddingIds = await dbContext.Database
                    .SqlQueryRaw<int>(sql)
                    .ToListAsync(stoppingToken);

                if (embeddingIds.Count == 0)
                {
                    LogNoDanglingEmbeddingsFound();
                    break;
                }

                LogFoundDanglingEmbeddings(embeddingIds.Count);

                await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
                try
                {
                    var parameters = embeddingIds.Select((x, i) => new SqliteParameter($"@id{i}", x)).ToArray();
                    var parameterNames = string.Join(", ", parameters.Select(x => x.ParameterName));

#pragma warning disable EF1002
                    var count = await dbContext.Database.ExecuteSqlRawAsync(
                        $"""
                         DELETE FROM vec_Embeddings AS ve
                         WHERE ve.Id IN ({parameterNames});

                         DELETE FROM Embeddings AS ve
                         WHERE ve.Id IN ({parameterNames});
                         """,
                        parameters,
                        stoppingToken);
#pragma warning restore EF1002

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
            }

            logger.LogInformation("Removing dangling embeddings completed.");

            await Task.Delay(embeddingsOptions.Delay, stoppingToken);
        }
    }

    [LoggerMessage(LogLevel.Information, "Removed {Count} dangling embeddings.")]
    private partial void LogRemovedDanglingEmbeddings(int count);

    [LoggerMessage(LogLevel.Information, "Found {Count} dangling embeddings.")]
    private partial void LogFoundDanglingEmbeddings(int count);

    [LoggerMessage(LogLevel.Debug, "No dangling embeddings found.")]
    private partial void LogNoDanglingEmbeddingsFound();
}