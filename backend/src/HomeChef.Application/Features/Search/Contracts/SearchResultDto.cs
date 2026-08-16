using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Foods.Contracts;

namespace HomeChef.Application.Features.Search.Contracts;

public sealed record SearchResultDto
{
    public required IReadOnlyList<ChefListItemDto> Chefs { get; init; }

    public required IReadOnlyList<FoodListItemDto> Foods { get; init; }

    public int TotalChefs { get; init; }

    public int TotalFoods { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }
}
