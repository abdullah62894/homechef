using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Foods.Contracts;
using HomeChef.Application.Features.Search.Contracts;

namespace HomeChef.Application.Features.Search;

public sealed class SearchService : ISearchService
{
    private readonly IChefProfileRepository _chefRepository;
    private readonly IFoodRepository _foodRepository;

    public SearchService(
        IChefProfileRepository chefRepository,
        IFoodRepository foodRepository)
    {
        _chefRepository = chefRepository;
        _foodRepository = foodRepository;
    }

    public async Task<SearchResultDto> SearchAsync(
        SearchQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, filter.Page);
        var pageSize = Math.Clamp(filter.PageSize, 1, 50);
        var type = string.IsNullOrWhiteSpace(filter.Type) ? "all" : filter.Type.Trim().ToLowerInvariant();

        var chefs = new List<ChefListItemDto>();
        var totalChefs = 0;

        var foods = new List<FoodListItemDto>();
        var totalFoods = 0;

        if (type is "all" or "chefs")
        {
            var chefFilter = new ChefQueryFilter
            {
                Search = filter.Query,
                City = filter.City,
                Area = filter.Area,
                Cuisine = filter.Cuisine,
                Lat = filter.Lat,
                Lng = filter.Lng,
                RadiusKm = filter.RadiusKm,
            };

            var (chefItems, total) = await _chefRepository.ListAsync(chefFilter, page, pageSize, cancellationToken);
            chefs = chefItems.Select(x => ChefService.ToListItem(x.Profile, x.DistanceKm)).ToList();
            totalChefs = total;
        }

        if (type is "all" or "foods")
        {
            var foodFilter = new FoodQueryFilter
            {
                Search = filter.Query,
                City = filter.City,
                Area = filter.Area,
                Cuisine = filter.Cuisine,
                CategoryId = filter.CategoryId,
                Lat = filter.Lat,
                Lng = filter.Lng,
                RadiusKm = filter.RadiusKm,
                IsAvailable = true,
            };

            var (foodItems, total) = await _foodRepository.ListAsync(foodFilter, page, pageSize, cancellationToken);
            foods = foodItems.Select(x => FoodService.ToListItem(x.Item, x.DistanceKm)).ToList();
            totalFoods = total;
        }

        return new SearchResultDto
        {
            Chefs = chefs,
            Foods = foods,
            TotalChefs = totalChefs,
            TotalFoods = totalFoods,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<LocationDirectoryDto> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        var counts = await _chefRepository.GetLocationCountsAsync(cancellationToken);

        var cities = counts
            .GroupBy(c => c.City, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var areas = g
                    .Where(x => !string.IsNullOrWhiteSpace(x.Area))
                    .Select(x => new AreaSummaryDto
                    {
                        Name = x.Area!,
                        ChefCount = x.Count,
                    })
                    .OrderBy(a => a.Name)
                    .ToList();

                var totalChefs = g.Sum(x => x.Count);

                return new CitySummaryDto
                {
                    City = g.Key,
                    TotalChefs = totalChefs,
                    Areas = areas,
                };
            })
            .OrderByDescending(c => c.TotalChefs)
            .ThenBy(c => c.City)
            .ToList();

        return new LocationDirectoryDto { Cities = cities };
    }

    public async Task<CitySummaryDto?> GetCityLocationAsync(string city, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city)) return null;

        var directory = await GetLocationsAsync(cancellationToken);
        return directory.Cities.FirstOrDefault(c => c.City.Equals(city.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
