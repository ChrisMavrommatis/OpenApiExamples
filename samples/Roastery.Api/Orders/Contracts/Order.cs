using Roastery.Api.Shared;

namespace Roastery.Api.Orders.Contracts;

public enum OrderStatus
{
    Received,
    Grinding,
    Ready,
    Collected,
    Cancelled,
}

public enum Sweetness
{
    Unsweetened,
    Medium,
    Sweet,
}

/// <summary>
/// One line of an order. Either a drink, or beans by weight, never both.
/// </summary>
public record OrderLine
{
    public required string Item { get; init; }
    public required int Quantity { get; init; }

    /// <summary>Drinks only.</summary>
    public Sweetness? Sweetness { get; init; }

    /// <summary>Drinks only.</summary>
    public bool WithMilk { get; init; }

    /// <summary>Beans only.</summary>
    public int? Grams { get; init; }
}

public record Order
{
    public required string Id { get; init; }
    public required string CustomerName { get; init; }
    public required OrderStatus Status { get; init; }
    public required DateTimeOffset PlacedAt { get; init; }
    public DateTimeOffset? PickupSlot { get; init; }
    public required OrderLine[] Lines { get; init; }
    public required decimal Total { get; init; }
}

public record PlaceOrderRequest
{
    public required string CustomerName { get; init; }
    public DateTimeOffset? PickupSlot { get; init; }
    public required OrderLine[] Lines { get; init; }

    // Partners post XML, our own app posts JSON. BindAsync takes both, so the endpoint takes both.
    public static ValueTask<PlaceOrderRequest?> BindAsync(HttpContext httpContext) =>
        RequestBody.ReadAsync<PlaceOrderRequest>(httpContext);
}

public record UpdateOrderStatusRequest
{
    public required OrderStatus Status { get; init; }

    public static ValueTask<UpdateOrderStatusRequest?> BindAsync(HttpContext httpContext) =>
        RequestBody.ReadAsync<UpdateOrderStatusRequest>(httpContext);
}
