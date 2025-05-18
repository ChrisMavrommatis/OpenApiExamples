using OpenApiExamples.Abstractions;

namespace OpenApiExamples;

/// <summary>
/// Represents an example for OpenAPI documentation.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IOpenApiExample{T}"/> interface.
/// The <see cref="OpenApiExample.Create{T}(string, T)"/> methods are static factory methods that create instances of <see cref="OpenApiExample{T}"/>.
/// </remarks>
public class OpenApiExample<T> : IOpenApiExample<T>
{
	
    public required string Key { get; init; }

    public string? Summary { get; init; }

    public string? Description { get; init; }

    public required T Value { get; init; }

    object IOpenApiExample.Value => this.Value!;
}



public static class OpenApiExample
{
    /// <summary>
    /// Creates an example with the specified key and value.
    /// </summary>
    public static IOpenApiExample<T> Create<T>(string key, T value)
    {
        return new OpenApiExample<T>()
        {
            Key = key,
            Value = value
        };
    }
    /// <summary>
    /// Creates an example with the specified key, summary, and value.
    /// </summary>
	public static IOpenApiExample<T> Create<T>(string key, string summary, T value)
	{
		return new OpenApiExample<T>
		{
			Key = key,
			Summary = summary,
			Value = value
		};
	}

    /// <summary>
    /// Creates an example with the specified key, summary, description, and value.
    /// </summary>
	public static IOpenApiExample<T> Create<T>(string key, string summary, string description, T value)
	{
		return new OpenApiExample<T>
		{
			Key = key,
			Summary = summary,
			Description = description,
			Value = value
		};
	}
}