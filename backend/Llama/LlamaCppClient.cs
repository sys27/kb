using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Backend.Llama;

public class LlamaCppClient
{
    private readonly LlmOptions llmOptions;
    private readonly HttpClient httpClient;

    public LlamaCppClient(IOptions<LlmOptions> llmOptions, HttpClient httpClient)
    {
        this.llmOptions = llmOptions.Value;
        this.httpClient = httpClient;
    }

    public Task<string> GetResponse(
        LlamaMessage message,
        GetResponseOptions? options,
        CancellationToken cancellationToken = default)
        => GetResponse([message], options, cancellationToken);

    public async Task<string> GetResponse(
        IReadOnlyList<LlamaMessage> messages,
        GetResponseOptions? options,
        CancellationToken cancellationToken = default)
    {
        var model = options?.Model ?? llmOptions.Model;
        var thinkingBudgetTokens = options?.ThinkingBudgetTokens ?? -1;

        var request = new
        {
            model,
            thinking_budget_tokens = thinkingBudgetTokens,
            messages
        };
        var httpResponse = await httpClient.PostAsJsonAsync("/v1/chat/completions", request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<CompletionResponse>(cancellationToken);
        if (response is null || response.Choices.Length == 0)
            throw new InvalidOperationException("No response from Llama");

        return response.Choices[0].Message.Content;
    }

    public async IAsyncEnumerable<string> GetResponseStream(
        IReadOnlyList<LlamaMessage> messages,
        GetResponseOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var model = options?.Model ?? llmOptions.Model;
        var thinkingBudgetTokens = options?.ThinkingBudgetTokens ?? -1;

        var request = new
        {
            stream = true,
            model,
            thinking_budget_tokens = thinkingBudgetTokens,
            messages
        };
        var httpMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions");
        httpMessage.Headers.Accept.ParseAdd("text/event-stream");
        httpMessage.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };
        httpMessage.Headers.Connection.ParseAdd("keep-alive");
        httpMessage.Content = JsonContent.Create(request, new MediaTypeHeaderValue("application/json"));

        var httpResponse = await httpClient.SendAsync(
            httpMessage,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        // TODO: pipelines?
        await using var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
                yield break;

            throw new NotImplementedException("Streaming not implemented yet");
        }
    }

    public IAsyncEnumerable<string> Rerank(
        string query,
        IReadOnlyList<string> documents,
        int topK,
        CancellationToken cancellationToken = default)
        => Rerank(query, documents, x => x, topK, cancellationToken);

    public async IAsyncEnumerable<T> Rerank<T>(
        string query,
        IReadOnlyList<T> documents,
        Func<T, string> documentSelector,
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
            documents = documents.Select(documentSelector),
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

    private sealed record CompletionResponse(ChoicesResponse[] Choices);

    private sealed record ChoicesResponse(LlamaMessage Message);

    private sealed record RerankResponse(IReadOnlyList<RerankResult> Results);

    private sealed record RerankResult(
        int Index,
        [property: JsonPropertyName("relevance_score")]
        double RelevanceScore);
}