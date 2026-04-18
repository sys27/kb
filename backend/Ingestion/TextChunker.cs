using Microsoft.ML.Tokenizers;

namespace Backend.Ingestion;

// TODO: semantic chunking?
public class TextChunker : IChunker
{
    private readonly Tokenizer tokenizer;

    public TextChunker(Tokenizer tokenizer)
        => this.tokenizer = tokenizer;

    public IReadOnlyList<Chunk> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentNullException(nameof(text));

        var span = text.AsSpan();
        var tokens = tokenizer.EncodeToTokens(span, out _);

        if (tokens.Count <= Chunk.MaxTokens)
            return [new Chunk(0, text.Length)];

        var chunks = new List<Chunk>(tokens.Count / Chunk.MaxTokens);
        var startIndex = 0;

        while (true)
        {
            var overlapIndex = Math.Min(startIndex + Chunk.Overlap, tokens.Count - 1);
            var endIndex = Math.Min(startIndex + Chunk.MaxTokens, tokens.Count - 1);

            var startToken = tokens[startIndex];
            var overlapToken = tokens[overlapIndex];
            var endToken = tokens[endIndex];
            var remaining = span[overlapToken.Offset.Start..endToken.Offset.End];

            // use remaining text if it's shorter than max tokens
            if (tokens.Count - (startIndex + 1) <= Chunk.MaxTokens)
            {
                chunks.Add(new Chunk(startToken.Offset.Start.Value, remaining.Length));

                break;
            }

            var length = FindSplit(remaining);
            if (length is not null)
            {
                // we're searching for split position starting from an overlap token,
                // so we need to add the overlap length to the start position
                length += overlapToken.Offset.Start.Value - startToken.Offset.Start.Value;
            }
            else
            {
                length = remaining.Length;
            }

            chunks.Add(new Chunk(startToken.Offset.Start.Value, length!.Value));

            var endSymbol = startToken.Offset.Start.Value + length;
            for (; endIndex >= startIndex; endIndex--)
            {
                endToken = tokens[endIndex];
                if (endToken.Offset.Start.Value <= endSymbol && endSymbol <= endToken.Offset.End.Value)
                    break;
            }

            startIndex = endIndex + 1;

            if (startIndex >= tokens.Count)
                break;

            startIndex -= Chunk.Overlap;
        }

        return chunks;
    }

    private int? FindSplit(ReadOnlySpan<char> span)
        => TryParagraph(span) ??
           TrySentence(span) ??
           TryWord(span) ??
           null;

    private int? TryParagraph(ReadOnlySpan<char> span)
    {
        // TODO: handle other paragraph symbols (\r\n\r\n)
        const string paragraph = "\n\n";

        var found = span.LastIndexOf(paragraph);
        if (found == -1)
            return null;

        return found + paragraph.Length;
    }

    private int? TrySentence(ReadOnlySpan<char> span)
    {
        // TODO: handle decimal separator
        // TODO: handle abbreviations
        for (var i = span.Length - 1; i >= 0; i--)
            if (span[i] is '.' or '!' or '?')
                return i + 1;

        return null;
    }

    private int? TryWord(ReadOnlySpan<char> span)
    {
        for (var i = span.Length - 1; i >= 0; i--)
            if (span[i] == ' ')
                return i + 1;

        return null;
    }
}