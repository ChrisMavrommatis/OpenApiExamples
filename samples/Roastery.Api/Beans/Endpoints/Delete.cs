using System.Net.Mime;
using Roastery.Api.Data;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Beans.Endpoints;

internal static class Delete
{
    public static RouteGroupBuilder MapDeleteBean(this RouteGroupBuilder group)
    {
        group.MapDelete("{id}", Handle)
            .WithName("beans.delete")
            .WithSummary("Take a bean off the shelf")
            .Produces(StatusCodes.Status204NoContent)
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
            : TypedResults.NoContent();
    }
}
