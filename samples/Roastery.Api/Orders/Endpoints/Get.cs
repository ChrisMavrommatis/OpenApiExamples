using System.Net.Mime;
using Roastery.Api.Data;
using Roastery.Api.Shared;
using Roastery.Api.Orders.Contracts;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Orders.Endpoints;

internal static class Get
{
    public static RouteGroupBuilder MapGetOrder(this RouteGroupBuilder group)
    {
        group.MapGet("{id}", Handle)
            .WithName("orders.get")
            .WithSummary("Get an order")
            .Produces<Order>(StatusCodes.Status200OK, MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)
            .ResponseExample<OrderExample>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ResponseExample<OrderExample>(StatusCodes.Status200OK, MediaTypeNames.Application.Xml)
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
            : ContentNegotiation.Ok(order);
    }
}
