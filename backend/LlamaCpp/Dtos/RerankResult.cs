using System.Text.Json.Serialization;

namespace LlamaCpp;

internal sealed record RerankResult(
    int Index,
    [property: JsonPropertyName("relevance_score")]
    double RelevanceScore);