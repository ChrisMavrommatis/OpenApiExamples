using OpenApiExamples.Abstractions;

namespace OpenApiExamples.Tests.TestDoubles;

// Public with a parameterless constructor because XmlSerializer requires both.
public class Widget
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SingleWidgetExample : ISingleOpenApiExamplesProvider<Widget>
{
    public IOpenApiExample<Widget> GetExample() =>
        OpenApiExample.Create("default", new Widget { Id = 1, Name = "box" });
}

public class MultipleWidgetExamples : IMultipleOpenApiExamplesProvider<Widget>
{
    public IEnumerable<IOpenApiExample<Widget>> GetExamples() =>
    [
        OpenApiExample.Create("small", "A small widget", new Widget { Id = 1, Name = "box" }),
        OpenApiExample.Create("large", "A large widget", "Takes two people to lift",
            new Widget { Id = 2, Name = "crate" }),
    ];
}

// Implements neither provider interface - the writer is expected to log and move on rather than throw.
public class NotAnExampleProvider
{
}

// A provider whose collection is empty, so the writer never creates the Examples dictionary.
public class NoWidgetExamples : IMultipleOpenApiExamplesProvider<Widget>
{
    public IEnumerable<IOpenApiExample<Widget>> GetExamples() => [];
}

public class WidgetNamer
{
    public string Name => "injected";
}

// Providers are built with ActivatorUtilities, so a constructor dependency has to come from the container.
public class InjectedWidgetExample : ISingleOpenApiExamplesProvider<Widget>
{
    private readonly WidgetNamer namer;

    public InjectedWidgetExample(WidgetNamer namer)
    {
        this.namer = namer;
    }

    public IOpenApiExample<Widget> GetExample() =>
        OpenApiExample.Create("default", new Widget { Id = 7, Name = this.namer.Name });
}
