using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Chats;

public class ChatTypeConfiguration : IEntityTypeConfiguration<Chat>
{
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder.ToTable("Chats");

        builder.HasKey(e => e.Id)
            .HasName("PK_Chats");

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(e => e.ProjectId, "IX_Chats_ProjectId");

        builder.HasOne(d => d.Project)
            .WithMany(p => p.Chats)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}