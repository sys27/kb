namespace Backend.Messages.Tools.WebSearch;

public class WebSearchService
{
    private readonly HttpClient client;

    public WebSearchService(HttpClient client)
        => this.client = client;

    public async Task<SearchResponse?> Search(string query, CancellationToken cancellationToken = default)
    {
        query = Uri.EscapeDataString(query);
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "q", query },
            { "format", "json" }
        });
        var response = await client.PostAsync("/search", content, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken);

        return result;
    }
}