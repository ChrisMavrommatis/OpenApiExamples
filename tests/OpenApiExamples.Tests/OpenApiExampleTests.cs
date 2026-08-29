using OpenApiExamples.Abstractions;
using OpenApiExamples.Tests.TestDoubles;

namespace OpenApiExamples.Tests;

// The factory overloads are the surface a consumer types most often, so they are pinned directly rather
// than only through the document tests.
public class OpenApiExampleTests
{
    private static readonly Widget Widget = new() { Id = 1, Name = "box" };

    [Fact]
    public void Create_WithKeyAndValue_LeavesSummaryAndDescriptionNull()
    {
        var example = OpenApiExample.Create("default", Widget);

        Assert.Equal("default", example.Key);
        Assert.Same(Widget, example.Value);
        Assert.Null(example.Summary);
        Assert.Null(example.Description);
    }

    [Fact]
    public void Create_WithSummary_LeavesDescriptionNull()
    {
        var example = OpenApiExample.Create("default", "A widget", Widget);

        Assert.Equal("A widget", example.Summary);
        Assert.Null(example.Description);
    }

    [Fact]
    public void Create_WithSummaryAndDescription_SetsBoth()
    {
        var example = OpenApiExample.Create("default", "A widget", "Holds things", Widget);

        Assert.Equal("A widget", example.Summary);
        Assert.Equal("Holds things", example.Description);
    }

    [Fact]
    public void NonGenericValue_ReturnsTheSameInstance()
    {
        // The writer reads examples through the non-generic interface, so this is the accessor it uses.
        IOpenApiExample example = (IOpenApiExample)OpenApiExample.Create("default", Widget);

        Assert.Same(Widget, example.Value);
    }
}
