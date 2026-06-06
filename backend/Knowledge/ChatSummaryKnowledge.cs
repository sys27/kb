namespace Backend.Knowledge;

public record ChatSummaryKnowledge(string? Summary) : IKnowledgeContent
{
    public string GetContent()
        => Summary ?? string.Empty;
}