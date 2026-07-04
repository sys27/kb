namespace LlamaCpp.Requests;

internal record EmbeddingRequest(string Model, IReadOnlyList<string> Input);