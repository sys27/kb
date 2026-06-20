namespace Backend.WebFetchHandlers;

public class WebFetchHandlerFactory
{
    private readonly IEnumerable<IWebFetchHandler> handlers;

    public WebFetchHandlerFactory(IEnumerable<IWebFetchHandler> handlers)
        => this.handlers = handlers;

    public IWebFetchHandler GetHandler(string url)
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

        return candidate!.Handler;
    }
}