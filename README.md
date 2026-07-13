# Skickamejl .NET SDK

Minimal .NET client for the Skickamejl message and template APIs.

This version supports:

```http
POST /api/Messages
POST /api/Templates/{alias}/send
```

## Send a message

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

## Send a template message

```csharp
using Skickamejl.Sdk;

using var client = new SkickamejlClient(
    new Uri("https://skickamejl.se/"),
    apiKey: "<server-api-token>");

var response = await client.SendTemplateMessageAsync(
    "orderbekraftelse",
    new SendTemplateMessageRequest(
        From: "sender@example.com",
        To: "recipient@example.com")
    {
        Variables = new Dictionary<string, string>
        {
            ["customerName"] = "Anna",
            ["orderNumber"] = "12345"
        },
        MessageStream = "transactional",
        Tag = "order"
    });

Console.WriteLine(response.MessageId);
```

## Supported message request fields

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

## Supported template request fields

- `From`
- `To`
- `SenderName`
- `ReplyTo`
- `Variables`
- `MessageStream`
- `Tag`
- `TrackOpens`
- `TrackLinks`
- `Metadata`

`TrackLinks`, when set, must be `all` or `none`.

## Development

```powershell
dotnet build .\src\Skickamejl.Sdk.slnx
dotnet test .\src\Skickamejl.Sdk.slnx
```
