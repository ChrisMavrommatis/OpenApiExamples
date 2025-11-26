using Microsoft.OpenApi;

namespace OpenApiExamples.Abstractions;

internal interface IOpenApiExamplesWriter
{
    ValueTask WriteAsync(OpenApiMediaType content, string itemContentType, Type itemProviderType);
}