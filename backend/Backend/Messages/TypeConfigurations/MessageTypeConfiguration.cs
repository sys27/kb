using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Messages.TypeConfigurations;

public class MessageTypeConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(e => e.Id)
            .HasName("PK_Messages");

        builder.HasIndex(e => e.MessageTypeId, "IX_Messages_MessageTypeId");

        builder.HasOne(d => d.MessageType)
            .WithMany(p => p.Messages)
            .HasForeignKey(d => d.MessageTypeId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Messages_MessageTypes_MessageTypeId");

        builder.HasIndex(e => e.ChatId, "IX_Messages_ChatId");

        builder.HasOne(d => d.Chat)
            .WithMany(p => p.Messages)
            .HasForeignKey(d => d.ChatId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Messages_Chats_ChatId");
    }
}