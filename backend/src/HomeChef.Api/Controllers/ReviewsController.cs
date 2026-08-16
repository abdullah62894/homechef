using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Reviews;
using HomeChef.Application.Features.Reviews.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>Lists public customer reviews for a specific chef.</summary>
    [HttpGet("api/chefs/{chefId:guid}/reviews")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ReviewDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListChefReviews(
        Guid chefId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _reviewService.ListChefReviewsAsync(chefId, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ReviewDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Gets rating summary and star distribution for a chef.</summary>
    [HttpGet("api/chefs/{chefId:guid}/reviews/summary")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ChefRatingSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChefRatingSummary(
        Guid chefId,
        CancellationToken cancellationToken = default)
    {
        var summary = await _reviewService.GetChefRatingSummaryAsync(chefId, cancellationToken);
        return Ok(new ApiResponse<ChefRatingSummaryDto>(summary));
    }

    /// <summary>Submits a review and rating for a chef (authenticated customers).</summary>
    [HttpPost("api/chefs/{chefId:guid}/reviews")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReview(
        Guid chefId,
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var review = await _reviewService.CreateChefReviewAsync(userId, chefId, request, cancellationToken);

        return Created(string.Empty, new ApiResponse<ReviewDto>(review));
    }

    /// <summary>Updates an existing review (review owner only).</summary>
    [HttpPut("api/reviews/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(
        Guid id,
        [FromBody] UpdateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var review = await _reviewService.UpdateReviewAsync(userId, id, request, cancellationToken);

        return Ok(new ApiResponse<ReviewDto>(review));
    }

    /// <summary>Deletes a review (review owner only).</summary>
    [HttpDelete("api/reviews/{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _reviewService.DeleteReviewAsync(userId, id, cancellationToken);

        return NoContent();
    }
}
