namespace HomeChef.Application.Features.Chefs.Contracts;

public sealed record ChefQueryFilter
{
    public string? Search { get; init; }

    public string? City { get; init; }

    public string? Area { get; init; }

    public string? Cuisine { get; init; }

    public double? Lat { get; init; }

    public double? Lng { get; init; }

    public double? RadiusKm { get; init; }
}
