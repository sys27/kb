namespace Backend.Messages.Tools.WebSearch.FetchHandlers;

public class WikipediaHandler : IWebFetchHandler
{
    private readonly HttpClient client;

    public WikipediaHandler(HttpClient client)
        => this.client = client;

    public async Task<WebFetchHandlerResult> Fetch(string url, CancellationToken cancellationToken = default)
    {
        // TODO: parse response
        // api.php?action=query&prop=extracts&explaintext=1&titles=Page_Title&format=json
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStreamAsync(cancellationToken);

        return new WebFetchHandlerResult(result, response.Content.Headers.ContentType?.MediaType ?? "text/plain");
    }

    public ISet<string> SupportedDomains => new HashSet<string>
    {
        "wikipedia.org"
    };
}