using System.Xml.Serialization;

namespace Roastery.Api.Shared;

/// <summary>
/// Minimal APIs read and write JSON and nothing else. These two carry the XML side.
/// </summary>
internal static class ContentNegotiation
{
    /// <summary>200, in whichever of the two the caller asked for.</summary>
    public static IResult Ok<T>(T value) => new NegotiatedResult<T>(value, StatusCodes.Status200OK);

    /// <summary>201 with a Location header, in whichever of the two the caller asked for.</summary>
    public static IResult Created<T>(string location, T value) =>
        new NegotiatedResult<T>(value, StatusCodes.Status201Created) { Location = location };

    /// <summary>For the endpoints that only speak XML, whatever the Accept header says.</summary>
    public static IResult Xml<T>(T value, int statusCode = StatusCodes.Status200OK) =>
        new NegotiatedResult<T>(value, statusCode) { XmlOnly = true };

    public static IResult XmlCreated<T>(string location, T value) =>
        new NegotiatedResult<T>(value, StatusCodes.Status201Created) { XmlOnly = true, Location = location };
}

internal class NegotiatedResult<T> : IResult
{
    private readonly T value;
    private readonly int statusCode;

    public NegotiatedResult(T value, int statusCode)
    {
        this.value = value;
        this.statusCode = statusCode;
    }

    public bool XmlOnly { get; init; }

    public string? Location { get; init; }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = this.statusCode;

        if (this.Location is not null)
        {
            httpContext.Response.Headers.Location = this.Location;
        }

        if (!this.XmlOnly && !WantsXml(httpContext.Request))
        {
            await httpContext.Response.WriteAsJsonAsync(this.value);
            return;
        }

        httpContext.Response.ContentType = "application/xml; charset=utf-8";

        // Serialize into a buffer first. XmlSerializer only writes synchronously and the response body
        // does not allow that.
        using var buffer = new MemoryStream();
        new XmlSerializer(typeof(T)).Serialize(buffer, this.value);
        buffer.Position = 0;
        await buffer.CopyToAsync(httpContext.Response.Body);
    }

    private static bool WantsXml(HttpRequest request) =>
        request.Headers.Accept.Any(accept => accept?.Contains("xml", StringComparison.OrdinalIgnoreCase) == true);
}

/// <summary>
/// The other half. A contract exposing <c>BindAsync</c> gets bound by this instead of the JSON binder,
/// which is what lets the same endpoint take an XML body.
/// </summary>
internal static class RequestBody
{
    public static async ValueTask<T?> ReadAsync<T>(HttpContext httpContext)
    {
        var contentType = httpContext.Request.ContentType ?? string.Empty;

        if (contentType.Contains("xml", StringComparison.OrdinalIgnoreCase))
        {
            // Buffered first for the same reason as the response: XmlSerializer only reads synchronously
            // and Kestrel does not allow that on the request body.
            using var buffer = new MemoryStream();
            await httpContext.Request.Body.CopyToAsync(buffer);
            buffer.Position = 0;

            return (T?)new XmlSerializer(typeof(T)).Deserialize(buffer);
        }

        return await httpContext.Request.ReadFromJsonAsync<T>();
    }
}
