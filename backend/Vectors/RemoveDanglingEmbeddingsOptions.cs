using Microsoft.Extensions.Options;

namespace Backend.Vectors;

public class RemoveDanglingEmbeddingsOptions : IValidateOptions<RemoveDanglingEmbeddingsOptions>
{
    public const string Section = "RemoveDanglingEmbeddings";

    public bool Enabled { get; set; }

    public int BatchSize { get; set; }

    public TimeSpan Delay { get; set; }

    public ValidateOptionsResult Validate(string? name, RemoveDanglingEmbeddingsOptions embeddingsOptions)
    {
        var failures = new List<string>();

        if (embeddingsOptions.BatchSize <= 0)
            failures.Add("BatchSize must be greater than zero");

        if (embeddingsOptions.Delay <= TimeSpan.Zero)
            failures.Add("Delay must be greater than zero");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}