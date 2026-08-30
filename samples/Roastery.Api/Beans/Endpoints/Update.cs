using System.Net.Mime;
using Roastery.Api.Beans.Contracts;
using Roastery.Api.Data;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Beans.Endpoints;

internal static class Update
{
    public static RouteGroupBuilder MapUpdateBean(this RouteGroupBuilder group)
    {
        group.MapPut("{id}", Handle)
            .WithName("beans.update")
            .WithSummary("Update a bean")
            .RequestExample<UpdateBeanRequestExample>(MediaTypeNames.Application.Json)
            .Produces<Bean>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ResponseExample<BeanExample>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ResponseExample<NotFoundProblemExample>(
                StatusCodes.Status404NotFound,
                MediaTypeNames.Application.ProblemJson
            );

        return group;
    }

    private static IResult Handle(string id, UpdateBeanRequest request)
    {
        var bean = MockData.Beans.FirstOrDefault(bean => bean.Id == id);

        if (bean is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, detail: "No bean is filed under that id.");
        }

        var updated = bean with
        {
            Name = request.Name,
            Roast = request.Roast,
            PricePerBag = request.PricePerBag,
            InStock = request.InStock,
        };

        return TypedResults.Ok(updated);
    }
}
