using System.Net;
using System.Text;
using System.Text.Json;

namespace Skickamejl.Sdk.Tests;

public sealed class SkickamejlClientTests
{
    [Fact]
    public async Task SendMessageAsync_PostsMessageWithApiKeyAndJsonBody()
    {
        var handler = new RecordingHandler(
            HttpStatusCode.Accepted,
            """
            {
              "messageId": "msg_123",
              "deliveryId": "delivery_123",
              "jobId": "delivery_123",
              "serverId": "7f76d011-a5d2-4d7e-a7b4-c8c3037fc98a",
              "status": "queued",
              "messageStream": "transactional",
              "queuedAtUtc": "2026-07-04T11:21:50Z"
            }
            """);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var client = new SkickamejlClient(httpClient, " test-token ");

        var response = await client.SendMessageAsync(new SendMessageRequest(
            From: "sender@example.com",
            To: "recipient@example.com",
            Subject: "Hello")
        {
            TextBody = "Plain text body",
            MessageStream = "transactional",
            Tag = "welcome",
            TrackOpens = true,
            TrackLinks = "all",
            Metadata = new Dictionary<string, string>
            {
                ["customerId"] = "123"
            }
        });

        Assert.Equal("msg_123", response.MessageId);
        Assert.Equal("queued", response.Status);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal(new Uri("https://api.example.test/api/Messages"), handler.RequestUri);
        Assert.Equal("test-token", handler.ApiKey);
        Assert.Equal("application/json", handler.ContentType);

        using var requestJson = JsonDocument.Parse(handler.RequestBody!);
        var root = requestJson.RootElement;
        Assert.Equal("sender@example.com", root.GetProperty("from").GetString());
        Assert.Equal("recipient@example.com", root.GetProperty("to").GetString());
        Assert.Equal("Hello", root.GetProperty("subject").GetString());
        Assert.Equal("Plain text body", root.GetProperty("textBody").GetString());
        Assert.Equal("transactional", root.GetProperty("messageStream").GetString());
        Assert.Equal("welcome", root.GetProperty("tag").GetString());
        Assert.True(root.GetProperty("trackOpens").GetBoolean());
        Assert.Equal("all", root.GetProperty("trackLinks").GetString());
        Assert.Equal("123", root.GetProperty("metadata").GetProperty("customerId").GetString());
        Assert.False(root.TryGetProperty("replyTo", out _));
    }

    [Fact]
    public async Task SendMessageAsync_ThrowsApiExceptionWithProblemDetails()
    {
        var handler = new RecordingHandler(
            HttpStatusCode.Unauthorized,
            """
            {
              "title": "Autentisering misslyckades.",
              "status": 401,
              "detail": "API-token saknas.",
              "code": "missing_api_key"
            }
            """);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var client = new SkickamejlClient(httpClient, "test-token");

        var exception = await Assert.ThrowsAsync<SkickamejlApiException>(() =>
            client.SendMessageAsync(CreateValidRequest()));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("API-token saknas.", exception.Message);
        Assert.Equal("Autentisering misslyckades.", exception.Problem?.Title);
        Assert.Equal(401, exception.Problem?.Status);
        Assert.Equal("missing_api_key", exception.Problem?.Extensions?["code"].GetString());
    }

    [Fact]
    public async Task SendMessageAsync_RejectsMissingBodiesBeforeSending()
    {
        var handler = new RecordingHandler(HttpStatusCode.Accepted, "{}");
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var client = new SkickamejlClient(httpClient, "test-token");
        var request = new SendMessageRequest(
            From: "sender@example.com",
            To: "recipient@example.com",
            Subject: "Hello");

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            client.SendMessageAsync(request));

        Assert.Equal("request", exception.ParamName);
        Assert.Contains("Either TextBody or HtmlBody is required.", exception.Message);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task SendMessageAsync_RejectsUnsupportedTrackLinksBeforeSending()
    {
        var handler = new RecordingHandler(HttpStatusCode.Accepted, "{}");
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.example.test/")
        };
        var client = new SkickamejlClient(httpClient, "test-token");
        var request = CreateValidRequest() with
        {
            TrackLinks = "external"
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            client.SendMessageAsync(request));

        Assert.Equal("request", exception.ParamName);
        Assert.Contains("TrackLinks must be either 'all' or 'none'.", exception.Message);
        Assert.Equal(0, handler.CallCount);
    }

    private static SendMessageRequest CreateValidRequest() =>
        new(
            From: "sender@example.com",
            To: "recipient@example.com",
            Subject: "Hello")
        {
            TextBody = "Plain text body"
        };

    private sealed class RecordingHandler(HttpStatusCode statusCode, string responseBody) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        public HttpMethod? Method { get; private set; }

        public Uri? RequestUri { get; private set; }

        public string? ApiKey { get; private set; }

        public string? ContentType { get; private set; }

        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Method = request.Method;
            RequestUri = request.RequestUri;
            ApiKey = request.Headers.TryGetValues(SkickamejlClient.ApiKeyHeaderName, out var values)
                ? values.SingleOrDefault()
                : null;
            ContentType = request.Content?.Headers.ContentType?.MediaType;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            };
        }
    }
}
