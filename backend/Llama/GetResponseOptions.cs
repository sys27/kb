namespace Backend.Llama;

public class GetResponseOptions
{
    public string? Model { get; set; }

    public int? ThinkingBudgetTokens { get; set; }

    public static GetResponseOptions NoReasoning
        => new GetResponseOptions
        {
            ThinkingBudgetTokens = 1,
        };
}