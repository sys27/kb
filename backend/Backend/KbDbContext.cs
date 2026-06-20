using Backend.Chats;
using Backend.Ingestion;
using Backend.Messages;
using Backend.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend;

public class KbDbContext : DbContext
{
    public KbDbContext(DbContextOptions<KbDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KbDbContext).Assembly);

        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        foreach (var property in entityType.GetProperties())
        {
            if (property.ClrType == typeof(DateTime))
                property.SetValueConverter(dateTimeConverter);
            else if (property.ClrType == typeof(DateTime?))
                property.SetValueConverter(nullableDateTimeConverter);
        }
    }

    public DbSet<Chat> Chats { get; set; }

    public DbSet<ChatTopic> ChatTopics { get; set; }

    public DbSet<ChatFact> ChatFacts { get; set; }

    public DbSet<ChatDecision> ChatDecisions { get; set; }

    public DbSet<ChatUserPreference> ChatUserPreferences { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<Document> Documents { get; set; }

    public DbSet<DocumentChunk> DocumentChunks { get; set; }
}