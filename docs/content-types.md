# Content types

| Content type | Written as |
|---|---|
| `application/json` | JSON object, array or value |
| `application/problem+json` | JSON object, array or value |
| `application/xml` | string containing the serialized XML |
| `text/xml` | string containing the serialized XML |

XML comes out as a string rather than a nested structure. That is the specification, not a bug: the document
you are writing into is itself JSON, so the XML payload lives in it as a string value.

## XML examples in Swagger UI

Swagger UI draws its own XML sample from the schema, and an array schema carries no root element name, so a
list endpoint shows `XML example cannot be generated` instead. One transformer names them:

```csharp
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
```

`ArrayOf{Name}` is what `XmlSerializer` writes, so the root element matches your real payload. The
property names inside it still come from the JSON schema, so they stay camelCase where your XML is
PascalCase.

## Declaring a content type

An example is only written if the operation actually declares that content type. When you use one the
endpoint does not produce by default, declare it as well:

```csharp
app.MapGet("/widgets/{id}", (int id) => TypedResults.Ok(new Widget()))
    .Produces<Widget>(200, "application/xml")
    .ResponseExample<WidgetExample>(200, "application/xml");
```

`Produces` replaces per status code rather than adding to it, so list every content type for one status code
in a single call:

```csharp
app.MapGet("/widgets/{id}", (int id) => TypedResults.Ok(new Widget()))
    .Produces<Widget>(200, "application/json", "application/xml")
    .ResponseExample<WidgetExample>(200, "application/json")
    .ResponseExample<WidgetExample>(200, "application/xml");
```
