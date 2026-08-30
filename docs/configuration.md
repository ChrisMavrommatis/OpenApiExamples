# Configuration

Nothing here is required. Configure it when you want examples to behave differently from the rest of your
app:

```csharp
builder.Services.AddOpenApiExamples(options =>
    options.XmlSerializerOptions.Encoding = Encoding.UTF8);
```

| Option | Purpose |
|---|---|
| `JsonSerializerOptions` | How examples are serialized for JSON content types. Defaults to a copy of your app's |
| `XmlSerializerOptions.Encoding` | Encoding declared in the generated XML |
| `Formatters` | The content type to formatter map, keyed by content type |

## Why does my example not match my schema?

It should. Examples are serialized with a copy of your app's own JSON settings, so one call covers your
schemas and your examples together:

```csharp
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

Setting `options.JsonSerializerOptions` replaces that copy rather than adding to it, so anything your app
configured is gone unless you configure it again:

```csharp
builder.Services.AddOpenApiExamples(options =>
    options.JsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = null });
```

Only the minimal-API `JsonOptions` is read, never MVC's.
