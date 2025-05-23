using Microsoft.AspNetCore.OpenApi;

namespace OpenApiExamples.ExtensionMethods;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions AddExamples(this OpenApiOptions options)
    {
        options.AddOperationTransformer<ResponseExamplesOperationTransformer>();
        options.AddOperationTransformer<RequestExamplesOperationTransformer>();
        return options;
    }
}