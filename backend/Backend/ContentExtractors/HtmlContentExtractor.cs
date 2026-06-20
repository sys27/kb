using SmartReader;

namespace Backend.ContentExtractors;

public class HtmlContentExtractor : IContentExtractor
{
    public async Task<Content> Extract(string source, Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new Reader(source, stream);
        var article = await reader.GetArticleAsync(cancellationToken);

        var sections = new List<ContentSection>();
        if (article.Excerpt is not null)
            sections.Add(new ContentSection(null, article.Excerpt));

        sections.Add(new ContentSection(null, article.TextContent));

        // TODO: extract sections
        return new Content(article.Title, sections);
    }
}