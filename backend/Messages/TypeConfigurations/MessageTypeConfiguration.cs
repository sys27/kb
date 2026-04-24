using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backend.Messages.TypeConfigurations;

public class MessageTypeConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(e => e.Id)
            .HasName("PK_Messages");

        builder.Property(e => e.Role)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<MessageRole>());

        builder.Property(e => e.Kind)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<MessageKind>());

        builder.HasIndex(e => e.ChatId, "IX_Messages_ChatId");

        builder.HasOne(d => d.Chat)
            .WithMany(p => p.Messages)
            .HasForeignKey(d => d.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}