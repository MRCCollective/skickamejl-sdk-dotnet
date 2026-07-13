using System.Text.Json.Serialization;

namespace Skickamejl.Sdk;

public sealed record SendTemplateMessageRequest(
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To)
{
    [JsonPropertyName("senderName")]
    public string? SenderName { get; init; }

    [JsonPropertyName("replyTo")]
    public string? ReplyTo { get; init; }

    [JsonPropertyName("variables")]
    public IReadOnlyDictionary<string, string>? Variables { get; init; }

    [JsonPropertyName("messageStream")]
    public string? MessageStream { get; init; }

    [JsonPropertyName("tag")]
    public string? Tag { get; init; }

    [JsonPropertyName("trackOpens")]
    public bool? TrackOpens { get; init; }

    [JsonPropertyName("trackLinks")]
    public string? TrackLinks { get; init; }

    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
