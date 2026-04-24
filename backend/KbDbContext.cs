using Backend.Chats;
using Backend.Ingestion;
using Backend.Messages;
using Backend.Projects;
using Backend.Vectors;
using Microsoft.EntityFrameworkCore;

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
    }

    public async Task<string?> GetEmbeddingsContent(Embeddings embeddings, CancellationToken cancellationToken = default)
    {
        if (embeddings.SourceType == (int)EmbeddingSourceType.DocumentChunk)
        {
            var chunk = await DocumentChunks.FirstOrDefaultAsync(x => x.Id == embeddings.SourceId, cancellationToken);

            return chunk?.Content;
        }

        if (embeddings.SourceType == (int)EmbeddingSourceType.ChatSummary)
        {
            var chat = await Chats.FirstOrDefaultAsync(x => x.Id == embeddings.SourceId, cancellationToken);

            return chat?.Summary;
        }

        if (embeddings.SourceType == (int)EmbeddingSourceType.ChatFact)
        {
            var fact = await ChatFacts.FirstOrDefaultAsync(x => x.Id == embeddings.SourceId, cancellationToken);

            return fact?.Fact;
        }

        if (embeddings.SourceType == (int)EmbeddingSourceType.ChatDecision)
        {
            var decision = await ChatDecisions.FirstOrDefaultAsync(x => x.Id == embeddings.SourceId, cancellationToken);
            if (decision is null)
                return null;

            return $"Decision: {decision.Decision}. Reason: {decision.Reason}.";
        }

        if (embeddings.SourceType == (int)EmbeddingSourceType.ChatUserPreference)
        {
            var preference = await ChatUserPreferences
                .FirstOrDefaultAsync(x => x.Id == embeddings.SourceId, cancellationToken);

            return preference?.Preference;
        }

        throw new ArgumentOutOfRangeException(nameof(embeddings.SourceType), "Unknown source type");
    }

    public DbSet<Chat> Chats { get; set; }

    public DbSet<ChatFact> ChatFacts { get; set; }

    public DbSet<ChatDecision> ChatDecisions { get; set; }

    public DbSet<ChatUserPreference> ChatUserPreferences { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<Document> Documents { get; set; }

    public DbSet<DocumentChunk> DocumentChunks { get; set; }
}