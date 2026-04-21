namespace Backend.Messages.Tools.WebSearch;

public class WebSearchService
{
    private readonly HttpClient client;

    public WebSearchService(HttpClient client)
        => this.client = client;

    public async Task<SearchResponse?> Search(string query, CancellationToken cancellationToken = default)
    {
        query = Uri.EscapeDataString(query);
        var response = await client.GetAsync($"/search?q={query}&format=json", cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken);

        return result;
    }
}