namespace HomeChef.Application.Features.Foods.Contracts;

public sealed record FoodListItemDto
{
    public required Guid Id { get; init; }

    public required Guid ChefProfileId { get; init; }

    public required string ChefDisplayName { get; init; }

    public required string ChefCity { get; init; }

    public string? ChefArea { get; init; }

    public string? ChefAddress { get; init; }

    public double? DistanceKm { get; init; }

    public Guid? CategoryId { get; init; }

    public string? CategoryName { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required decimal Price { get; init; }

    public required string Currency { get; init; }

    public required bool IsAvailable { get; init; }

    public string? ImageUrl { get; init; }

    public int? PreparationTimeMinutes { get; init; }
}
