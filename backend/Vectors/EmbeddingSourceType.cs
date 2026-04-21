namespace Backend.Vectors;

public enum EmbeddingSourceType
{
    DocumentChunk = 1,
    ChatSummary = 2,
    ChatFact = 3,
    ChatDecision = 4,
    ChatUserPreference = 5,
}