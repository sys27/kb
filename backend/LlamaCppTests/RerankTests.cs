using LlamaCpp;

namespace LlamaCppTests;

public class RerankTests
{
    private static readonly string[] sampleDocuments =
    [
        "The capital of France is Paris.",
        "Bananas are a good source of potassium.",
        "The Eiffel Tower is located in Paris, France.",
        "Photosynthesis converts sunlight into chemical energy.",
        "Paris is also the name of a city in Texas.",
    ];

    private HttpClient httpClient;
    private LlamaCppClient client;

    [OneTimeSetUp]
    public void Setup()
    {
        httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:11434/v1"),
            Timeout = TimeSpan.FromMinutes(5),
        };
        var options = new LlamaCppClientOptions
        {
            RerankingModel = "qwen3-reranking:0.6b",
        };

        client = new LlamaCppClient(httpClient, options);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        httpClient.Dispose();
    }

    [Test]
    public async Task RerankOrderDocumentsByRelevanceTest()
    {
        var results = await CollectAsync(
            client.Rerank("What is the capital of France?", sampleDocuments));

        Assert.That(results, Is.Not.Empty);
        Assert.That(results[0], Is.EqualTo(sampleDocuments[0]));
    }

    [Test]
    public async Task RerankTopKLimitsNumberOfResultsTest()
    {
        var options = new RerankOptions { TopK = 2 };
        var results = await CollectAsync(
            client.Rerank("What is the capital of France?", sampleDocuments, options));

        Assert.That(results, Has.Count.LessThanOrEqualTo(2));
    }

    [Test]
    public async Task RerankRelevanceScoreThresholdFiltersLowScoringDocumentsTest()
    {
        var lenientOptions = new RerankOptions { RelevanceScoreThreshold = 0f };
        var strictOptions = new RerankOptions { RelevanceScoreThreshold = 0.99f };

        var lenientResults = await CollectAsync(
            client.Rerank("What is the capital of France?", sampleDocuments, lenientOptions));
        var strictResults = await CollectAsync(
            client.Rerank("What is the capital of France?", sampleDocuments, strictOptions));

        Assert.That(strictResults.Count, Is.LessThanOrEqualTo(lenientResults.Count));
    }

    [Test]
    public async Task RerankEmptyDocumentsListTest()
    {
        var results = await CollectAsync(
            client.Rerank("What is the capital of France?", []));

        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task RerankDocumentSelectorTest()
    {
        var documents = sampleDocuments
            .Select((text, index) => new Document(index, text))
            .ToArray();

        var results = await CollectAsync(
            client.Rerank(
                "What is the capital of France?",
                documents,
                d => d.Text));

        Assert.That(results, Is.Not.Empty);
        Assert.That(results[0].Text, Does.Contain("capital of France"));
    }

    private static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source)
    {
        var results = new List<T>();
        await foreach (var item in source)
            results.Add(item);

        return results;
    }

    private record Document(int Index, string Text);
}