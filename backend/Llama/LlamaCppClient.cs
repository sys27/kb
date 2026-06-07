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
        var enableThinking = options?.EnableThinking ?? true;

        var request = new
        {
            model,
            chat_template_kwargs = new
            {
                enable_thinking = enableThinking,
            },
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
        var enableThinking = options?.EnableThinking ?? true;

        var request = new
        {
            stream = true,
            model,
            chat_template_kwargs = new
            {
                enable_thinking = enableThinking,
            },
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

    public async Task<EmbeddingResult<T>> Embedding<T>(T input, CancellationToken cancellationToken = default)
    {
        var embeddings = await Embeddings([input], cancellationToken);

        return embeddings[0];
    }

    public async Task<IReadOnlyList<EmbeddingResult<T>>> Embeddings<T>(
        IReadOnlyList<T> documents,
        CancellationToken cancellationToken = default)
    {
        if (documents is null)
            throw new ArgumentNullException(nameof(documents));

        if (documents.Count == 0)
            return [];

        var request = new
        {
            model = llmOptions.EmbeddingModel,
            input = documents,
        };
        var httpResponse = await httpClient.PostAsJsonAsync("/embeddings", request, cancellationToken);
        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<EmbeddingsResponse[]>(cancellationToken);
        if (response is null)
            throw new InvalidOperationException("No embeddings returned from Llama");

        if (response.Length != documents.Count)
            throw new InvalidOperationException("Response length does not match input length");

        var embeddings = response
            .Select(x => new EmbeddingResult<T>(
                documents[x.Index],
                x.Embedding.FirstOrDefault()?.Select(e => (float)e).ToArray() ?? []))
            .ToArray();

        return embeddings;
    }

    public IAsyncEnumerable<string> Rerank(
        string query,
        IReadOnlyList<string> documents,
        RerankOptions? options = null,
        CancellationToken cancellationToken = default)
        => Rerank(query, documents, x => x, options, cancellationToken);

    public async IAsyncEnumerable<T> Rerank<T>(
        string query,
        IReadOnlyList<T> documents,
        Func<T, string> documentSelector,
        RerankOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new RerankOptions();

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

        var topDocuments = rerankResponse.Results
            .Where(x => x.RelevanceScore >= options.RelevanceScoreThreshold)
            .OrderByDescending(x => x.RelevanceScore)
            .Take(options.TopK);

        foreach (var document in topDocuments)
            if (document.Index >= 0 && document.Index < documents.Count)
                yield return documents[document.Index];
    }

    private sealed record CompletionResponse(ChoicesResponse[] Choices);

    private sealed record ChoicesResponse(LlamaMessage Message);

    private sealed record EmbeddingsResponse(int Index, double[][] Embedding);

    private sealed record RerankResponse(IReadOnlyList<RerankResult> Results);

    private sealed record RerankResult(
        int Index,
        [property: JsonPropertyName("relevance_score")]
        double RelevanceScore);
}