# OpenApiExamples

**Enrich your OpenAPI documentation with real, customizable examples for requests and responses in .NET.**

[![NuGet](https://img.shields.io/nuget/v/OpenApiExamples.svg)](https://www.nuget.org/packages/OpenApiExamples)
[![Downloads](https://img.shields.io/nuget/dt/OpenApiExamples.svg)](https://www.nuget.org/packages/OpenApiExamples)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## Features

- ✨ Seamlessly injects example payloads into your OpenAPI JSON
- ⚡ Supports multiple content types (JSON, XML)
- 🧩 Easy integration with ASP.NET Core
- 🔌 Extensible formatter system
- 🎯 One example or many, named and described, per endpoint
- 💉 Example providers are resolved from DI, so they can take dependencies

## Installation

Install via NuGet Package Manager:

```text
Install-Package OpenApiExamples
```

Or via .NET CLI:

```text
dotnet add package OpenApiExamples
```

## Quick start

**1. Register the services and the transformers.**

```csharp
using OpenApiExamples.ExtensionMethods;

var builder = WebApplication.CreateBuilder(args);

// JsonSerializerDefaults.Web makes examples camelCase, matching the schemas ASP.NET Core generates.
// See Configuration below for why this matters.
builder.Services.AddOpenApiExamples(options =>
    options.JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web));

builder.Services.AddOpenApi(options => options.AddExamples());

var app = builder.Build();
app.MapOpenApi();
```

**2. Write a provider for the example you want to show.**

```csharp
using OpenApiExamples;
using OpenApiExamples.Abstractions;

public class CreateWidgetExample : ISingleOpenApiExamplesProvider<Widget>
{
    public IOpenApiExample<Widget> GetExample() =>
        OpenApiExample.Create("default", new Widget { Id = 1, Name = "Blue Widget" });
}
```

**3. Attach it to an endpoint.**

```csharp
app.MapPost("/widgets", (Widget widget) => TypedResults.Ok(widget))
    .RequestExample<CreateWidgetExample>("application/json")
    .ResponseExample<CreateWidgetExample>(200, "application/json");
```

Your document now carries the example inline:

```json
"requestBody": {
  "content": {
    "application/json": {
      "schema": { "$ref": "#/components/schemas/Widget" },
      "example": { "id": 1, "name": "Blue Widget" }
    }
  }
}
```

## Multiple examples

Implement `IMultipleOpenApiExamplesProvider<TModel>` to offer several named examples. Each one becomes a
selectable entry in Swagger UI and Scalar.

```csharp
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
```

Attach it with the plural methods:

```csharp
app.MapGet("/widgets", () => TypedResults.Ok(widgets))
    .ResponseExamples<WidgetExamples>(200, "application/json");
```

`OpenApiExample.Create` has three overloads — key and value, plus optional `summary` and `description`, both of
which are carried into the document.

## Applying examples to a route group

Response examples can be declared once for a whole group:

```csharp
var api = app.MapGroup("/api")
    .ResponseExample<ProblemDetailsExample>(400, "application/problem+json");

api.MapGet("/widgets", () => TypedResults.Ok(widgets));
api.MapGet("/gadgets", () => TypedResults.Ok(gadgets));
```

Both endpoints pick up the `400` example. Request examples are per-endpoint only.

## Available methods

| Method | Applies to | Provider interface |
|---|---|---|
| `RequestExample<T>(contentType)` | endpoint | `ISingleOpenApiExamplesProvider` |
| `RequestExamples<T>(contentType)` | endpoint | `IMultipleOpenApiExamplesProvider` |
| `ResponseExample<T>(statusCode, contentType)` | endpoint, group | `ISingleOpenApiExamplesProvider` |
| `ResponseExamples<T>(statusCode, contentType)` | endpoint, group | `IMultipleOpenApiExamplesProvider` |

`ResponseExample` accepts the status code as either an `int` or a `string`.

## Content types

| Content type | Written as |
|---|---|
| `application/json` | JSON object, array or value |
| `application/problem+json` | JSON object, array or value |
| `application/xml` | string containing the serialized XML |
| `text/xml` | string containing the serialized XML |

XML examples are emitted as strings because that is what the OpenAPI specification calls for — the example
lives inside a JSON document, so the XML payload is a string value within it.

> **Note**
> An example is only written if the operation actually declares that content type. When you use a content type
> the endpoint does not produce by default, declare it as well:
> ```csharp
> app.MapGet("/widgets/{id}", (int id) => TypedResults.Ok(widget))
>     .Produces<Widget>(200, "application/xml")
>     .ResponseExample<WidgetExample>(200, "application/xml");
> ```

## Configuration

```csharp
builder.Services.AddOpenApiExamples(options =>
{
    options.JsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    options.XmlSerializerOptions.Encoding = Encoding.UTF8;
});
```

| Option | Purpose |
|---|---|
| `JsonSerializerOptions` | Controls how examples are serialized for JSON content types |
| `XmlSerializerOptions.Encoding` | Encoding declared in the generated XML |
| `Formatters` | The content type to formatter map, keyed by content type |

> **Important**
> `JsonSerializerOptions` defaults to `new JsonSerializerOptions()`, which keeps your property names as
> written, while ASP.NET Core generates **camelCase** schemas. If your models are PascalCase, your examples
> will not match your schemas and spec validators will flag them. Passing
> `new JsonSerializerOptions(JsonSerializerDefaults.Web)` as shown above lines the two up.

## Providers and dependency injection

Providers are constructed through `ActivatorUtilities`, so they can take constructor dependencies and do not
need to be registered themselves:

```csharp
public class WidgetExample : ISingleOpenApiExamplesProvider<Widget>
{
    private readonly IWidgetCatalog catalog;

    public WidgetExample(IWidgetCatalog catalog) => this.catalog = catalog;

    public IOpenApiExample<Widget> GetExample() =>
        OpenApiExample.Create("default", this.catalog.Featured);
}
```

## Custom formatters

Implement `IOpenApiExamplesFormatter` to support a content type of your own:

```csharp
using System.Text.Json.Nodes;
using OpenApiExamples.Abstractions;

public class YamlExamplesFormatter : IOpenApiExamplesFormatter
{
    public IEnumerable<string> SupportedContentTypes => ["application/yaml"];

    public ValueTask<JsonNode> FormatAsync(object example)
    {
        var yaml = MyYamlSerializer.Serialize(example);
        return ValueTask.FromResult<JsonNode>(JsonValue.Create(yaml)!);
    }
}
```

Register it after `AddOpenApiExamples`:

```csharp
builder.Services
    .AddOpenApiExamples()
    .AddExamplesFormatter<YamlExamplesFormatter>();
```

A formatter claiming a content type that is already mapped replaces the existing one and logs a warning, which
is how you override the built-in JSON or XML behaviour. Registering the same formatter type twice throws.

## Requirements

- .NET 10
- `Microsoft.AspNetCore.OpenApi` (ASP.NET Core's built-in OpenAPI document generation)

## License

[MIT](LICENSE) © Chris Mavrommatis
