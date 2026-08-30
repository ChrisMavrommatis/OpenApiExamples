using System.Net.Mime;
using Roastery.Api.Data;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Orders.Endpoints;

internal static class Cancel
{
    public static RouteGroupBuilder MapCancelOrder(this RouteGroupBuilder group)
    {
        group.MapDelete("{id}", Handle)
            .WithName("orders.cancel")
            .WithSummary("Cancel an order")
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
        var order = MockData.Orders.FirstOrDefault(order => order.Id == id);

        return order is null
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, detail: "No order is filed under that id.")
            : TypedResults.NoContent();
    }
}
