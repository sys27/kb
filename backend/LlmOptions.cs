using Microsoft.Extensions.Options;

namespace Backend;

public class LlmOptions : IValidateOptions<LlmOptions>
{
    public const string Section = "LLM";

    public required string Endpoint { get; set; }

    public required string ApiKey { get; set; }

    public required string Model { get; set; }

    public required string EmbeddingModel { get; set; }

    public ValidateOptionsResult Validate(string? name, LlmOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Endpoint))
            failures.Add("Endpoint cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(options.ApiKey))
            failures.Add("ApiKey cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(options.Model))
            failures.Add("Model cannot be null or whitespace");

        if (string.IsNullOrWhiteSpace(options.EmbeddingModel))
            failures.Add("EmbeddingModel cannot be null or whitespace");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}