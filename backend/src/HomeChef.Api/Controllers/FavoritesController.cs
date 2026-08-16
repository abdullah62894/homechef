using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Favorites;
using HomeChef.Application.Features.Favorites.Contracts;
using HomeChef.Application.Features.Foods.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public sealed class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    /// <summary>Adds a chef to the authenticated user's favorites.</summary>
    [HttpPost("chefs/{chefId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddChefFavorite(
        Guid chefId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _favoriteService.AddChefFavoriteAsync(userId, chefId, cancellationToken);
        return NoContent();
    }

    /// <summary>Removes a chef from the authenticated user's favorites.</summary>
    [HttpDelete("chefs/{chefId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveChefFavorite(
        Guid chefId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _favoriteService.RemoveChefFavoriteAsync(userId, chefId, cancellationToken);
        return NoContent();
    }

    /// <summary>Lists all favorite chefs of the authenticated user.</summary>
    [HttpGet("chefs")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChefListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListFavoriteChefs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _favoriteService.ListFavoriteChefsAsync(userId, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ChefListItemDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Adds a food item to the authenticated user's favorites.</summary>
    [HttpPost("foods/{foodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddFoodFavorite(
        Guid foodId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _favoriteService.AddFoodFavoriteAsync(userId, foodId, cancellationToken);
        return NoContent();
    }

    /// <summary>Removes a food item from the authenticated user's favorites.</summary>
    [HttpDelete("foods/{foodId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveFoodFavorite(
        Guid foodId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _favoriteService.RemoveFoodFavoriteAsync(userId, foodId, cancellationToken);
        return NoContent();
    }

    /// <summary>Lists all favorite foods of the authenticated user.</summary>
    [HttpGet("foods")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FoodListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListFavoriteFoods(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _favoriteService.ListFavoriteFoodsAsync(userId, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<FoodListItemDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Returns the complete set of favorited chef and food IDs for the authenticated user.</summary>
    [HttpGet("ids")]
    [ProducesResponseType(typeof(ApiResponse<UserFavoriteIdsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserFavoriteIds(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ids = await _favoriteService.GetUserFavoriteIdsAsync(userId, cancellationToken);
        return Ok(new ApiResponse<UserFavoriteIdsDto>(ids));
    }
}
