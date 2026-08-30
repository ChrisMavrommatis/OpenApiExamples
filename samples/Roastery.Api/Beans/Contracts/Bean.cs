namespace Roastery.Api.Beans.Contracts;

public enum RoastLevel
{
    Light,
    Medium,
    MediumDark,
    Dark,
}

/// <summary>
/// A coffee we sell by the bag.
/// </summary>
public record Bean
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Origin { get; init; }
    public required RoastLevel Roast { get; init; }
    public required int BagSizeGrams { get; init; }
    public required decimal PricePerBag { get; init; }
    public required bool InStock { get; init; }
    public required string[] TastingNotes { get; init; }
}

public record CreateBeanRequest
{
    public required string Name { get; init; }
    public required string Origin { get; init; }
    public required RoastLevel Roast { get; init; }
    public required int BagSizeGrams { get; init; }
    public required decimal PricePerBag { get; init; }
    public string[] TastingNotes { get; init; } = [];
}

public record UpdateBeanRequest
{
    public required string Name { get; init; }
    public required RoastLevel Roast { get; init; }
    public required decimal PricePerBag { get; init; }
    public required bool InStock { get; init; }
}
