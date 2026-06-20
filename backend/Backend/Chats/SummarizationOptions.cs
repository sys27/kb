using Microsoft.Extensions.Options;

namespace Backend.Chats;

public class SummarizationOptions : IValidateOptions<SummarizationOptions>
{
    public const string Section = "Summarization";

    public bool Enabled { get; set; } = true;

    public TimeSpan Delay { get; set; }

    public TimeSpan SummaryInactivityWindow { get; set; }

    public ValidateOptionsResult Validate(string? name, SummarizationOptions options)
    {
        var failures = new List<string>();

        if (options.Delay <= TimeSpan.Zero)
            failures.Add("Delay must be greater than zero");

        if (options.SummaryInactivityWindow <= TimeSpan.Zero)
            failures.Add("SummaryInactivityWindow must be greater than zero");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}