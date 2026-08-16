using HomeChef.Api.Common;
using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Foods.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/foods")]
public sealed class FoodsController : ControllerBase
{
    private readonly IFoodService _foodService;

    public FoodsController(IFoodService foodService)
    {
        _foodService = foodService;
    }

    /// <summary>Lists public food/menu items across all chefs (paginated, filterable).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FoodListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? chefId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] string? area = null,
        [FromQuery] string? cuisine = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lng = null,
        [FromQuery] double? radiusKm = null,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = new FoodQueryFilter
        {
            CategoryId = categoryId,
            ChefId = chefId,
            Search = search,
            City = city,
            Area = area,
            Cuisine = cuisine,
            Lat = lat,
            Lng = lng,
            RadiusKm = radiusKm,
            IsAvailable = isAvailable,
        };

        var result = await _foodService.ListFoodsAsync(filter, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<FoodListItemDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Returns a specific food item by its unique ID.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FoodItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var food = await _foodService.GetFoodByIdAsync(id, cancellationToken);
        return Ok(new ApiResponse<FoodItemDto>(food));
    }

    /// <summary>Lists all available food categories.</summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FoodCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCategories(CancellationToken cancellationToken)
    {
        var categories = await _foodService.ListCategoriesAsync(cancellationToken);
        return Ok(new ApiResponse<IReadOnlyList<FoodCategoryDto>>(categories));
    }
}
