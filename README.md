# Skickamejl .NET SDK

Minimal .NET client for the Skickamejl message API.

This first version supports only:

```http
POST /api/Messages
```

## Usage

```csharp
using Skickamejl.Sdk;

using var client = new SkickamejlClient(
    new Uri("https://skickamejl.se/"),
    apiKey: "<server-api-token>");

var response = await client.SendMessageAsync(new SendMessageRequest(
    From: "sender@example.com",
    To: "recipient@example.com",
    Subject: "Hello from Skickamejl")
{
    TextBody = "This message was sent through the Skickamejl .NET SDK.",
    MessageStream = "transactional",
    Tag = "welcome",
    Metadata = new Dictionary<string, string>
    {
        ["customerId"] = "123"
    }
});

Console.WriteLine(response.MessageId);
```

The client sends the token in the `X-Api-Key` header, matching the Skickamejl server API.

## Supported Request Fields

- `From`
- `To`
- `Subject`
- `ReplyTo`
- `TextBody`
- `HtmlBody`
- `MessageStream`
- `Tag`
- `TrackOpens`
- `TrackLinks`
- `Metadata`

Either `TextBody` or `HtmlBody` is required. When set, `TrackLinks` must be `all` or `none`.

## Development

```powershell
dotnet build .\src\Skickamejl.Sdk.slnx
dotnet test .\src\Skickamejl.Sdk.slnx
```

## Publishing

The GitHub Actions workflow in `.github/workflows/publish.yml` publishes to NuGet.org on pushes to `master` and manual dispatches. NuGet trusted publishing must be configured for the `MRCCollective/skickamejl-sdk-dotnet` repository and the `publish.yml` workflow.
