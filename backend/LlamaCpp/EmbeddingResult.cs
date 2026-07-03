namespace LlamaCpp;

public record EmbeddingResult<T>(T Value, float[] Embedding);