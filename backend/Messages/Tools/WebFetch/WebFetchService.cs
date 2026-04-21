namespace Backend.Messages.Tools.WebFetch;

public class WebFetchService
{
    private readonly IEnumerable<IWebFetchHandler> handlers;

    public WebFetchService(IEnumerable<IWebFetchHandler> handlers)
        => this.handlers = handlers;

    public async Task<string?> Fetch(string url, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(url);
        var candidate = handlers
            .Select(h => new
            {
                Handler = h,
                BestMatchLength = h.SupportedDomains
                    .Where(pattern => uri.Host.EndsWith(pattern, StringComparison.OrdinalIgnoreCase))
                    .Max(x => (int?)x.Length)
            })
            .Where(x => x.BestMatchLength is not null)
            .MaxBy(x => x.BestMatchLength);

        return await candidate!.Handler.Fetch(url, cancellationToken);
    }
}