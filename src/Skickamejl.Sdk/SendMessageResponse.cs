using System.Text.Json.Serialization;

namespace Skickamejl.Sdk;

public sealed record SendMessageResponse(
    [property: JsonPropertyName("messageId")] string? MessageId,
    [property: JsonPropertyName("deliveryId")] string? DeliveryId,
    [property: JsonPropertyName("jobId")] string? JobId,
    [property: JsonPropertyName("serverId")] Guid? ServerId,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("messageStream")] string? MessageStream,
    [property: JsonPropertyName("queuedAtUtc")] DateTimeOffset QueuedAtUtc);
