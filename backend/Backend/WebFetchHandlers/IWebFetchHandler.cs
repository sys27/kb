namespace Backend.WebFetchHandlers;

public interface IWebFetchHandler
{
    Task<WebFetchHandlerResult> Fetch(string url, CancellationToken cancellationToken = default);

    ISet<string> SupportedDomains { get; }
}