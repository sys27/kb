namespace Backend.ContentExtractors;

public class PlainTextContentExtractor : IContentExtractor
{
    public async Task<string> Extract(string source, Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync(cancellationToken);
    }
}