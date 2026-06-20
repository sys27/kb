using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Chats.TypeConfigurations;

public class ChatUserPreferenceTypeConfiguration : IEntityTypeConfiguration<ChatUserPreference>
{
    public void Configure(EntityTypeBuilder<ChatUserPreference> builder)
    {
        builder.ToTable("ChatUserPreferences");

        builder.HasKey(p => p.Id)
            .HasName("PK_ChatUserPreferences");

        builder.Property(p => p.Preference)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(p => p.ChatId)
            .HasDatabaseName("IX_ChatUserPreferences_ChatId");

        builder.HasOne(p => p.Chat)
            .WithMany(c => c.UserPreferences)
            .HasForeignKey(p => p.ChatId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ChatUserPreferences_Chats_ChatId");
    }
}