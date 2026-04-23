namespace Backend.Messages.Tools.WebFetch;

public partial class WikipediaHandler : IWebFetchHandler
{
    private readonly HttpClient client;

    public WikipediaHandler(HttpClient client)
        => this.client = client;

    public async Task<string?> Fetch(string url, CancellationToken cancellationToken = default)
    {
        // TODO: parse response
        // api.php?action=query&prop=extracts&explaintext=1&titles=Page_Title&format=json
        var response = await client.GetAsync(url, cancellationToken);
        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        return result;
    }

    public ISet<string> SupportedDomains => new HashSet<string>
    {
        "wikipedia.org"
    };
}