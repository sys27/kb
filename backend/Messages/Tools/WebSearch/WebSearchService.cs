using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using Backend.ContentExtractors;
using Backend.Llama;
using Backend.WebFetchHandlers;
using Microsoft.Extensions.Options;

namespace Backend.Messages.Tools.WebSearch;

public class WebSearchService
{
    private readonly WebSearchOptions webSearchOptions;
    private readonly HttpClient client;
    private readonly LlamaCppClient llamaCppClient;
    private readonly WebFetchHandlerFactory webFetchHandlerFactory;
    private readonly ContentExtractorFactory contentExtractorFactory;
    private readonly TextChunker textChunker;

    public WebSearchService(
        IOptions<WebSearchOptions> webSearchOptions,
        HttpClient client,
        LlamaCppClient llamaCppClient,
        WebFetchHandlerFactory webFetchHandlerFactory,
        ContentExtractorFactory contentExtractorFactory,
        TextChunker textChunker)
    {
        this.webSearchOptions = webSearchOptions.Value;
        this.client = client;
        this.llamaCppClient = llamaCppClient;
        this.webFetchHandlerFactory = webFetchHandlerFactory;
        this.contentExtractorFactory = contentExtractorFactory;
        this.textChunker = textChunker;
    }

    public async IAsyncEnumerable<SearchResult> Search(
        string query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var searxngResults = await QuerySearxng(query, cancellationToken);
        var reranked = llamaCppClient.Rerank(
            query,
            searxngResults,
            x => x.Content ?? string.Empty,
            new RerankOptions { TopK = webSearchOptions.RerankTopK },
            cancellationToken);

        await foreach (var item in reranked)
        {
            var chunks = await FetchContentChunks(query, item.Url, cancellationToken);

            yield return new SearchResult(item.Url, item.Title, chunks);
        }
    }

    private async Task<SearxngResult[]> QuerySearxng(string query, CancellationToken cancellationToken)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "q", query },
            { "format", "json" }
        });
        var httpResponse = await client.PostAsync("/search", content, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var searchResponse = await httpResponse.Content.ReadFromJsonAsync<SearxngResponse>(cancellationToken);
        var searxngResults = searchResponse?.Results
            .OrderByDescending(x => x.Score)
            .Take(webSearchOptions.MaxResults)
            .ToArray() ?? [];

        return searxngResults;
    }

    private async Task<IReadOnlyList<string>> FetchContentChunks(
        string query,
        string url,
        CancellationToken cancellationToken)
    {
        var handler = webFetchHandlerFactory.GetHandler(url);
        var (stream, _, contentType) = await handler.Fetch(url, cancellationToken);

        await using (stream)
        {
            var extractor = contentExtractorFactory.Create(contentType);
            var content = await extractor.Extract(url, stream, cancellationToken);
            var chunkedContent = content.Sections
                .Where(x => !string.IsNullOrWhiteSpace(x.Content))
                .SelectMany(x => textChunker.Split(x.Content).Select(c => (x.Content, Chunk: c)))
                .Select(x => x.Content.Substring(x.Chunk.Start, x.Chunk.Length))
                .ToArray();

            var similarChunks = new List<string>();
            var (_, queryEmbedding) = await llamaCppClient.Embedding(query, cancellationToken);
            var contentEmbeddings = await llamaCppClient.Embeddings(chunkedContent, cancellationToken);
            foreach (var (chunk, contentEmbedding) in contentEmbeddings)
            {
                var similarity = TensorPrimitives.CosineSimilarity(queryEmbedding, contentEmbedding);
                if (similarity > webSearchOptions.SimilarityThreshold)
                    similarChunks.Add(chunk);
            }

            var topResults = await llamaCppClient
                .Rerank(query, similarChunks, new RerankOptions { TopK = 10 }, cancellationToken)
                .ToArrayAsync(cancellationToken);

            return topResults;
        }
    }
}