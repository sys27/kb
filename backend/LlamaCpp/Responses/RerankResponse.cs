namespace LlamaCpp.Responses;

internal sealed record RerankResponse(IReadOnlyList<RerankResult> Results);