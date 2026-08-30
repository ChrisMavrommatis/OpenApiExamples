using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenApiExamples;
using OpenApiExamples.Abstractions;
using OpenApiExamples.ExtensionMethods;

namespace OpenApiExamples.Tests.DocsSnippets;

public class Widget
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateWidgetExample : ISingleOpenApiExamplesProvider<Widget>
{
    public IOpenApiExample<Widget> GetExample() =>
        OpenApiExample.Create("default", new Widget { Id = 1, Name = "Blue Widget" });
}

public class WidgetExamples : IMultipleOpenApiExamplesProvider<Widget>
{
    public IEnumerable<IOpenApiExample<Widget>> GetExamples() =>
    [
        OpenApiExample.Create("small", "A small widget", new Widget { Id = 1, Name = "Bolt" }),
        OpenApiExample.Create(
            key: "large",
            summary: "A large widget",
            description: "Takes two people to lift.",
            value: new Widget { Id = 2, Name = "Girder" }
        ),
    ];
}

public interface IWidgetCatalog
{
    Widget Featured { get; }
}

public class WidgetExample : ISingleOpenApiExamplesProvider<Widget>
{
    private readonly IWidgetCatalog catalog;

    public WidgetExample(IWidgetCatalog catalog) => this.catalog = catalog;

    public IOpenApiExample<Widget> GetExample() =>
        OpenApiExample.Create("default", this.catalog.Featured);
}

public class ProblemDetailsExample : ISingleOpenApiExamplesProvider<Widget>
{
    public IOpenApiExample<Widget> GetExample() => OpenApiExample.Create("default", new Widget());
}

public class YamlExamplesFormatter : IOpenApiExamplesFormatter
{
    public IEnumerable<string> SupportedContentTypes => ["application/yaml"];

    public ValueTask<JsonNode> FormatAsync(object example)
    {
        var yaml = MyYamlSerializer.Serialize(example);
        return ValueTask.FromResult<JsonNode>(JsonValue.Create(yaml)!);
    }
}

internal static class MyYamlSerializer
{
    public static string Serialize(object value) => value.ToString() ?? string.Empty;
}

// Every snippet in README.md and docs/, compiled. This file is a syntax check, not a behaviour test - if the
// public API shifts under the documentation, this stops building.
public static class Snippets
{
    public static void QuickStart(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApiExamples();

        builder.Services.AddOpenApi(options => options.AddExamples());

        var app = builder.Build();
        app.MapOpenApi();

        app.MapPost("/widgets", (Widget widget) => TypedResults.Ok(widget))
            .RequestExample<CreateWidgetExample>("application/json")
            .ResponseExample<CreateWidgetExample>(200, "application/json");

        app.MapGet("/widgets", () => TypedResults.Ok(Array.Empty<Widget>()))
            .ResponseExamples<WidgetExamples>(200, "application/json");

        app.MapGet("/widgets/{id}", (int id) => TypedResults.Ok(new Widget()))
            .Produces<Widget>(200, "application/xml")
            .ResponseExample<WidgetExample>(200, "application/xml");

        var api = app.MapGroup("/api")
            .ResponseExample<ProblemDetailsExample>(400, "application/problem+json");

        api.MapGet("/widgets", () => TypedResults.Ok(Array.Empty<Widget>()));
        api.MapGet("/gadgets", () => TypedResults.Ok(Array.Empty<Widget>()));
    }

    public static void DeclaringContentTypes(WebApplication app)
    {
        app.MapGet("/widgets/{id}", (int id) => TypedResults.Ok(new Widget()))
            .Produces<Widget>(200, "application/json", "application/xml")
            .ResponseExample<WidgetExample>(200, "application/json")
            .ResponseExample<WidgetExample>(200, "application/xml");
    }

    public static void XmlRootNames(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi(options =>
        {
            options.AddExamples();

            options.AddSchemaTransformer((schema, context, _) =>
            {
                var element = context.JsonTypeInfo.Type.GetElementType();

                if (schema.Type == JsonSchemaType.Array && element is not null)
                {
                    schema.Xml ??= new OpenApiXml { Name = $"ArrayOf{element.Name}", Wrapped = true };
                }

                return Task.CompletedTask;
            });
        });
    }

    public static void Configuration(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApiExamples(options =>
        {
            options.JsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
            options.XmlSerializerOptions.Encoding = Encoding.UTF8;
        });

        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services
            .AddOpenApiExamples()
            .AddExamplesFormatter<YamlExamplesFormatter>();
    }
}
