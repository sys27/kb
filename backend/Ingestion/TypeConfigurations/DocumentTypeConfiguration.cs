using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Ingestion.TypeConfigurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(e => e.Id)
            .HasName("PK_Documents");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Hash)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasDefaultValue(DocumentStatus.Pending)
            .HasConversion(new EnumToStringConverter<DocumentStatus>());

        builder.HasIndex(e => e.ProjectId, "IX_Documents_ProjectId");

        builder.HasOne(d => d.Project)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Documents_Projects_ProjectId");
    }
}