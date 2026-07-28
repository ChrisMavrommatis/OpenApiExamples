using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenApiExamples.Abstractions;
using OpenApiExamples.ExtensionMethods;

namespace OpenApiExamples.Tests.ExtensionMethods;

public class ServiceCollectionExtensionsTests
{
    private class CsvFormatter : IOpenApiExamplesFormatter
    {
        public IEnumerable<string> SupportedContentTypes => ["text/csv"];

        public ValueTask<JsonNode> FormatAsync(object example) =>
            ValueTask.FromResult<JsonNode>(JsonValue.Create("csv")!);
    }

    // Deliberately claims a content type the built-in JSON formatter already owns.
    private class OverridingJsonFormatter : IOpenApiExamplesFormatter
    {
        public IEnumerable<string> SupportedContentTypes => ["application/json"];

        public ValueTask<JsonNode> FormatAsync(object example) =>
            ValueTask.FromResult<JsonNode>(JsonValue.Create("overridden")!);
    }

    private static OpenApiExamplesOptions Resolve(IServiceCollection services) =>
        services.BuildServiceProvider().GetRequiredService<IOptions<OpenApiExamplesOptions>>().Value;

    [Fact]
    public void AddOpenApiExamples_MapsEveryBuiltInContentType()
    {
        var options = Resolve(new ServiceCollection().AddLogging().AddOpenApiExamples());

        Assert.Equal(
            ["application/json", "application/problem+json", "application/xml", "text/xml"],
            options.Formatters.Keys.Order()
        );
    }

    [Fact]
    public void AddOpenApiExamples_WithoutLogging_StillResolves()
    {
        // The registration reaches for ILoggerFactory with GetService, so an app that never called AddLogging
        // must not blow up on the first options resolution.
        var options = Resolve(new ServiceCollection().AddOpenApiExamples());

        Assert.NotEmpty(options.Formatters);
    }

    [Fact]
    public async Task AddExamplesFormatter_MakesACustomContentTypeAvailable()
    {
        var options = Resolve(
            new ServiceCollection().AddLogging().AddOpenApiExamples().AddExamplesFormatter<CsvFormatter>()
        );

        var formatter = Assert.Contains("text/csv", options.Formatters);
        Assert.IsType<CsvFormatter>(formatter);
        Assert.Equal("csv", (await formatter.FormatAsync(new object())).GetValue<string>());
    }

    [Fact]
    public async Task AddExamplesFormatter_OverridesABuiltInContentType()
    {
        // Last registration wins - the options callback walks the formatters in registration order and the
        // built-ins go in first. The warning it logs on the way past is the documented signal.
        var options = Resolve(
            new ServiceCollection().AddLogging().AddOpenApiExamples()
                .AddExamplesFormatter<OverridingJsonFormatter>()
        );

        var formatter = options.Formatters["application/json"];
        Assert.IsType<OverridingJsonFormatter>(formatter);
        Assert.Equal("overridden", (await formatter.FormatAsync(new object())).GetValue<string>());
    }

    [Fact]
    public void AddExamplesFormatter_Twice_Throws()
    {
        var services = new ServiceCollection().AddLogging().AddOpenApiExamples()
            .AddExamplesFormatter<CsvFormatter>();

        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddExamplesFormatter<CsvFormatter>()
        );
        Assert.Contains(nameof(CsvFormatter), exception.Message);
    }
}
