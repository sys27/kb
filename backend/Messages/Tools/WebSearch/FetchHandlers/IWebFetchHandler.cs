namespace Backend.Messages.Tools.WebSearch.FetchHandlers;

public interface IWebFetchHandler
{
    Task<WebFetchHandlerResult> Fetch(string url, CancellationToken cancellationToken = default);

    ISet<string> SupportedDomains { get; }
}