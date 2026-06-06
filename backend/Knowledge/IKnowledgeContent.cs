using System.Text.Json.Serialization;

namespace Backend.Knowledge;

[JsonPolymorphic]
[JsonDerivedType(typeof(ChatDecisionKnowledge))]
[JsonDerivedType(typeof(ChatFactKnowledge))]
[JsonDerivedType(typeof(ChatSummaryKnowledge))]
[JsonDerivedType(typeof(ChatUserPreferenceKnowledge))]
[JsonDerivedType(typeof(DocumentChunkKnowledge))]
public interface IKnowledgeContent
{
    string GetContent();
}