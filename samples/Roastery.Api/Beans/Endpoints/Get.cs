using System.Net.Mime;
using Roastery.Api.Beans.Contracts;
using Roastery.Api.Data;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Beans.Endpoints;

internal static class Get
{
    public static RouteGroupBuilder MapGetBean(this RouteGroupBuilder group)
    {
        group.MapGet("{id}", Handle)
            .WithName("beans.get")
            .WithSummary("Get a bean")
            .Produces<Bean>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ResponseExample<BeanExample>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ResponseExample<NotFoundProblemExample>(
                StatusCodes.Status404NotFound,
                MediaTypeNames.Application.ProblemJson
            );

        return group;
    }

    private static IResult Handle(string id)
    {
        var bean = MockData.Beans.FirstOrDefault(bean => bean.Id == id);

        return bean is null
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, detail: "No bean is filed under that id.")
            : TypedResults.Ok(bean);
    }
}
