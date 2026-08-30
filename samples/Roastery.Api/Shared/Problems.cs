using OpenApiExamples;
using OpenApiExamples.Abstractions;

namespace Roastery.Api.Shared;

/// <summary>
/// Shaped like RFC 9457, which is what <c>ProducesProblem</c> puts in the schema.
/// </summary>
public record Problem
{
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required int Status { get; init; }
    public string? Detail { get; init; }
    public string? Instance { get; init; }
}

public class NotFoundProblemExample : ISingleOpenApiExamplesProvider<Problem>
{
    public IOpenApiExample<Problem> GetExample() =>
        OpenApiExample.Create(
            key: "notFound",
            summary: "Nothing under that id",
            value: new Problem
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Title = "Not Found",
                Status = 404,
                Detail = "No bean or order is filed under that id.",
            }
        );
}

public class ValidationProblemExample : ISingleOpenApiExamplesProvider<Problem>
{
    public IOpenApiExample<Problem> GetExample() =>
        OpenApiExample.Create(
            key: "validation",
            summary: "The body did not make sense",
            value: new Problem
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "quantity must be at least 1.",
            }
        );
}

public class ServerErrorProblemExample : ISingleOpenApiExamplesProvider<Problem>
{
    public IOpenApiExample<Problem> GetExample() =>
        OpenApiExample.Create(
            key: "serverError",
            summary: "The grinder jammed",
            value: new Problem
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title = "Internal Server Error",
                Status = 500,
                Detail = "Something went wrong on our side. Try again.",
            }
        );
}
