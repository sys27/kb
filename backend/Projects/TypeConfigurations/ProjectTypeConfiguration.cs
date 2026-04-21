using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Projects.TypeConfigurations;

public class ProjectTypeConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(e => e.Id)
            .HasName("PK_Projects");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(256);

        // TODO: remove
        builder.HasData(new Project { Id = 1, Name = "test" });
    }
}