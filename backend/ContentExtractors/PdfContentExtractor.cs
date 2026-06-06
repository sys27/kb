using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Backend.ContentExtractors;

public class PdfContentExtractor : IContentExtractor
{
    public Task<Content> Extract(string source, Stream stream, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        using var pdf = PdfDocument.Open(stream, new ParsingOptions { SkipMissingFonts = true });
        foreach (var page in pdf.GetPages())
        {
            var text = ContentOrderTextExtractor.GetText(page);
            sb.Append(text);
        }

        // TODO: extract title and sections
        return Task.FromResult(new Content(null, [new ContentSection(null, sb.ToString())]));
    }
}