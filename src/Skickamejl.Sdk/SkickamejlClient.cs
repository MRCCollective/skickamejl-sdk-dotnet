using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Skickamejl.Sdk;

/// <summary>
/// Minimal client for the Skickamejl message API.
/// </summary>
public sealed class SkickamejlClient : IDisposable
{
    public const string ApiKeyHeaderName = "X-Api-Key";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient httpClient;
    private readonly string apiKey;
    private readonly bool ownsHttpClient;

    public SkickamejlClient(HttpClient httpClient, string apiKey)
        : this(httpClient, apiKey, ownsHttpClient: false)
    {
    }

    public SkickamejlClient(Uri baseAddress, string apiKey)
        : this(new HttpClient { BaseAddress = baseAddress }, apiKey, ownsHttpClient: true)
    {
    }

    private SkickamejlClient(HttpClient httpClient, string apiKey, bool ownsHttpClient)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.apiKey = string.IsNullOrWhiteSpace(apiKey)
            ? throw new ArgumentException("API key is required.", nameof(apiKey))
            : apiKey.Trim();
        this.ownsHttpClient = ownsHttpClient;
    }

    public async Task<SendMessageResponse> SendMessageAsync(
        SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/Messages");
        httpRequest.Headers.TryAddWithoutValidation(ApiKeyHeaderName, apiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient
            .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        var responseBody = response.Content is null
            ? string.Empty
            : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Accepted)
        {
            return JsonSerializer.Deserialize<SendMessageResponse>(responseBody, JsonOptions)
                ?? throw new SkickamejlApiException(
                    response.StatusCode,
                    "Skickamejl returned an empty message response.",
                    responseBody,
                    problem: null);
        }

        var problem = TryDeserializeProblem(responseBody);
        var message = problem?.Detail
            ?? problem?.Title
            ?? $"Skickamejl API returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).";

        throw new SkickamejlApiException(response.StatusCode, message, responseBody, problem);
    }

    public void Dispose()
    {
        if (ownsHttpClient)
        {
            httpClient.Dispose();
        }
    }

    private static SkickamejlProblemDetails? TryDeserializeProblem(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SkickamejlProblemDetails>(responseBody, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void ValidateRequest(SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.From))
        {
            throw new ArgumentException("From is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.To))
        {
            throw new ArgumentException("To is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            throw new ArgumentException("Subject is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.TextBody) && string.IsNullOrWhiteSpace(request.HtmlBody))
        {
            throw new ArgumentException("Either TextBody or HtmlBody is required.", nameof(request));
        }

        if (!string.IsNullOrWhiteSpace(request.TrackLinks) &&
            !string.Equals(request.TrackLinks.Trim(), "all", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(request.TrackLinks.Trim(), "none", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("TrackLinks must be either 'all' or 'none'.", nameof(request));
        }
    }
}
