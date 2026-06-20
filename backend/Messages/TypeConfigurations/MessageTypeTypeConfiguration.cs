using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Messages.TypeConfigurations;

public class MessageTypeTypeConfiguration : IEntityTypeConfiguration<MessageType>
{
    public void Configure(EntityTypeBuilder<MessageType> builder)
    {
        builder.ToTable("MessageTypes");

        builder.HasKey(e => e.Id)
            .HasName("PK_MessageTypes");

        builder.Property(e => e.Role)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(e => e.Kind)
            .IsRequired()
            .HasMaxLength(32);

        builder.HasData(
            new MessageType { Id = MessageType.SystemId, Role = "System", Kind = "Text" },
            new MessageType { Id = MessageType.AssistantReasoningId, Role = "Assistant", Kind = "Reasoning" },
            new MessageType { Id = MessageType.AssistantAnswerId, Role = "Assistant", Kind = "Answer" },
            new MessageType { Id = MessageType.UserContextId, Role = "User", Kind = "Context" },
            new MessageType { Id = MessageType.UserRequestId, Role = "User", Kind = "Request" },
            new MessageType { Id = MessageType.ToolCallId, Role = "Tool", Kind = "Call" },
            new MessageType { Id = MessageType.ToolResultId, Role = "Tool", Kind = "Result" },
            new MessageType { Id = MessageType.AddSource, Role = "User", Kind = "AddSource" }
        );
    }
}