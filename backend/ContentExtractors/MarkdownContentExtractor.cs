using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Backend.ContentExtractors;

public class MarkdownContentExtractor : IContentExtractor
{
    public async Task<Content> Extract(string source, Stream stream, CancellationToken cancellationToken)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UsePreciseSourceLocation()
            .UseYamlFrontMatter()
            .DisableHtml()
            .Build();

        using var reader = new StreamReader(stream);
        var markdown = await reader.ReadToEndAsync(cancellationToken);
        var document = Markdown.Parse(markdown, pipeline);
        var sections = new List<ContentSection>();

        var topLevelHeaders = document
            .OfType<HeadingBlock>()
            .Where(h => h.Level == 1)
            .OrderBy(h => h.Span.Start)
            .ToList();

        if (topLevelHeaders.Count > 0 && topLevelHeaders[0].Span.Start > 0)
        {
            var intro = markdown[..topLevelHeaders[0].Span.Start].Trim();
            if (!string.IsNullOrWhiteSpace(intro))
                sections.Add(new ContentSection(null, intro));
        }

        for (var i = 0; i < topLevelHeaders.Count; i++)
        {
            var header = topLevelHeaders[i];

            var sectionStart = header.Span.Start;
            var sectionEnd = i < topLevelHeaders.Count - 1
                ? topLevelHeaders[i + 1].Span.Start - 1
                : markdown.Length - 1;

            sections.Add(new ContentSection(
                ExtractHeadingText(header),
                markdown.Substring(sectionStart, sectionEnd - sectionStart + 1)));
        }

        var frontmatter = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        var metadata = frontmatter?.Lines.Lines
            .Select(x => x.ToString())
            .Select(x =>
            {
                var colonIndex = x.IndexOf(':');
                if (colonIndex < 0)
                    return (string.Empty, x);

                return (x[..colonIndex], x[(colonIndex + 1)..].Trim());
            })
            .ToDictionary(x => x.Item1, x => x.Item2) ?? [];

        metadata.TryGetValue("title", out var title);

        return new Content(title, sections);
    }

    private static string ExtractHeadingText(HeadingBlock heading)
    {
        if (heading.Inline is null)
            return string.Empty;

        return string.Concat(
            heading.Inline
                .Descendants()
                .OfType<LiteralInline>()
                .Select(x => x.Content.Text.Substring(
                    x.Content.Start,
                    x.Content.Length)));
    }
}