using SmartReader;
using VersOne.Epub;
using VersOne.Epub.Options;

namespace Backend.ContentExtractors;

public class EpubContentExtractor : IContentExtractor
{
    public async Task<Content> Extract(string source, Stream stream, CancellationToken cancellationToken)
    {
        var book = await EpubReader.ReadBookAsync(stream, EpubReaderOptionsPreset.IGNORE_ALL_ERRORS);
        if (book?.Navigation is null)
            throw new InvalidOperationException("Failed to read EPUB");

        var sections = new List<ContentSection>(book.ReadingOrder.Count);
        foreach (var navigationItem in book.Navigation)
        {
            var chapter = navigationItem.HtmlContentFile;
            if (chapter?.ContentType is not EpubContentType.XHTML_1_1)
                continue;

            using var reader = new Reader($"file://{chapter.FilePath}", chapter.Content);
            var article = await reader.GetArticleAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(article.TextContent))
                continue;

            var section = new ContentSection(navigationItem.Title, article.TextContent);
            sections.Add(section);
        }

        return new Content(book.Title, sections);
    }
}