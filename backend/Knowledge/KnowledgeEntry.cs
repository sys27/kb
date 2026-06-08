namespace Backend.Knowledge;

public record KnowledgeEntry(KnowledgeSource SourceType, int SourceId, IKnowledgeContent Content);