using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Search;
using HomeChef.Application.Features.Search.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly IChefService _chefService;

    public SearchController(ISearchService searchService, IChefService chefService)
    {
        _searchService = searchService;
        _chefService = chefService;
    }

    /// <summary>Universal search across chefs and food items.</summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SearchResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q = null,
        [FromQuery] string? city = null,
        [FromQuery] string? area = null,
        [FromQuery] string? cuisine = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lng = null,
        [FromQuery] double? radiusKm = null,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = new SearchQueryFilter
        {
            Query = q,
            City = city,
            Area = area,
            Cuisine = cuisine,
            CategoryId = categoryId,
            Lat = lat,
            Lng = lng,
            RadiusKm = radiusKm,
            Type = type,
            Page = page,
            PageSize = pageSize,
        };

        var result = await _searchService.SearchAsync(filter, cancellationToken);
        return Ok(new ApiResponse<SearchResultDto>(result));
    }

    /// <summary>Lists all cities/areas with chef counts.</summary>
    [HttpGet("locations")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<LocationDirectoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocations(CancellationToken cancellationToken)
    {
        var directory = await _searchService.GetLocationsAsync(cancellationToken);
        return Ok(new ApiResponse<LocationDirectoryDto>(directory));
    }

    /// <summary>Lists areas and chef counts for a specific city.</summary>
    [HttpGet("locations/{city}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CitySummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCity(string city, CancellationToken cancellationToken)
    {
        var result = await _searchService.GetCityLocationAsync(city, cancellationToken);
        if (result is null)
        {
            return NotFound(new ApiErrorResponse(new ApiError("NOT_FOUND", $"No chefs found in city '{city}'.")));
        }

        return Ok(new ApiResponse<CitySummaryDto>(result));
    }

    /// <summary>Lists chefs in a specific city and area.</summary>
    [HttpGet("locations/{city}/{area}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChefListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCityArea(
        string city,
        string area,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = new ChefQueryFilter
        {
            City = city,
            Area = area,
        };

        var result = await _chefService.ListAsync(filter, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ChefListItemDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }
}
