using Backend.Vectors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.VectorData;

namespace Backend;

public static class WebApplicationExtensions
{
    public static async Task InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        var dbContext = serviceProvider.GetRequiredService<KbDbContext>();
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("SQLite Database migration completed");

        var collection = serviceProvider.GetRequiredService<VectorStoreCollection<int, Embeddings>>();
        await collection.EnsureCollectionExistsAsync();
        logger.LogInformation("Vector Store Collection created");
    }
}