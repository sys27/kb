using Microsoft.Extensions.Options;

namespace Backend.Ingestion;

public class IngestionOptions : IValidateOptions<IngestionOptions>
{
    public const string Section = "Ingestion";

    public required string ConnectionString { get; set; }

    public required string Path { get; set; }

    public TimeSpan Delay { get; set; }

    public ValidateOptionsResult Validate(string? name, IngestionOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            failures.Add("ConnectionString cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(options.Path))
            failures.Add("Path cannot be null or whitespace");

        if (options.Delay <= TimeSpan.Zero)
            failures.Add("Delay must be greater than zero");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}