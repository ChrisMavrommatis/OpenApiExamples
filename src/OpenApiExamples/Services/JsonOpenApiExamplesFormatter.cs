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
        // SerializeToNode, not Serialize + JsonValue.Create: the latter produces a JSON *string* holding the
        // serialized object, so the example lands in the document double encoded -
        // "example": "{\"id\":1}" instead of "example": { "id": 1 }. Swagger UI, Scalar and generated SDKs then
        // show an escaped blob, and Spectral's oas3-valid-media-example flags every one of them.
        // The XML formatter's JsonValue.Create is correct - XML in a JSON example really is a string.
        var openApiExample = JsonSerializer.SerializeToNode(example, this.options.Value.JsonSerializerOptions);
        return ValueTask.FromResult<JsonNode>(openApiExample!);
    }
}