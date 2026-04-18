using Markdig;
using Markdig.Syntax;

namespace Backend.Ingestion;

public class MarkdownChunker : IChunker
{
    public IReadOnlyList<Chunk> Split(string text)
    {
        // TODO: ???
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UsePreciseSourceLocation()
            .DisableHtml()
            .Build();
        var document = Markdown.Parse(text, pipeline);
        var headings = document.Descendants<HeadingBlock>();
        foreach (var heading in headings)
        {
        }

        throw new NotImplementedException();
    }
}