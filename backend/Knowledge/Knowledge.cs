namespace Backend.Knowledge;

public record Knowledge(KnowledgeSource SourceType, int SourceId, IKnowledgeContent Content);