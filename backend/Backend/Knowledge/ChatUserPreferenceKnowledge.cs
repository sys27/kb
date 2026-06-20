namespace Backend.Knowledge;

public record ChatUserPreferenceKnowledge(string Preference) : IKnowledgeContent
{
    public string GetContent()
        => Preference;
}