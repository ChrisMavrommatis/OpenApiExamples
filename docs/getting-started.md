# Getting started

**1. Register the services and the transformers.**

```csharp
using OpenApiExamples.ExtensionMethods;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApiExamples();

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

## Next

- [Writing examples](examples.md) - many named examples, request and response, whole route groups
- [Content types](content-types.md) - JSON, XML, and declaring the ones your endpoint does not produce by default
- [Configuration](configuration.md) - serializer options, and why an example might not match its schema
