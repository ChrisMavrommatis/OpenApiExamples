using System.Text.Json.Nodes;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using OpenApiExamples.Abstractions;
using OpenApiExamples.Models;

namespace OpenApiExamples.Services;

internal class XmlOpenApiExamplesFormatter : IOpenApiExamplesFormatter
{
    private readonly IOptions<OpenApiExamplesOptions> options;

    public XmlOpenApiExamplesFormatter(
        IOptions<OpenApiExamplesOptions> options
    )
    {
        this.options = options;
    }
	
    public IEnumerable<string> SupportedContentTypes =>
    [
        "application/xml",
        "text/xml",
    ];
	
    public ValueTask<JsonNode> FormatAsync(object example)
    {
        var serializer = new XmlSerializer(example.GetType());

        using var stringWriter = new StringWriterWithEncoding(this.options.Value.XmlSerializerOptions.Encoding);
        serializer.Serialize(stringWriter, example);
        var xml = stringWriter.ToString();
        var openApiExample = JsonValue.Create(xml);
        return ValueTask.FromResult<JsonNode>(openApiExample);
    }
}