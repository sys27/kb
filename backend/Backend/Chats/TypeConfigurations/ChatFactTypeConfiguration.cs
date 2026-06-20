using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Chats.TypeConfigurations;

public class ChatFactTypeConfiguration : IEntityTypeConfiguration<ChatFact>
{
    public void Configure(EntityTypeBuilder<ChatFact> builder)
    {
        builder.ToTable("ChatFacts");

        builder.HasKey(e => e.Id)
            .HasName("PK_ChatFacts");

        builder.Property(e => e.Fact)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(e => e.ChatId)
            .HasDatabaseName("IX_ChatFacts_ChatId");

        builder.HasOne(e => e.Chat)
            .WithMany(e => e.Facts)
            .HasForeignKey(e => e.ChatId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ChatFacts_Chats_ChatId");
    }
}