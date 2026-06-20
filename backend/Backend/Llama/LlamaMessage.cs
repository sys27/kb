namespace Backend.Llama;

public sealed record LlamaMessage(string Role, string Content)
{
    public const string SystemRole = "system";
    public const string UserRole = "user";
    public const string AssistantRole = "assistant";

    public static LlamaMessage ForSystem(string content)
        => new LlamaMessage(SystemRole, content);

    public static LlamaMessage ForUser(string content)
        => new LlamaMessage(UserRole, content);

    public static LlamaMessage ForAssistant(string content)
        => new LlamaMessage(AssistantRole, content);
}