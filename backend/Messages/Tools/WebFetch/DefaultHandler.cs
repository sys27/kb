namespace Backend.Messages.Tools.WebFetch;

public partial class DefaultHandler : IWebFetchHandler
{
    private readonly HttpClient client;

    public DefaultHandler(HttpClient client)
        => this.client = client;

    public async Task<string?> Fetch(string url, CancellationToken cancellationToken = default)
    {
        // TODO: parse response
        var response = await client.GetAsync(url, cancellationToken);
        var result = await response.Content.ReadAsStringAsync(cancellationToken);

        return result;
    }

    public ISet<string> SupportedDomains => new HashSet<string>
    {
        ""
    };
}