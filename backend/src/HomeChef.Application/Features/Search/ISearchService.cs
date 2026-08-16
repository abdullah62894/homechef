using HomeChef.Application.Features.Search.Contracts;

namespace HomeChef.Application.Features.Search;

public interface ISearchService
{
    Task<SearchResultDto> SearchAsync(
        SearchQueryFilter filter,
        CancellationToken cancellationToken = default);

    Task<LocationDirectoryDto> GetLocationsAsync(CancellationToken cancellationToken = default);

    Task<CitySummaryDto?> GetCityLocationAsync(string city, CancellationToken cancellationToken = default);
}
