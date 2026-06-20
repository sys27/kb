using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Messages;

namespace Backend.Chats;

public class ChatGptImporter
{
    private readonly KbDbContext dbContext;
    private readonly JsonSerializerOptions jsonOptions;

    public ChatGptImporter(KbDbContext dbContext)
    {
        this.dbContext = dbContext;
        this.jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Web)
        {
            AllowOutOfOrderMetadataProperties = true
        };
    }

    public async Task Import(Stream stream, CancellationToken cancellationToken = default)
    {
        var chats = await JsonSerializer.DeserializeAsync<GptChat[]>(
            stream,
            jsonOptions,
            cancellationToken);

        if (chats is null)
            return;

        foreach (var gptChat in chats)
        {
            var chat = new Chat
            {
                Name = gptChat.Title,
            };

            var mappings = SortMappings(gptChat.Mapping);
            foreach (var mapping in mappings)
            {
                var gptMessage = mapping.Message;
                if (gptMessage is null)
                    continue;

                var role = gptMessage.Author.Role;
                if (role is not "user" and not "assistant")
                    continue;

                var content = gptMessage.Content;
                if (content is GptMessageBrowsingContent)
                    continue;

                if (content is GptMessageTextContent text &&
                    (text.Parts.Length == 0 || string.IsNullOrWhiteSpace(text.Parts[0])))
                    continue;

                var createTime = gptMessage.CreateTime is not null
                    ? FromUnixTimestampDouble(gptMessage.CreateTime.Value)
                    : DateTime.UtcNow;
                var message = content.ToMessage(role, createTime);
                chat.AddMessage(message);
            }

            await dbContext.Chats.AddAsync(chat, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DateTime FromUnixTimestampDouble(double timestamp)
    {
        var seconds = (long)timestamp;
        var fractional = timestamp - seconds;

        return DateTimeOffset
            .FromUnixTimeSeconds(seconds)
            .AddSeconds(fractional)
            .UtcDateTime;
    }

    private static IReadOnlyList<GptMapping> SortMappings(Dictionary<string, GptMapping> mappings)
    {
        var result = new List<GptMapping>();
        var visited = new HashSet<string>();

        var roots = mappings.Values
            .Where(x => x.Parent == null);

        foreach (var root in roots)
        {
            Visit(root);
        }

        return result;

        void Visit(GptMapping mapping)
        {
            if (!visited.Add(mapping.Id))
            {
                return;
            }

            result.Add(mapping);

            foreach (var childId in mapping.Children)
            {
                if (mappings.TryGetValue(childId, out var child))
                {
                    Visit(child);
                }
            }
        }
    }

    private sealed class GptChat
    {
        public required string Title { get; set; }

        [JsonPropertyName("create_time")]
        public required double CreateTime { get; set; }

        [JsonPropertyName("update_time")]
        public required double UpdateTime { get; set; }

        public required Dictionary<string, GptMapping> Mapping { get; set; }
    }

    private sealed class GptMapping
    {
        public required string Id { get; set; }

        public required string[] Children { get; set; }

        public string? Parent { get; set; }

        public required GptMessage? Message { get; set; }
    }

    private sealed class GptMessage
    {
        public required GptMessageAuthor Author { get; set; }

        public required IGptMessageContent Content { get; set; }

        [JsonPropertyName("create_time")]
        public double? CreateTime { get; set; }
    }

    private sealed class GptMessageAuthor
    {
        public required string Role { get; set; }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "content_type")]
    [JsonDerivedType(typeof(GptMessageTextContent), "text")]
    [JsonDerivedType(typeof(GptMessageFileContent), "tether_quote")]
    [JsonDerivedType(typeof(GptMessageBrowsingContent), "tether_browsing_display")]
    [JsonDerivedType(typeof(GptMessageCodeContent), "code")]
    private interface IGptMessageContent
    {
        Message ToMessage(string role, DateTime createTime);
    }

    private sealed class GptMessageTextContent : IGptMessageContent
    {
        public required string[] Parts { get; set; }

        public Message ToMessage(string role, DateTime createTime)
            => new Message()
            {
                Text = Parts[0],
                Timestamp = createTime,
                MessageTypeId = role == "user" ? MessageType.UserRequestId : MessageType.AssistantAnswerId,
            };
    }

    private sealed class GptMessageFileContent : IGptMessageContent
    {
        public required string Text { get; set; }

        public Message ToMessage(string role, DateTime createTime)
            => new Message()
            {
                Text = Text,
                Timestamp = createTime,
                MessageTypeId = MessageType.UserRequestId,
            };
    }

    private sealed class GptMessageBrowsingContent : IGptMessageContent
    {
        public Message ToMessage(string role, DateTime createTime)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class GptMessageCodeContent : IGptMessageContent
    {
        public required string Text { get; set; }

        public Message ToMessage(string role, DateTime createTime)
            => new Message()
            {
                Text = Text,
                Timestamp = createTime,
                MessageTypeId = MessageType.AssistantAnswerId,
            };
    }
}