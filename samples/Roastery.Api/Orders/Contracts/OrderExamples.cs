using Roastery.Api.Data;
using OpenApiExamples;
using OpenApiExamples.Abstractions;

namespace Roastery.Api.Orders.Contracts;

public class OrderExample : ISingleOpenApiExamplesProvider<Order>
{
    public IOpenApiExample<Order> GetExample() =>
        OpenApiExample.Create("default", MockData.Orders[0]);
}

public class OrderListExamples : IMultipleOpenApiExamplesProvider<Order[]>
{
    public IEnumerable<IOpenApiExample<Order[]>> GetExamples() =>
    [
        OpenApiExample.Create(
            key: "morningRush",
            summary: "Two orders on the counter",
            description: "Newest first, the same order the endpoint sorts them in.",
            value: MockData.Orders.OrderByDescending(order => order.PlacedAt).ToArray()
        ),
        OpenApiExample.Create(
            key: "ready",
            summary: "Filtered by status",
            description: "What GET /api/orders?status=Ready comes back with.",
            value: MockData.Orders.Where(o => o.Status == OrderStatus.Ready).ToArray()
        ),
    ];
}

/// <summary>
/// The three shapes worth showing a caller: one drink, a round for the table, and beans by weight.
/// </summary>
public class PlaceOrderRequestExamples : IMultipleOpenApiExamplesProvider<PlaceOrderRequest>
{
    public IEnumerable<IOpenApiExample<PlaceOrderRequest>> GetExamples() =>
    [
        OpenApiExample.Create(
            key: "singleDrink",
            summary: "One coffee, no sugar",
            value: new PlaceOrderRequest
            {
                CustomerName = "Sam Okafor",
                Lines =
                [
                    new OrderLine { Item = "filter", Quantity = 1, Sweetness = Sweetness.Unsweetened },
                ],
            }
        ),
        OpenApiExample.Create(
            key: "roundForTheTable",
            summary: "A round for the table, for later",
            description: "Set pickupSlot and we hold it until then.",
            value: new PlaceOrderRequest
            {
                CustomerName = "Ellie Brand",
                PickupSlot = new DateTimeOffset(2026, 3, 14, 8, 30, 0, TimeSpan.FromHours(2)),
                Lines =
                [
                    new OrderLine { Item = "iced-espresso", Quantity = 2, Sweetness = Sweetness.Medium },
                    new OrderLine { Item = "flat-white", Quantity = 1, Sweetness = Sweetness.Sweet, WithMilk = true },
                ],
            }
        ),
        OpenApiExample.Create(
            key: "bulkBeans",
            summary: "Five kilos of house blend",
            description: "A beans line carries grams and leaves sweetness off.",
            value: new PlaceOrderRequest
            {
                CustomerName = "Northgate Deli",
                Lines =
                [
                    new OrderLine { Item = "house-blend", Quantity = 5, Grams = 1000 },
                ],
            }
        ),
    ];
}

public class UpdateOrderStatusRequestExample : ISingleOpenApiExamplesProvider<UpdateOrderStatusRequest>
{
    public IOpenApiExample<UpdateOrderStatusRequest> GetExample() =>
        OpenApiExample.Create(
            key: "ready",
            summary: "Call it out to the counter",
            value: new UpdateOrderStatusRequest { Status = OrderStatus.Ready }
        );
}
