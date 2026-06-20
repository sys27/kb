namespace Backend.ContentExtractors;

public class ContentExtractorFactory
{
    private readonly IServiceProvider serviceProvider;

    public ContentExtractorFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public IContentExtractor Create(string contentType)
        => contentType switch
        {
            "application/epub+zip" => serviceProvider.GetRequiredService<EpubContentExtractor>(),
            "application/pdf" => serviceProvider.GetRequiredService<PdfContentExtractor>(),
            "text/markdown" => serviceProvider.GetRequiredService<MarkdownContentExtractor>(),
            "text/html" => serviceProvider.GetRequiredService<HtmlContentExtractor>(),

            _ => serviceProvider.GetRequiredService<PlainTextContentExtractor>(),
        };
}