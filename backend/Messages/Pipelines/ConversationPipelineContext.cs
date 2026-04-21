namespace Backend.Messages.Pipelines;

public class ConversationPipelineContext
{
    private readonly Dictionary<string, object> properties = [];

    public T Get<T>(string key)
        => (T)properties[key];

    public void Set(string key, object value)
        => properties[key] = value;
}