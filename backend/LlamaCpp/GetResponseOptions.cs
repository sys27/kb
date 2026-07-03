namespace LlamaCpp;

public class GetResponseOptions
{
    public string? Model { get; init; }

    public bool EnableThinking { get; init; } = true;

    public static GetResponseOptions NoThinking
        => new GetResponseOptions
        {
            EnableThinking = false,
        };
}