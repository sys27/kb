namespace Backend.ContentExtractors;

public class ContentExtractorFactory
{
    private readonly IServiceProvider serviceProvider;

    public ContentExtractorFactory(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public IContentExtractor Create(string contentType)
        => contentType switch
        {
            "text/html" => serviceProvider.GetRequiredService<HtmlContentExtractor>(),
            "application/pdf" => serviceProvider.GetRequiredService<PdfContentExtractor>(),

            _ => serviceProvider.GetRequiredService<PlainTextContentExtractor>(),
        };
}