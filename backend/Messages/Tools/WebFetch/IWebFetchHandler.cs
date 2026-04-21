namespace Backend.Messages.Tools.WebFetch;

public interface IWebFetchHandler
{
    Task<string?> Fetch(string url, CancellationToken cancellationToken = default);

    ISet<string> SupportedDomains { get; }
}