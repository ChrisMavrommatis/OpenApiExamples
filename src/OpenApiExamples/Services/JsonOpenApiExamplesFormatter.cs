using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OpenApiExamples.Abstractions;

namespace OpenApiExamples.Services;

internal class JsonOpenApiExamplesFormatter : IOpenApiExamplesFormatter
{
    private readonly IOptions<OpenApiExamplesOptions> options;

    public JsonOpenApiExamplesFormatter(
        IOptions<OpenApiExamplesOptions> options
    )
    {
        this.options = options;
    }
    public IEnumerable<string> SupportedContentTypes =>
    [
        "application/json",
        "application/problem+json",
    ];
	
    public ValueTask<JsonNode> FormatAsync(object example)
    {
        var serializedExample = JsonSerializer.Serialize(example, this.options.Value.JsonSerializerOptions);
        var openApiExample = JsonValue.Create(serializedExample);
        return ValueTask.FromResult<JsonNode>(openApiExample);
    }
}