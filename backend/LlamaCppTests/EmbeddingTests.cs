using LlamaCpp;

namespace LlamaCppTests;

public class EmbeddingTests
{
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
            EmbeddingsModel = "qwen3-embedding:0.6b",
        };

        client = new LlamaCppClient(httpClient, options);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        httpClient.Dispose();
    }

    [Test]
    public async Task EmbedSingleDocumentTest()
    {
        const string input = "Hello, World!";
        var result = await client.Embedding(input);

        Assert.That(result.Value, Is.EqualTo(input));
        Assert.That(result.Embedding, Has.Length.EqualTo(1024));
    }

    [Test]
    public async Task EmbedMultipleDocumentsTest()
    {
        var documents = new[]
        {
            "Hello, World!",
            "This is a test.",
            "This is another test.",
        };
        var results = await client.Embeddings(documents);

        foreach (var (document, result) in documents.Zip(results))
        {
            Assert.That(result.Value, Is.EqualTo(document));
            Assert.That(result.Embedding, Has.Length.EqualTo(1024));
        }
    }
}