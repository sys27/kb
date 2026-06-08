namespace Backend.Knowledge;

[Flags]
public enum KnowledgeSource
{
    DocumentChunk = 1 << 0,
    ChatSummary = 1 << 1,
    ChatFact = 1 << 2,
    ChatDecision = 1 << 3,
    ChatUserPreference = 1 << 4,

    All = DocumentChunk | ChatSummary | ChatFact | ChatDecision | ChatUserPreference,
}