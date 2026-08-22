using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Admin;
using HomeChef.Application.Features.Admin.Contracts;
using HomeChef.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

/// <summary>
/// Admin console endpoints — user management and content moderation (Stage 9).
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = Policies.RequireAdmin)]
public sealed class AdminsController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>Lists accounts with roles and suspension state.</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminUserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListUsers(
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.ListUsersAsync(new AdminUserQuery { Search = search, Role = role }, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<AdminUserDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Blocks sign-in for an account. Admins and self are protected.</summary>
    [HttpPost("users/{id:guid}/suspend")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendUser(Guid id, CancellationToken cancellationToken)
    {
        var adminUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _adminService.SuspendUserAsync(adminUserId, id, cancellationToken);

        return Ok(new ApiResponse<AdminUserDto>(user));
    }

    /// <summary>Lifts a suspension.</summary>
    [HttpPost("users/{id:guid}/restore")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestoreUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _adminService.RestoreUserAsync(id, cancellationToken);

        return Ok(new ApiResponse<AdminUserDto>(user));
    }

    /// <summary>Lists all reviews, newest first, for moderation.</summary>
    [HttpGet("reviews")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AdminReviewDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.ListReviewsAsync(page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<AdminReviewDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Removes a review (moderation).</summary>
    [HttpDelete("reviews/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteReviewAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Removes a food item (moderation). Favorites of it cascade.</summary>
    [HttpDelete("foods/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFood(Guid id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteFoodAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Removes a kitchen (chef profile) with all its content. The account is kept.</summary>
    [HttpDelete("chefs/{chefProfileId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteChefProfile(Guid chefProfileId, CancellationToken cancellationToken)
    {
        await _adminService.DeleteChefProfileAsync(chefProfileId, cancellationToken);
        return NoContent();
    }
}
