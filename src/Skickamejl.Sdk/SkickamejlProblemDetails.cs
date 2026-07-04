using System.Text.Json;
using System.Text.Json.Serialization;

namespace Skickamejl.Sdk;

public sealed class SkickamejlProblemDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("status")]
    public int? Status { get; init; }

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("instance")]
    public string? Instance { get; init; }

    [JsonPropertyName("errors")]
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    [JsonExtensionData]
    public IDictionary<string, JsonElement>? Extensions { get; init; }
}
