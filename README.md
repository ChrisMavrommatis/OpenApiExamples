# OpenApiExamples

**Make your API docs actually useful. Real request and response examples in your OpenAPI document, with zero boilerplate.**

[![NuGet](https://img.shields.io/nuget/v/OpenApiExamples.svg)](https://www.nuget.org/packages/OpenApiExamples)
[![Downloads](https://img.shields.io/nuget/dt/OpenApiExamples.svg)](https://www.nuget.org/packages/OpenApiExamples)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

OpenAPI examples for **ASP.NET Core minimal APIs** on **.NET 10**, rendered by **Swagger UI** and **Scalar**.

---

## Why OpenApiExamples?

- ✨ Rich, realistic examples in seconds
- ⚡ Zero runtime cost, examples are written when the document is generated
- 🧩 Seamless ASP.NET Core integration, no extra middleware
- 🎯 One example or many named ones, per endpoint or per route group
- 📄 JSON and XML built in, anything else through your own formatter
- 💉 First-class dependency injection support

Your OpenAPI docs deserve better than empty schemas. ASP.NET Core generates the shape of your payloads but
never the content, so your consumers get a `Widget` with no widget in it. OpenApiExamples fills that in.

## Install

```text
Install-Package OpenApiExamples
```

```text
dotnet add package OpenApiExamples
```

## Get started in three steps

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

**2. Write a provider.** A plain class that returns your example. No registration needed.

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

That's it. Your document now carries the example inline, and Swagger UI pre-fills its "try it out" box with it:

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

## Multiple named examples

Give your consumers a dropdown. Implement `IMultipleOpenApiExamplesProvider<TModel>` and every example gets a
key, a summary and a description:

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

`OpenApiExample.Create` has three overloads: key and value, plus optional `summary` and `description`, and
whichever you fill in are carried into the document.

## Route group examples

Declare a shared error shape once and every endpoint in the group picks it up:

```csharp
var api = app.MapGroup("/api")
    .ResponseExample<ProblemDetailsExample>(400, "application/problem+json");

api.MapGet("/widgets", () => TypedResults.Ok(widgets));
api.MapGet("/gadgets", () => TypedResults.Ok(gadgets));
```

Request examples are per endpoint, since a group has no single request body.

## API reference

| Method | Applies to | Provider interface |
|---|---|---|
| `RequestExample<T>(contentType)` | endpoint | `ISingleOpenApiExamplesProvider` |
| `RequestExamples<T>(contentType)` | endpoint | `IMultipleOpenApiExamplesProvider` |
| `ResponseExample<T>(statusCode, contentType)` | endpoint, group | `ISingleOpenApiExamplesProvider` |
| `ResponseExamples<T>(statusCode, contentType)` | endpoint, group | `IMultipleOpenApiExamplesProvider` |

`ResponseExample` and `ResponseExamples` both accept the status code as either an `int` or a `string`.

## Supported content types

| Content type | Written as |
|---|---|
| `application/json` | JSON object, array or value |
| `application/problem+json` | JSON object, array or value |
| `application/xml` | string containing the serialized XML |
| `text/xml` | string containing the serialized XML |

XML comes out as a string rather than a nested structure. That is the specification, not a bug: the document
you are writing into is itself JSON, so the XML payload lives in it as a string value.

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

### Why does my example not match my schema?

Almost always the casing. `JsonSerializerOptions` defaults to `new JsonSerializerOptions()`, which keeps your
property names exactly as written, while ASP.NET Core generates **camelCase** schemas. If your models are
PascalCase your examples will quietly disagree with your own schemas, and nothing tells you until a spec
validator does. Passing `new JsonSerializerOptions(JsonSerializerDefaults.Web)` lines the two up.

## Dependency injection

Providers are constructed through `ActivatorUtilities`, so they take constructor dependencies like anything
else in your application, and you still never register them yourself:

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

Need a content type that is not in the table? Write a formatter:

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

Claiming a content type that is already mapped replaces the existing formatter and logs a warning, which is
how you override the built-in JSON or XML behaviour. Registering the same formatter type twice throws.

## Scope

This is a minimal API library and it stays one. Examples are written at document-generation time, so a
provider that reaches for a database does that work at startup rather than per request.

## Requirements

- .NET 10
- `Microsoft.AspNetCore.OpenApi`, ASP.NET Core's built-in OpenAPI document generation

## License

[MIT](LICENSE) © Chris Mavrommatis
