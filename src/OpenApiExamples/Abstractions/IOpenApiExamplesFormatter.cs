using System.Text.Json.Nodes;

namespace OpenApiExamples.Abstractions;

public interface IOpenApiExamplesFormatter
{
    IEnumerable<string> SupportedContentTypes { get; }
    ValueTask<JsonNode> FormatAsync(object example);
}