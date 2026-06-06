namespace Backend.ContentExtractors;

public record Content(string? Title, IReadOnlyList<ContentSection> Sections);