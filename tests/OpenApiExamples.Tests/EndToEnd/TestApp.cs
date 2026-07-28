using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OpenApiExamples.ExtensionMethods;

namespace OpenApiExamples.Tests.EndToEnd;

/// <summary>
/// Hosts a real minimal-API app in memory and returns the OpenAPI document it generates. These tests cover the
/// whole path a consumer uses - route extension, metadata, operation transformer, writer, formatter - and
/// assert on the shipped artifact rather than on an intermediate object.
/// </summary>
internal static class TestApp
{
    public static async Task<JsonElement> GenerateDocumentAsync(
        Action<WebApplication> mapEndpoints,
        Action<OpenApiExamplesOptions>? configureExamples = null
    )
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddOpenApi(options => options.AddExamples());
        builder.Services.AddOpenApiExamples(configureExamples);

        await using var app = builder.Build();
        app.MapOpenApi();
        mapEndpoints(app);

        await app.StartAsync();
        var json = await app.GetTestClient().GetStringAsync("/openapi/v1.json");
        await app.StopAsync();

        // Cloned so the element outlives the JsonDocument this method owns.
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    public static JsonElement RequestContent(this JsonElement document, string path, string contentType) =>
        document.GetProperty("paths").GetProperty(path).GetProperty("post")
            .GetProperty("requestBody").GetProperty("content").GetProperty(contentType);

    public static JsonElement ResponseContent(
        this JsonElement document,
        string path,
        string statusCode,
        string contentType,
        string method = "get"
    ) =>
        document.GetProperty("paths").GetProperty(path).GetProperty(method)
            .GetProperty("responses").GetProperty(statusCode)
            .GetProperty("content").GetProperty(contentType);
}
