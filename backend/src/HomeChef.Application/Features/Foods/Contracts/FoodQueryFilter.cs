namespace HomeChef.Application.Features.Foods.Contracts;

public sealed record FoodQueryFilter
{
    public Guid? ChefId { get; init; }

    public Guid? CategoryId { get; init; }

    public string? Search { get; init; }

    public string? City { get; init; }

    public string? Area { get; init; }

    public string? Cuisine { get; init; }

    public double? Lat { get; init; }

    public double? Lng { get; init; }

    public double? RadiusKm { get; init; }

    public bool? IsAvailable { get; init; }
}
