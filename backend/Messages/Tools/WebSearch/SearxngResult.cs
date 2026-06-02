using System.Text.Json.Serialization;

namespace Backend.Messages.Tools.WebSearch;

internal sealed class SearxngResult
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }
}