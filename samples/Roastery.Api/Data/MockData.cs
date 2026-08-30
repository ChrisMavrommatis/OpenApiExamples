using Roastery.Api.Beans.Contracts;
using Roastery.Api.Invoices.Contracts;
using Roastery.Api.Orders.Contracts;

namespace Roastery.Api.Data;

/// <summary>
/// The whole "database". Handlers and example providers both read from here, so the docs and the
/// canned responses can never drift apart.
/// </summary>
public static class MockData
{
    // Fixed, so the generated document is the same on every run.
    private static readonly DateTimeOffset Morning = new(2026, 3, 14, 8, 5, 0, TimeSpan.FromHours(2));

    public static readonly Bean[] Beans =
    [
        new Bean
        {
            Id = "kenya-nyeri-aa",
            Name = "Nyeri AA",
            Origin = "Kenya",
            Roast = RoastLevel.Light,
            BagSizeGrams = 250,
            PricePerBag = 12.50m,
            InStock = true,
            TastingNotes = ["blackcurrant", "grapefruit", "cane sugar"],
        },
        new Bean
        {
            Id = "house-blend",
            Name = "House Blend",
            Origin = "Brazil / Ethiopia",
            Roast = RoastLevel.MediumDark,
            BagSizeGrams = 1000,
            PricePerBag = 28.00m,
            InStock = true,
            TastingNotes = ["cocoa", "hazelnut"],
        },
        new Bean
        {
            Id = "sumatra-gayo",
            Name = "Gayo Highlands",
            Origin = "Indonesia",
            Roast = RoastLevel.Dark,
            BagSizeGrams = 250,
            PricePerBag = 11.00m,
            InStock = false,
            TastingNotes = ["cedar", "dark chocolate", "tobacco"],
        },
        new Bean
        {
            Id = "guji-decaf",
            Name = "Guji Decaf",
            Origin = "Ethiopia",
            Roast = RoastLevel.Medium,
            BagSizeGrams = 250,
            PricePerBag = 13.00m,
            InStock = true,
            TastingNotes = ["peach", "jasmine"],
        },
    ];

    public static readonly Order[] Orders =
    [
        new Order
        {
            Id = "ORD-1042",
            CustomerName = "Ellie Brand",
            Status = OrderStatus.Ready,
            PlacedAt = Morning,
            PickupSlot = Morning.AddMinutes(25),
            Lines =
            [
                new OrderLine { Item = "iced-espresso", Quantity = 2, Sweetness = Sweetness.Medium },
                new OrderLine { Item = "flat-white", Quantity = 1, Sweetness = Sweetness.Sweet, WithMilk = true },
            ],
            Total = 9.60m,
        },
        new Order
        {
            Id = "ORD-1043",
            CustomerName = "Sam Okafor",
            Status = OrderStatus.Received,
            PlacedAt = Morning.AddMinutes(12),
            Lines =
            [
                new OrderLine { Item = "filter", Quantity = 1, Sweetness = Sweetness.Unsweetened },
                new OrderLine { Item = "kenya-nyeri-aa", Quantity = 1, Grams = 500 },
            ],
            Total = 27.30m,
        },
    ];

    public static readonly Invoice[] Invoices =
    [
        new Invoice
        {
            Number = "INV-2026-0041",
            Customer = "Northgate Deli",
            IssuedOn = new DateTime(2026, 3, 1),
            DueOn = new DateTime(2026, 3, 31),
            Status = InvoiceStatus.Paid,
            Lines =
            [
                new InvoiceLine { BeanId = "house-blend", Bags = 5, PricePerBag = 28.00m, LineTotal = 140.00m },
            ],
            Net = 140.00m,
            Vat = 33.60m,
            Gross = 173.60m,
        },
        new Invoice
        {
            Number = "INV-2026-0043",
            Customer = "Harbour Cafe",
            IssuedOn = new DateTime(2026, 3, 12),
            DueOn = new DateTime(2026, 4, 11),
            Status = InvoiceStatus.Overdue,
            Lines =
            [
                new InvoiceLine { BeanId = "house-blend", Bags = 10, PricePerBag = 26.50m, LineTotal = 265.00m },
                new InvoiceLine { BeanId = "kenya-nyeri-aa", Bags = 4, PricePerBag = 12.50m, LineTotal = 50.00m },
            ],
            Net = 315.00m,
            Vat = 75.60m,
            Gross = 390.60m,
        },
    ];
}
