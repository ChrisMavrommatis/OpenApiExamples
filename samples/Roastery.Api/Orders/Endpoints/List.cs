using System.Net.Mime;
using Roastery.Api.Data;
using Roastery.Api.Shared;
using Roastery.Api.Orders.Contracts;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Orders.Endpoints;

internal static class List
{
    public static RouteGroupBuilder MapListOrders(this RouteGroupBuilder group)
    {
        group.MapGet("", Handle)
            .WithName("orders.list")
            .WithSummary("List orders")
            .WithDescription("Today's orders, newest first. Filter by status.")
            .Produces<Order[]>(StatusCodes.Status200OK, MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)
            .ResponseExamples<OrderListExamples>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)
            .ResponseExamples<OrderListExamples>(StatusCodes.Status200OK, MediaTypeNames.Application.Xml);

        return group;
    }

    private static IResult Handle(OrderStatus? status)
    {
        var orders = MockData.Orders
            .Where(order => status is null || order.Status == status)
            .OrderByDescending(order => order.PlacedAt)
            .ToArray();

        return ContentNegotiation.Ok(orders);
    }
}
