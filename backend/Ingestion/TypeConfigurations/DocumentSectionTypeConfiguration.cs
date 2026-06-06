using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Ingestion.TypeConfigurations;

public class DocumentSectionTypeConfiguration : IEntityTypeConfiguration<DocumentSection>
{
    public void Configure(EntityTypeBuilder<DocumentSection> builder)
    {
        builder.ToTable("DocumentSections");

        builder.HasKey(e => e.Id)
            .HasName("PK_DocumentSections");

        builder.Property(e => e.Header)
            .HasMaxLength(256);

        builder.HasIndex(e => e.DocumentId, "IX_DocumentSections_DocumentId");

        builder.HasOne(d => d.Document)
            .WithMany(p => p.DocumentSections)
            .HasForeignKey(d => d.DocumentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DocumentSections_Documents_DocumentId");
    }
}