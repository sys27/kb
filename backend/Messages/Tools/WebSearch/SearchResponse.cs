using System.Text.Json.Serialization;

namespace Backend.Messages.Tools.WebSearch;

public class SearchResponse
{
    [JsonPropertyName("results")]
    public List<SearchResult> Results { get; set; } = [];
}