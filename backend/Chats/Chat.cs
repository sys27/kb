using Backend.Messages;
using Backend.Projects;

namespace Backend.Chats;

public class Chat
{
    public int Id { get; init; }

    public required string Name { get; set; }

    public ICollection<Message> Messages { get; init; } = [];

    public DateTime? LastMessageAt { get; set; }

    public string? Summary { get; set; }

    public DateTime? LastSummaryUpdate { get; set; }

    public int? ProjectId { get; set; }

    public Project? Project { get; init; }

    public ICollection<ChatTopic> Topics { get; init; } = [];

    public ICollection<ChatFact> Facts { get; init; } = [];

    public ICollection<ChatDecision> Decisions { get; init; } = [];

    public ICollection<ChatUserPreference> UserPreferences { get; init; } = [];

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

    public void UpdateTopics(IEnumerable<string> topics)
    {
        Topics.Clear();

        foreach (var topic in topics)
            Topics.Add(new ChatTopic { Topic = topic, ChatId = Id, Chat = this });
    }

    public void UpdateFacts(IEnumerable<string> facts)
    {
        Facts.Clear();

        foreach (var fact in facts)
            Facts.Add(new ChatFact { Fact = fact, ChatId = Id, Chat = this });
    }

    public void UpdateDecisions(IEnumerable<(string, string)> decisions)
    {
        Decisions.Clear();

        foreach (var (decision, reason) in decisions)
            Decisions.Add(new ChatDecision { Decision = decision, Reason = reason, ChatId = Id, Chat = this });
    }

    public void UpdateUserPreferences(IEnumerable<string> preferences)
    {
        UserPreferences.Clear();

        foreach (var preference in preferences)
            UserPreferences.Add(new ChatUserPreference { Preference = preference, ChatId = Id, Chat = this });
    }

    public void UpdateProject(int? projectId)
    {
        if (ProjectId != projectId)
            LastSummaryUpdate = null;

        ProjectId = projectId;
    }
}