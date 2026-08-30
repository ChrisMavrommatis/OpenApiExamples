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
- 🤝 Examples inherit your app's JSON settings, so they match the schemas beside them

Your OpenAPI docs deserve better than empty schemas. ASP.NET Core generates the shape of your payloads but
never the content, so your consumers get a `Widget` with no widget in it. OpenApiExamples fills that in.

## Install

```text
dotnet add package OpenApiExamples
```

## Write an example

A provider is a plain class that returns your model. No registration needed.

```csharp
public class CreateWidgetExample : ISingleOpenApiExamplesProvider<Widget>
{
    public IOpenApiExample<Widget> GetExample() =>
        OpenApiExample.Create("default", new Widget { Id = 1, Name = "Blue Widget" });
}
```

Attach it to an endpoint:

```csharp
app.MapPost("/widgets", (Widget widget) => TypedResults.Ok(widget))
    .RequestExample<CreateWidgetExample>("application/json")
    .ResponseExample<CreateWidgetExample>(200, "application/json");
```

Your document now carries the example inline, and Swagger UI pre-fills its "try it out" box with it:

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

Want a dropdown of named examples instead? Implement `IMultipleOpenApiExamplesProvider<T>` and attach it
with `RequestExamples` or `ResponseExamples`.

## Docs

| Page | What it covers |
|---|---|
| [Getting started](https://github.com/ChrisMavrommatis/OpenApiExamples/blob/main/docs/getting-started.md) | Registration, your first provider, what lands in the document |
| [Writing examples](https://github.com/ChrisMavrommatis/OpenApiExamples/blob/main/docs/examples.md) | Named examples, request and response, whole route groups, providers with dependencies |
| [Content types](https://github.com/ChrisMavrommatis/OpenApiExamples/blob/main/docs/content-types.md) | JSON, XML and problem+json |
| [Configuration](https://github.com/ChrisMavrommatis/OpenApiExamples/blob/main/docs/configuration.md) | Serializer options, and why an example might not match its schema |
| [Custom formatters](https://github.com/ChrisMavrommatis/OpenApiExamples/blob/main/docs/formatters.md) | Any content type that is not built in |

## Sample

[`samples/Roastery.Api`](https://github.com/ChrisMavrommatis/OpenApiExamples/tree/main/samples/Roastery.Api)
is a running minimal API using every feature above, across three route groups: one JSON only, one serving
both JSON and XML, one XML only.

```text
dotnet run --project samples/Roastery.Api
```

## Scope

This is a minimal API library and it stays one. Examples are written at document-generation time, so a
provider that reaches for a database does that work at startup rather than per request.

## Requirements

- .NET 10
- `Microsoft.AspNetCore.OpenApi`, ASP.NET Core's built-in OpenAPI document generation

## License

[MIT](LICENSE) © Chris Mavrommatis
