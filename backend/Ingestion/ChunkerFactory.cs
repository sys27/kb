namespace Backend.Ingestion;

public class ChunkerFactory
{
    private readonly IServiceProvider serviceProvider;

    public ChunkerFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public IChunker Create(string file)
    {
        var fileInfo = new FileInfo(file);

        return fileInfo.Extension switch
        {
            ".txt" => serviceProvider.GetRequiredService<TextChunker>(),
            ".md" => serviceProvider.GetRequiredService<MarkdownChunker>(),
            _ => throw new InvalidOperationException(),
        };
    }
}