namespace HomeChef.Application.Features.Search.Contracts;

public sealed record SearchQueryFilter
{
    public string? Query { get; init; }

    public string? City { get; init; }

    public string? Area { get; init; }

    public string? Cuisine { get; init; }

    public Guid? CategoryId { get; init; }

    public double? Lat { get; init; }

    public double? Lng { get; init; }

    public double? RadiusKm { get; init; }

    public string? Type { get; init; } // "all", "chefs", "foods"

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
