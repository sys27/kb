using Backend.Messages;
using Backend.Projects;

namespace Backend.Chats;

public class Chat
{
    public int Id { get; init; }

    public required string Title { get; set; }

    public ICollection<Message> Messages { get; init; } = [];

    public DateTime? LastMessageAt { get; set; }

    public string? Summary { get; set; }

    public DateTime? LastSummaryUpdate { get; set; }

    public int? ProjectId { get; init; }

    public Project? Project { get; init; }

    public void AddMessage(Message message)
    {
        Messages.Add(message);
        LastMessageAt = message.Timestamp;
    }

    public void UpdateSummary(string summary)
    {
        Summary = summary;
        LastSummaryUpdate = DateTime.UtcNow;
    }
}