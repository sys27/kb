using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Chats.TypeConfigurations;

public class ChatTopicTypeConfiguration : IEntityTypeConfiguration<ChatTopic>
{
    public void Configure(EntityTypeBuilder<ChatTopic> builder)
    {
        builder.ToTable("ChatTopics");

        builder.HasKey(e => e.Id)
            .HasName("PK_ChatTopics");

        builder.Property(e => e.Topic)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(e => e.ChatId)
            .HasDatabaseName("IX_ChatTopics_ChatId");

        builder.HasOne(e => e.Chat)
            .WithMany(e => e.Topics)
            .HasForeignKey(e => e.ChatId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ChatTopics_Chats_ChatId");
    }
}