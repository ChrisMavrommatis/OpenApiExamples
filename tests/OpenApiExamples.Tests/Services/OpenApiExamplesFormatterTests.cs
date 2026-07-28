using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OpenApiExamples.Abstractions;
using OpenApiExamples.ExtensionMethods;
using OpenApiExamples.Tests.TestDoubles;

namespace OpenApiExamples.Tests.Services;

public class OpenApiExamplesFormatterTests
{
    // Both formatters are internal, so the tests resolve them the way a consumer does - through the public
    // registration, keyed by the content type each one declares. That exercises AddOpenApiExamples() too.
    private static IOpenApiExamplesFormatter GetFormatter(
        string contentType,
        Action<OpenApiExamplesOptions>? configure = null
    )
    {
        var provider = new ServiceCollection()
            .AddLogging()
            .AddOpenApiExamples(configure)
            .BuildServiceProvider();

        return provider.GetServices<IOpenApiExamplesFormatter>()
            .Single(f => f.SupportedContentTypes.Contains(contentType));
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("application/problem+json")]
    public async Task JsonFormatter_ProducesAnObject_NotAnEncodedString(string contentType)
    {
        var formatter = GetFormatter(contentType);

        var node = await formatter.FormatAsync(new Widget { Id = 1, Name = "box" });

        // A JsonValue here is the regression: the example lands in the document double encoded, as
        // "example": "{\"Id\":1}" instead of "example": { "Id": 1 }.
        var obj = Assert.IsAssignableFrom<JsonObject>(node);
        Assert.Equal(1, (int)obj["Id"]!);
        Assert.Equal("box", (string?)obj["Name"]);
    }

    [Fact]
    public async Task JsonFormatter_ProducesAnArray_ForCollections()
    {
        var formatter = GetFormatter("application/json");

        var node = await formatter.FormatAsync(new[]
        {
            new Widget { Id = 1, Name = "box" },
            new Widget { Id = 2, Name = "crate" },
        });

        var array = Assert.IsAssignableFrom<JsonArray>(node);
        Assert.Equal(2, array.Count);
    }

    [Fact]
    public async Task JsonFormatter_HonoursConfiguredSerializerOptions()
    {
        var formatter = GetFormatter(
            "application/json",
            options => options.JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
        );

        var node = await formatter.FormatAsync(new Widget { Id = 1, Name = "box" });

        var obj = Assert.IsAssignableFrom<JsonObject>(node);
        Assert.True(obj.ContainsKey("id"), "Expected the configured camelCase policy to be applied.");
        Assert.False(obj.ContainsKey("Id"));
    }

    [Theory]
    [InlineData("application/xml")]
    [InlineData("text/xml")]
    public async Task XmlFormatter_ProducesAString(string contentType)
    {
        var formatter = GetFormatter(contentType);

        var node = await formatter.FormatAsync(new Widget { Id = 1, Name = "box" });

        // Guards the trap next to the JSON fix: the XML formatter's JsonValue.Create looks like the same bug
        // and is correct - XML inside a JSON example genuinely is a string. Do not "fix" it for consistency.
        var value = Assert.IsAssignableFrom<JsonValue>(node);
        var xml = value.GetValue<string>();
        Assert.Contains("<Widget", xml);
        Assert.Contains("<Name>box</Name>", xml);
    }

    [Fact]
    public async Task XmlFormatter_HonoursConfiguredEncoding()
    {
        var formatter = GetFormatter(
            "application/xml",
            options => options.XmlSerializerOptions.Encoding = Encoding.Unicode
        );

        var node = await formatter.FormatAsync(new Widget { Id = 1, Name = "box" });

        // The encoding only surfaces in the XML declaration - StringWriterWithEncoding exists precisely so
        // XmlSerializer can read it back off the writer.
        var xml = Assert.IsAssignableFrom<JsonValue>(node).GetValue<string>();
        Assert.Contains("encoding=\"utf-16\"", xml);
    }
}
