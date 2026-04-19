using Microsoft.Extensions.Options;

namespace Backend.Chats;

public class SummarizationOptions : IValidateOptions<SummarizationOptions>
{
    public const string Section = "Summarization";

    public TimeSpan Delay { get; set; }

    public ValidateOptionsResult Validate(string? name, SummarizationOptions options)
    {
        var failures = new List<string>();

        if (options.Delay <= TimeSpan.Zero)
            failures.Add("Delay must be greater than zero");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}