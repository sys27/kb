namespace LlamaCpp.Responses;

internal sealed record EmbeddingsResponse(int Index, double[][] Embedding);