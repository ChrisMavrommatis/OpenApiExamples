# Custom formatters

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
