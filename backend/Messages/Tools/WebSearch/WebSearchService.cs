using System.Runtime.CompilerServices;
using Backend.Llama;
using Microsoft.Extensions.Options;

namespace Backend.Messages.Tools.WebSearch;

public class WebSearchService
{
    private readonly WebSearchOptions webSearchOptions;
    private readonly HttpClient client;
    private readonly LlamaCppClient llamaCppClient;

    public WebSearchService(
        IOptions<WebSearchOptions> webSearchOptions,
        HttpClient client,
        LlamaCppClient llamaCppClient)
    {
        this.webSearchOptions = webSearchOptions.Value;
        this.client = client;
        this.llamaCppClient = llamaCppClient;
    }

    public async IAsyncEnumerable<SearchResult> Search(
        string query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "q", query },
            { "format", "json" }
        });
        var httpResponse = await client.PostAsync("/search", content, cancellationToken);
        var searchResponse = await httpResponse.Content.ReadFromJsonAsync<SearxngResponse>(cancellationToken);
        var searxngResults = searchResponse?.Results
            .OrderByDescending(x => x.Score)
            .Take(webSearchOptions.MaxResults)
            .ToArray() ?? [];

        var reranked = llamaCppClient.Rerank(
            query,
            searxngResults,
            x => x.Content ?? string.Empty,
            webSearchOptions.RerankTopK,
            cancellationToken);

        await foreach (var item in reranked)
            yield return new SearchResult(item.Title!, item.Url!, item.Content);
    }
}