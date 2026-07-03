namespace LlamaCpp;

public class RerankOptions
{
    public string? RerankingModel { get; init; }

    public int TopK { get; init; } = 10;

    public double RelevanceScoreThreshold { get; init; } = 0.5;
}