using System.Net.Mime;
using Roastery.Api.Shared;
using Roastery.Api.Orders.Contracts;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Orders.Endpoints;

internal static class Place
{
    public static RouteGroupBuilder MapPlaceOrder(this RouteGroupBuilder group)
    {
        group.MapPost("", Handle)
            .WithName("orders.place")
            .WithSummary("Place an order")
            .WithDescription("Drinks, beans by weight, or both on the same order.")
            // BindAsync takes the parameter off the body, so the body has to be declared here or it
            // vanishes from the document and the request examples have nowhere to land.
            .Accepts<PlaceOrderRequest>(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)
            .RequestExamples<PlaceOrderRequestExamples>(MediaTypeNames.Application.Json)
            .RequestExamples<PlaceOrderRequestExamples>(MediaTypeNames.Application.Xml)
            .Produces<Order>(StatusCodes.Status201Created, MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)
            .ResponseExample<OrderExample>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .ResponseExample<OrderExample>(StatusCodes.Status201Created, MediaTypeNames.Application.Xml)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ResponseExample<ValidationProblemExample>(
                StatusCodes.Status400BadRequest,
                MediaTypeNames.Application.ProblemJson
            );

        return group;
    }

    private static IResult Handle(PlaceOrderRequest request)
    {
        var id = $"ORD-{Random.Shared.Next(1000, 9999)}";

        var order = new Order
        {
            Id = id,
            CustomerName = request.CustomerName,
            Status = OrderStatus.Received,
            PlacedAt = DateTimeOffset.UtcNow,
            PickupSlot = request.PickupSlot,
            Lines = request.Lines,
            // Flat 3.20 an item until someone gives me a price list.
            Total = request.Lines.Sum(line => line.Quantity * 3.20m),
        };

        return ContentNegotiation.Created($"/api/orders/{id}", order);
    }
}
