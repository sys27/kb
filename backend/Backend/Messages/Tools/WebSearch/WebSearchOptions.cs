using Microsoft.Extensions.Options;

namespace Backend.Messages.Tools.WebSearch;

public class WebSearchOptions : IValidateOptions<WebSearchOptions>
{
    public const string Section = "WebSearch";

    public string? BaseUrl { get; set; }

    public int MaxResults { get; set; }

    public int RerankTopK { get; set; }

    public double SimilarityThreshold { get; set; }

    public ValidateOptionsResult Validate(string? name, WebSearchOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
            failures.Add("BaseUrl cannot be null or whitespace");

        if (options.MaxResults <= 0)
            failures.Add("MaxResults must be greater than 0");

        if (options.RerankTopK <= 0)
            failures.Add("RerankTopK must be greater than 0");

        if (options.SimilarityThreshold is < 0 or > 1)
            failures.Add("SimilarityThreshold must be between 0 and 1");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}