using HomeChef.Application.Common;
using HomeChef.Application.Features.Reviews.Contracts;

namespace HomeChef.Application.Features.Reviews;

public interface IReviewService
{
    Task<PagedResult<ReviewDto>> ListChefReviewsAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ChefRatingSummaryDto> GetChefRatingSummaryAsync(
        Guid chefProfileId,
        CancellationToken cancellationToken = default);

    Task<ReviewDto> CreateChefReviewAsync(
        Guid customerUserId,
        Guid chefProfileId,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<ReviewDto> UpdateReviewAsync(
        Guid customerUserId,
        Guid reviewId,
        UpdateReviewRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteReviewAsync(
        Guid customerUserId,
        Guid reviewId,
        CancellationToken cancellationToken = default);
}
