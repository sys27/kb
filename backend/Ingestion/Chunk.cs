namespace Backend.Ingestion;

public readonly record struct Chunk(int Start, int Length)
{
    public const int MaxTokens = 512;
    public const int Overlap = 50;
}