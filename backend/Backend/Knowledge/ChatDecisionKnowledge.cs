namespace Backend.Knowledge;

public record ChatDecisionKnowledge(string Decision, string Reason) : IKnowledgeContent
{
    public string GetContent()
        => $"Decision: {Decision}\nReason: {Reason}";
}