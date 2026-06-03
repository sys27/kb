namespace Backend.Messages.Tools.WebSearch.FetchHandlers;

public class DefaultHandler : IWebFetchHandler
{
    private readonly HttpClient client;

    public DefaultHandler(HttpClient client)
        => this.client = client;

    public async Task<WebFetchHandlerResult> Fetch(string url, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStreamAsync(cancellationToken);

        return new WebFetchHandlerResult(result, response.Content.Headers.ContentType?.MediaType ?? "text/plain");
    }

    public ISet<string> SupportedDomains => new HashSet<string>
    {
        ""
    };
}