using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenApiExamples.ExtensionMethods;
using OpenApiExamples.Models;
using OpenApiExamples.Tests.TestDoubles;

namespace OpenApiExamples.Tests.EndToEnd;

public class OpenApiDocumentTests
{
    [Fact]
    public async Task RequestExample_IsWrittenAsAJsonObject()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapPost("/widgets", (Widget widget) => TypedResults.Ok(widget))
                .RequestExample<SingleWidgetExample>("application/json")
        );

        var example = document.RequestContent("/widgets", "application/json").GetProperty("example");

        // The bug this library shipped in 1.0.1 showed up here as JsonValueKind.String.
        Assert.Equal(JsonValueKind.Object, example.ValueKind);
        Assert.Equal(1, example.GetProperty("Id").GetInt32());
        Assert.Equal("box", example.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task ResponseExample_IsWrittenAsAJsonObject()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets/{id}", (int id) => TypedResults.Ok(new Widget { Id = id, Name = "box" }))
                .ResponseExample<SingleWidgetExample>(200, "application/json")
        );

        var example = document.ResponseContent("/widgets/{id}", "200", "application/json")
            .GetProperty("example");

        Assert.Equal(JsonValueKind.Object, example.ValueKind);
        Assert.Equal("box", example.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task ResponseExamples_AreKeyedAndCarrySummaryAndDescription()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets", () => TypedResults.Ok(new[] { new Widget() }))
                .ResponseExamples<MultipleWidgetExamples>(200, "application/json")
        );

        var examples = document.ResponseContent("/widgets", "200", "application/json")
            .GetProperty("examples");

        var small = examples.GetProperty("small");
        Assert.Equal("A small widget", small.GetProperty("summary").GetString());
        Assert.Equal(JsonValueKind.Object, small.GetProperty("value").ValueKind);
        Assert.False(small.TryGetProperty("description", out _));

        var large = examples.GetProperty("large");
        Assert.Equal("A large widget", large.GetProperty("summary").GetString());
        Assert.Equal("Takes two people to lift", large.GetProperty("description").GetString());
        Assert.Equal("crate", large.GetProperty("value").GetProperty("Name").GetString());
    }

    [Fact]
    public async Task XmlResponseExample_IsWrittenAsAString()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets/{id}", (int id) => TypedResults.Ok(new Widget { Id = id, Name = "box" }))
                .Produces<Widget>(200, "application/xml")
                .ResponseExample<SingleWidgetExample>(200, "application/xml")
        );

        var example = document.ResponseContent("/widgets/{id}", "200", "application/xml")
            .GetProperty("example");

        Assert.Equal(JsonValueKind.String, example.ValueKind);
        Assert.Contains("<Widget", example.GetString()!);
    }

    [Fact]
    public async Task GroupLevelResponseExample_AppliesToEveryEndpointInTheGroup()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
        {
            var group = app.MapGroup("/api")
                .ResponseExample<SingleWidgetExample>(200, "application/json");

            group.MapGet("/widgets", () => TypedResults.Ok(new Widget { Id = 1, Name = "box" }));
            group.MapGet("/gadgets", () => TypedResults.Ok(new Widget { Id = 2, Name = "crate" }));
        });

        foreach (var path in new[] { "/api/widgets", "/api/gadgets" })
        {
            var example = document.ResponseContent(path, "200", "application/json").GetProperty("example");
            Assert.Equal(JsonValueKind.Object, example.ValueKind);
            Assert.Equal("box", example.GetProperty("Name").GetString());
        }
    }

    [Fact]
    public async Task ConfiguredSerializerOptions_ReachTheGeneratedDocument()
    {
        var document = await TestApp.GenerateDocumentAsync(
            app => app.MapPost("/widgets", (Widget widget) => TypedResults.Ok(widget))
                .RequestExample<SingleWidgetExample>("application/json"),
            options => options.JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
        );

        var example = document.RequestContent("/widgets", "application/json").GetProperty("example");

        // Worth pinning: the default OpenApiExamplesOptions.JsonSerializerOptions is PascalCase while
        // ASP.NET Core generates camelCase schemas, so this is the setting that makes examples match schemas.
        Assert.True(example.TryGetProperty("id", out _));
        Assert.False(example.TryGetProperty("Id", out _));
    }

    [Fact]
    public async Task ContentTypeWithNoRegisteredFormatter_IsSkippedWithoutFailingTheDocument()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets", () => TypedResults.Ok(new Widget()))
                .Produces<Widget>(200, "text/csv")
                .ResponseExample<SingleWidgetExample>(200, "text/csv")
        );

        var content = document.ResponseContent("/widgets", "200", "text/csv");

        Assert.False(content.TryGetProperty("example", out _));
        Assert.False(content.TryGetProperty("examples", out _));
    }

    [Fact]
    public async Task MetadataContentTypeThatTheEndpointDoesNotProduce_IsIgnored()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets", () => TypedResults.Ok(new Widget()))
                .ResponseExample<SingleWidgetExample>(200, "application/xml")
        );

        var content = document.ResponseContent("/widgets", "200", "application/json");

        Assert.False(content.TryGetProperty("example", out _));
    }

    [Fact]
    public async Task ProviderImplementingNeitherInterface_IsLoggedAndSkipped()
    {
        // The typed extension methods constrain T, so reaching this branch means hand-attached metadata -
        // which is exactly the shape a consumer produces when they write their own helper.
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapPost("/widgets", (Widget widget) => TypedResults.Ok(widget))
                .WithMetadata(new RequestExampleMetadata("application/json", typeof(NotAnExampleProvider)))
        );

        var content = document.RequestContent("/widgets", "application/json");

        Assert.False(content.TryGetProperty("example", out _));
    }

    [Fact]
    public async Task EndpointWithoutExampleMetadata_IsLeftAlone()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapPost("/widgets", (Widget widget) => TypedResults.Ok(widget))
        );

        var content = document.RequestContent("/widgets", "application/json");

        Assert.False(content.TryGetProperty("example", out _));
        Assert.True(content.TryGetProperty("schema", out _));
    }

    [Fact]
    public async Task RequestExamples_AreKeyedOnTheRequestBody()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapPost("/widgets", (Widget widget) => TypedResults.Ok(widget))
                .RequestExamples<MultipleWidgetExamples>("application/json")
        );

        var examples = document.RequestContent("/widgets", "application/json")
            .GetProperty("examples");

        Assert.Equal("A small widget", examples.GetProperty("small").GetProperty("summary").GetString());
        Assert.Equal(
            "crate",
            examples.GetProperty("large").GetProperty("value").GetProperty("Name").GetString()
        );
    }

    [Fact]
    public async Task GroupLevelResponseExamples_ApplyToEveryEndpointInTheGroup()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
        {
            var group = app.MapGroup("/api")
                .ResponseExamples<MultipleWidgetExamples>(200, "application/json");

            group.MapGet("/widgets", () => TypedResults.Ok(new Widget { Id = 1, Name = "box" }));
            group.MapGet("/gadgets", () => TypedResults.Ok(new Widget { Id = 2, Name = "crate" }));
        });

        foreach (var path in new[] { "/api/widgets", "/api/gadgets" })
        {
            var examples = document.ResponseContent(path, "200", "application/json")
                .GetProperty("examples");

            Assert.Equal("A small widget", examples.GetProperty("small").GetProperty("summary").GetString());
            Assert.True(examples.TryGetProperty("large", out _));
        }
    }

    [Fact]
    public async Task ResponseExample_WithAStringStatusCode_IsWritten()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets", () => TypedResults.Ok(new Widget { Id = 1, Name = "box" }))
                .ResponseExample<SingleWidgetExample>("200", "application/json")
        );

        var example = document.ResponseContent("/widgets", "200", "application/json")
            .GetProperty("example");

        Assert.Equal("box", example.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task ResponseExamples_WithAStringStatusCode_AreWritten()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets", () => TypedResults.Ok(new Widget()))
                .ResponseExamples<MultipleWidgetExamples>("200", "application/json")
        );

        var examples = document.ResponseContent("/widgets", "200", "application/json")
            .GetProperty("examples");

        Assert.True(examples.TryGetProperty("small", out _));
        Assert.True(examples.TryGetProperty("large", out _));
    }

    [Fact]
    public async Task GroupLevelResponseExamples_WithAStringStatusCode_AreWritten()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
        {
            var group = app.MapGroup("/api")
                .ResponseExamples<MultipleWidgetExamples>("200", "application/json");

            group.MapGet("/widgets", () => TypedResults.Ok(new Widget()));
        });

        var examples = document.ResponseContent("/api/widgets", "200", "application/json")
            .GetProperty("examples");

        Assert.True(examples.TryGetProperty("small", out _));
    }

    [Fact]
    public async Task ProviderWithAConstructorDependency_IsBuiltFromTheContainer()
    {
        // The writer uses ActivatorUtilities, which is the only reason a provider may take services at all.
        var document = await TestApp.GenerateDocumentAsync(
            app => app.MapGet("/widgets", () => TypedResults.Ok(new Widget()))
                .ResponseExample<InjectedWidgetExample>(200, "application/json"),
            configureServices: services => services.AddSingleton<WidgetNamer>()
        );

        var example = document.ResponseContent("/widgets", "200", "application/json")
            .GetProperty("example");

        Assert.Equal("injected", example.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task ProviderReturningNoExamples_WritesNoExamplesKey()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets", () => TypedResults.Ok(new Widget()))
                .ResponseExamples<NoWidgetExamples>(200, "application/json")
        );

        var content = document.ResponseContent("/widgets", "200", "application/json");

        // The writer only creates the dictionary inside the loop, so an empty provider leaves the media type bare.
        Assert.False(content.TryGetProperty("examples", out _));
        Assert.False(content.TryGetProperty("example", out _));
    }

    [Fact]
    public async Task XmlResponseExamples_AreKeyedStrings()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets", () => TypedResults.Ok(new Widget()))
                .Produces<Widget>(200, "application/xml")
                .ResponseExamples<MultipleWidgetExamples>(200, "application/xml")
        );

        var examples = document.ResponseContent("/widgets", "200", "application/xml")
            .GetProperty("examples");

        var small = examples.GetProperty("small").GetProperty("value");
        Assert.Equal(JsonValueKind.String, small.ValueKind);
        Assert.Contains("<Widget", small.GetString()!);
        Assert.Contains("<Name>crate</Name>", examples.GetProperty("large").GetProperty("value").GetString()!);
    }

    [Fact]
    public async Task StatusCodeThatTheEndpointDoesNotDocument_IsIgnored()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets", () => TypedResults.Ok(new Widget()))
                .ResponseExample<SingleWidgetExample>(404, "application/json")
        );

        var responses = document.GetProperty("paths").GetProperty("/widgets").GetProperty("get")
            .GetProperty("responses");

        Assert.False(responses.TryGetProperty("404", out _));
        Assert.False(
            document.ResponseContent("/widgets", "200", "application/json").TryGetProperty("example", out _)
        );
    }

    [Fact]
    public async Task RequestAndResponseExamplesOnOneEndpoint_AreBothWritten()
    {
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapPost("/widgets", (Widget widget) => TypedResults.Ok(widget))
                .RequestExample<SingleWidgetExample>("application/json")
                .ResponseExamples<MultipleWidgetExamples>(200, "application/json")
        );

        Assert.Equal(
            "box",
            document.RequestContent("/widgets", "application/json")
                .GetProperty("example").GetProperty("Name").GetString()
        );
        Assert.True(
            document.ResponseContent("/widgets", "200", "application/json", method: "post")
                .GetProperty("examples").TryGetProperty("small", out _)
        );
    }

    [Fact]
    public async Task TwoContentTypesOnOneResponse_AreBothWritten()
    {
        // Each attached metadata item is a separate pass through the transformer loop.
        var document = await TestApp.GenerateDocumentAsync(app =>
            app.MapGet("/widgets", () => TypedResults.Ok(new Widget { Id = 1, Name = "box" }))
                .Produces<Widget>(200, "application/json", "application/xml")
                .ResponseExample<SingleWidgetExample>(200, "application/json")
                .ResponseExample<SingleWidgetExample>(200, "application/xml")
        );

        var json = document.ResponseContent("/widgets", "200", "application/json").GetProperty("example");
        var xml = document.ResponseContent("/widgets", "200", "application/xml").GetProperty("example");

        Assert.Equal(JsonValueKind.Object, json.ValueKind);
        Assert.Equal(JsonValueKind.String, xml.ValueKind);
    }
}
