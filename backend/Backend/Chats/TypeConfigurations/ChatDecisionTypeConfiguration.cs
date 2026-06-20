using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Chats.TypeConfigurations;

public class ChatDecisionTypeConfiguration : IEntityTypeConfiguration<ChatDecision>
{
    public void Configure(EntityTypeBuilder<ChatDecision> builder)
    {
        builder.ToTable("ChatDecisions");

        builder.HasKey(e => e.Id)
            .HasName("PK_ChatDecisions");

        builder.Property(e => e.Decision)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(e => e.ChatId)
            .HasDatabaseName("IX_ChatDecisions_ChatId");

        builder.HasOne(e => e.Chat)
            .WithMany(e => e.Decisions)
            .HasForeignKey(e => e.ChatId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_ChatDecisions_Chats_ChatId");
    }
}