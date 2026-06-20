namespace Backend.Llama;

public class RerankOptions
{
    public int TopK { get; set; } = 10;

    public double RelevanceScoreThreshold { get; set; } = 0.5;
}