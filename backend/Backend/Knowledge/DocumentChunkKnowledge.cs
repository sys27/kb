namespace Backend.Knowledge;

public record DocumentChunkKnowledge(string? Title, string? Header, string Chunk) : IKnowledgeContent
{
    public string GetContent()
        => Chunk;
}