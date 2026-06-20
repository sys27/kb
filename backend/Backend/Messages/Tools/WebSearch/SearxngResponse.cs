using System.Text.Json.Serialization;

namespace Backend.Messages.Tools.WebSearch;

internal sealed class SearxngResponse
{
    [JsonPropertyName("results")]
    public List<SearxngResult> Results { get; set; } = [];
}