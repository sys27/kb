using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Ingestion.TypeConfigurations;

public class DocumentChunkTypeConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");

        builder.HasKey(e => e.Id)
            .HasName("PK_DocumentChunks");

        builder.HasIndex(e => e.DocumentSectionId, "IX_DocumentChunks_DocumentSectionId");

        builder.HasOne(x => x.DocumentSection)
            .WithMany(p => p.DocumentChunks)
            .HasForeignKey(d => d.DocumentSectionId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DocumentChunks_DocumentSections_DocumentSectionId");
    }
}