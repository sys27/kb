namespace LlamaCpp.Requests;

internal record RerankRequest(string Model, string Query, IReadOnlyList<string> Documents);