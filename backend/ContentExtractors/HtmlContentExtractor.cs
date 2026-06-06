using SmartReader;

namespace Backend.ContentExtractors;

public class HtmlContentExtractor : IContentExtractor
{
    public async Task<Content> Extract(string source, Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new Reader(source, stream);
        var article = await reader.GetArticleAsync(cancellationToken);

        // TODO: extract sections
        return new Content(article.Title, [new ContentSection(null, article.TextContent)]);
    }
}