using System.Text.Json.Serialization;

namespace Skickamejl.Sdk;

public sealed record SendMessageRequest(
    [property: JsonPropertyName("from")] string From,
    [property: JsonPropertyName("to")] string To,
    [property: JsonPropertyName("subject")] string Subject)
{
    [JsonPropertyName("replyTo")]
    public string? ReplyTo { get; init; }

    [JsonPropertyName("textBody")]
    public string? TextBody { get; init; }

    [JsonPropertyName("htmlBody")]
    public string? HtmlBody { get; init; }

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
