using Microsoft.Extensions.Options;

namespace Backend.Ingestion;

public class IngestionOptions : IValidateOptions<IngestionOptions>
{
    public const string Section = "Ingestion";

    public bool IsIngestionEnabled { get; set; } = true;

    public bool IsDocumentDiscoveryEnabled { get; set; } = true;

    public required string Path { get; set; }

    public TimeSpan IngestionDelay { get; set; }

    public TimeSpan DocumentDiscoveryDelay { get; set; }

    public ValidateOptionsResult Validate(string? name, IngestionOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Path))
            failures.Add("Path cannot be null or whitespace");

        if (options.IngestionDelay <= TimeSpan.Zero)
            failures.Add("IngestionDelay must be greater than zero");

        if (options.DocumentDiscoveryDelay <= TimeSpan.Zero)
            failures.Add("DocumentDiscoveryDelay must be greater than zero");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}