using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Foods.Contracts;
using HomeChef.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/chefs")]
public sealed class ChefFoodsController : ControllerBase
{
    private readonly IFoodService _foodService;

    public ChefFoodsController(IFoodService foodService)
    {
        _foodService = foodService;
    }

    /// <summary>Lists public food/menu items offered by a specific chef.</summary>
    [HttpGet("{chefId:guid}/foods")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FoodListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListChefFoods(
        Guid chefId,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _foodService.ListChefFoodsAsync(chefId, page, pageSize, isAvailable, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<FoodListItemDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Lists all food items created by the calling chef (including unavailable).</summary>
    [HttpGet("me/foods")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FoodListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListMyFoods(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _foodService.ListMyFoodsAsync(userId, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<FoodListItemDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Creates a new food item for the calling chef.</summary>
    [HttpPost("me/foods")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<FoodItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateFood(
        [FromBody] CreateFoodItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var food = await _foodService.CreateChefFoodAsync(userId, request, cancellationToken);

        return Created($"/api/foods/{food.Id}", new ApiResponse<FoodItemDto>(food));
    }

    /// <summary>Updates an existing food item owned by the calling chef.</summary>
    [HttpPut("me/foods/{id:guid}")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<FoodItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFood(
        Guid id,
        [FromBody] UpdateFoodItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var food = await _foodService.UpdateChefFoodAsync(userId, id, request, cancellationToken);

        return Ok(new ApiResponse<FoodItemDto>(food));
    }

    /// <summary>Deletes a food item owned by the calling chef.</summary>
    [HttpDelete("me/foods/{id:guid}")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFood(Guid id, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _foodService.DeleteChefFoodAsync(userId, id, cancellationToken);

        return NoContent();
    }

    /// <summary>Toggles or sets the availability of a food item owned by the calling chef.</summary>
    [HttpPatch("me/foods/{id:guid}/availability")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<FoodItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvailability(
        Guid id,
        [FromBody] SetFoodAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var food = await _foodService.SetFoodAvailabilityAsync(userId, id, request.IsAvailable, cancellationToken);

        return Ok(new ApiResponse<FoodItemDto>(food));
    }
}
