namespace Backend.ContentExtractors;

public interface IContentExtractor
{
    Task<string> Extract(string source, Stream stream, CancellationToken cancellationToken);
}