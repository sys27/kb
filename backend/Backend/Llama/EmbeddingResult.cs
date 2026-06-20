namespace Backend.Llama;

public record EmbeddingResult<T>(T Value, float[] Embedding);