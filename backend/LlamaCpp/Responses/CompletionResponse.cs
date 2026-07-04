namespace LlamaCpp.Responses;

internal sealed record CompletionResponse(ChoicesResponse[] Choices);