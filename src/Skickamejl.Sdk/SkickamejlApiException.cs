using System.Net;

namespace Skickamejl.Sdk;

public sealed class SkickamejlApiException : Exception
{
    public SkickamejlApiException(
        HttpStatusCode statusCode,
        string message,
        string? responseBody,
        SkickamejlProblemDetails? problem)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        Problem = problem;
    }

    public HttpStatusCode StatusCode { get; }

    public string? ResponseBody { get; }

    public SkickamejlProblemDetails? Problem { get; }
}
