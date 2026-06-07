namespace Backend.Llama;

public class GetResponseOptions
{
    public string? Model { get; set; }

    public bool EnableThinking { get; set; } = true;

    public static GetResponseOptions NoThinking
        => new GetResponseOptions
        {
            EnableThinking = false,
        };
}