namespace Backend.ContentExtractors;

public class PlainTextContentExtractor : IContentExtractor
{
    public async Task<Content> Extract(string source, Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync(cancellationToken);

        return new Content(null, [new ContentSection(null, content)]);
    }
}