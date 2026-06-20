namespace Backend.ContentExtractors;

public interface IContentExtractor
{
    Task<Content> Extract(string source, Stream stream, CancellationToken cancellationToken);
}