using Microsoft.Extensions.Options;

namespace Backend.Messages.Tools.WebSearch;

public class WebSearchOptions : IValidateOptions<WebSearchOptions>
{
    public const string Section = "WebSearch";

    public string? BaseUrl { get; set; }

    public ValidateOptionsResult Validate(string? name, WebSearchOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            failures.Add("BaseUrl cannot be null or whitespace");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}