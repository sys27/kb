using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Ingestion;

public class DocumentChunkTypeConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");

        builder.HasKey(e => e.Id)
            .HasName("PK_DocumentChunks");

        builder.HasIndex(e => e.DocumentId, "IX_DocumentChunks_DocumentId");

        builder.Ignore(e => e.Embedding);

        builder.HasOne(x => x.Document)
            .WithMany(p => p.DocumentChunks)
            .HasForeignKey(d => d.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}