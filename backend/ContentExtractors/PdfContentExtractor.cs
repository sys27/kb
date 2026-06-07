using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.Outline;

namespace Backend.ContentExtractors;

public class PdfContentExtractor : IContentExtractor
{
    public Task<Content> Extract(string source, Stream stream, CancellationToken cancellationToken)
    {
        var sections = new List<ContentSection>();

        using var pdf = PdfDocument.Open(stream, new ParsingOptions { SkipMissingFonts = true });
        if (pdf.TryGetBookmarks(out var bookmarks))
        {
            var chapterBookmarks = bookmarks.Roots
                .OfType<DocumentBookmarkNode>()
                .OrderBy(b => b.PageNumber)
                .ToList();

            for (var i = 0; i < chapterBookmarks.Count; i++)
            {
                var bookmark = chapterBookmarks[i];
                var startPage = bookmark.PageNumber;
                var endPage = i < chapterBookmarks.Count - 1
                    ? chapterBookmarks[i + 1].PageNumber - 1
                    : pdf.NumberOfPages;

                var sb = new StringBuilder();

                for (var pageNumber = startPage; pageNumber <= endPage; pageNumber++)
                {
                    var page = pdf.GetPage(pageNumber);
                    sb.AppendLine(ContentOrderTextExtractor.GetText(page));
                }

                sections.Add(new ContentSection(bookmark.Title, sb.ToString()));
            }
        }
        else
        {
            var sb = new StringBuilder();

            foreach (var page in pdf.GetPages())
            {
                var text = ContentOrderTextExtractor.GetText(page);
                sb.Append(text);
            }

            sections.Add(new ContentSection(null, sb.ToString()));
        }

        return Task.FromResult(new Content(pdf.Information.Title, sections));
    }
}