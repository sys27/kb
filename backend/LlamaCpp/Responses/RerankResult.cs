using System.Text.Json.Serialization;

namespace LlamaCpp.Responses;

internal sealed record RerankResult(
    int Index,
    [property: JsonPropertyName("relevance_score")]
    double RelevanceScore);