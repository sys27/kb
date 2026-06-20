namespace Backend.Knowledge;

public record ChatFactKnowledge(string Fact) : IKnowledgeContent
{
    public string GetContent()
        => Fact;
}