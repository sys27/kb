namespace Backend.WebFetchHandlers;

public class DefaultHandler : IWebFetchHandler
{
    protected readonly HttpClient client;

    public DefaultHandler(HttpClient client)
        => this.client = client;

    public async Task<WebFetchHandlerResult> Fetch(string url, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStreamAsync(cancellationToken);
        var fileName = GetFileName(response, url);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "text/plain";

        return new WebFetchHandlerResult(result, fileName, contentType);
    }

    private string GetFileName(HttpResponseMessage response, string url)
    {
        var contentDisposition = response.Content.Headers.ContentDisposition;
        var fileName = contentDisposition?.FileNameStar ??
                       contentDisposition?.FileName;

        if (!string.IsNullOrWhiteSpace(fileName))
            return fileName.Trim('"');

        var uri = new Uri(url);
        var urlFileName = Path.GetFileName(uri.LocalPath);
        if (!string.IsNullOrWhiteSpace(urlFileName))
        {
            return Path.HasExtension(urlFileName)
                ? urlFileName
                : $"{urlFileName}.html";
        }

        var host = uri.Host;
        var path = uri.AbsolutePath.Trim('/');

        return string.IsNullOrEmpty(path)
            ? $"{host}.html"
            : $"{host}_{path.Replace('/', '_')}.html";
    }

    public virtual ISet<string> SupportedDomains => new HashSet<string>
    {
        ""
    };
}