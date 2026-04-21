using System.Text.Json.Serialization;

namespace Backend.Messages.Tools.WebSearch;

public class SearchResult
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("positions")]
    public List<int> Positions { get; set; } = [];

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("publishedDate")]
    public DateTime? PublishedDate { get; set; }
}