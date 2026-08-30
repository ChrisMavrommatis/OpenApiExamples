using System.Net.Mime;
using Roastery.Api.Orders.Endpoints;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Orders;

internal static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/orders")
            .WithTags("Orders")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ResponseExample<ServerErrorProblemExample>(
                StatusCodes.Status500InternalServerError,
                MediaTypeNames.Application.ProblemJson
            );

        group
            .MapListOrders()
            .MapGetOrder()
            .MapPlaceOrder()
            .MapUpdateOrderStatus()
            .MapCancelOrder();

        return endpoints;
    }
}
