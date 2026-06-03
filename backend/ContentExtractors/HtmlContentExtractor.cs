using SmartReader;

namespace Backend.ContentExtractors;

public class HtmlContentExtractor : IContentExtractor
{
    public async Task<string> Extract(string source, Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new Reader(source, stream);
        var article = await reader.GetArticleAsync(cancellationToken);

        return article.TextContent;
    }
}