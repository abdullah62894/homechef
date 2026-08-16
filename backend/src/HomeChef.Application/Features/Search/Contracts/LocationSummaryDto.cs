namespace HomeChef.Application.Features.Search.Contracts;

public sealed record AreaSummaryDto
{
    public required string Name { get; init; }

    public int ChefCount { get; init; }
}

public sealed record CitySummaryDto
{
    public required string City { get; init; }

    public int TotalChefs { get; init; }

    public required IReadOnlyList<AreaSummaryDto> Areas { get; init; }
}

public sealed record LocationDirectoryDto
{
    public required IReadOnlyList<CitySummaryDto> Cities { get; init; }
}
