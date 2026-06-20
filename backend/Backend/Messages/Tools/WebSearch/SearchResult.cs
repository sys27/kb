namespace Backend.Messages.Tools.WebSearch;

public record SearchResult(string Url, string Title, IEnumerable<string> Chunks);