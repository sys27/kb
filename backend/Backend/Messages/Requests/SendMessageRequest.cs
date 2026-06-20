using System.ComponentModel;

namespace Backend.Messages.Requests;

public record SendMessageRequest(string Text, [property: DefaultValue(false)] bool EnableWebSearch);