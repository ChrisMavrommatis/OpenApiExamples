# Writing examples

## Many named examples

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
