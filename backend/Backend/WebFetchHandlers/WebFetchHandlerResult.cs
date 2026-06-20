namespace Backend.WebFetchHandlers;

public record WebFetchHandlerResult(Stream ContentStream, string FileName, string ContentType);