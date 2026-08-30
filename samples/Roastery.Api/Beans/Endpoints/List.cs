using System.Net.Mime;
using Roastery.Api.Beans.Contracts;
using Roastery.Api.Data;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Beans.Endpoints;

internal static class List
{
    public static RouteGroupBuilder MapListBeans(this RouteGroupBuilder group)
    {
        group.MapGet("", Handle)
            .WithName("beans.list")
            .WithSummary("List beans")
            .WithDescription("Everything on the shelf, narrowed by origin or roast.")
            .Produces<Bean[]>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ResponseExamples<BeanListExamples>(
                StatusCodes.Status200OK,
                MediaTypeNames.Application.Json
            );

        return group;
    }

    private static IResult Handle(string? origin, RoastLevel? roast)
    {
        var beans = MockData.Beans
            .Where(bean => origin is null || bean.Origin.Contains(origin, StringComparison.OrdinalIgnoreCase))
            .Where(bean => roast is null || bean.Roast == roast)
            .ToArray();

        return TypedResults.Ok(beans);
    }
}
