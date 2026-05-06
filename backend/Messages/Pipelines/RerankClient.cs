using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Backend.Messages.Pipelines;

public class RerankClient
{
    private readonly LlmOptions llmOptions;
    private readonly HttpClient httpClient;

    public RerankClient(IOptions<LlmOptions> llmOptions, HttpClient httpClient)
    {
        this.llmOptions = llmOptions.Value;
        this.httpClient = httpClient;
    }

    public async IAsyncEnumerable<string> Rerank(
        string query,
        IReadOnlyList<string> documents,
        int topK,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("Query cannot be null or whitespace", nameof(query));

        if (documents is null)
            throw new ArgumentNullException(nameof(documents));

        if (documents.Count == 0)
            yield break;

        var request = new
        {
            model = llmOptions.RerankingModel,
            query,
            documents,
        };
        var httpResponse = await httpClient.PostAsJsonAsync("/reranking", request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var rerankResponse = await httpResponse.Content.ReadFromJsonAsync<RerankResponse>(cancellationToken);
        if (rerankResponse is null)
            yield break;

        // TODO: config?
        var topDocuments = rerankResponse.Results
            .Where(x => x.RelevanceScore >= 0.5)
            .OrderByDescending(x => x.RelevanceScore)
            .Take(topK);

        foreach (var document in topDocuments)
            if (document.Index >= 0 && document.Index < documents.Count)
                yield return documents[document.Index];
    }

    private sealed record RerankResponse(IReadOnlyList<RerankResult> Results);

    private sealed record RerankResult(
        int Index,
        [property: JsonPropertyName("relevance_score")]
        double RelevanceScore);
}