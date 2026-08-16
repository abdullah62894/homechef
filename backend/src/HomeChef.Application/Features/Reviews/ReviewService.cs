using HomeChef.Application.Common;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Reviews.Contracts;
using HomeChef.Domain.Reviews;

namespace HomeChef.Application.Features.Reviews;

public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IChefProfileRepository _chefProfileRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IChefProfileRepository chefProfileRepository)
    {
        _reviewRepository = reviewRepository;
        _chefProfileRepository = chefProfileRepository;
    }

    public async Task<PagedResult<ReviewDto>> ListChefReviewsAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefProfileRepository.GetByIdAsync(chefProfileId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _reviewRepository.ListByChefIdAsync(chef.Id, page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<ReviewDto>(
            items.Select(ToDto).ToList(),
            page,
            pageSize,
            total,
            hasMore);
    }

    public async Task<ChefRatingSummaryDto> GetChefRatingSummaryAsync(
        Guid chefProfileId,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefProfileRepository.GetByIdAsync(chefProfileId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        return await _reviewRepository.GetSummaryByChefIdAsync(chef.Id, cancellationToken);
    }

    public async Task<ReviewDto> CreateChefReviewAsync(
        Guid customerUserId,
        Guid chefProfileId,
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefProfileRepository.GetByIdAsync(chefProfileId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        if (chef.UserId == customerUserId)
        {
            throw new BusinessException(ErrorCodes.SelfReviewForbidden, "Chefs cannot review their own kitchen.");
        }

        var existing = await _reviewRepository.GetByChefAndCustomerAsync(chefProfileId, customerUserId, cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException(ErrorCodes.DuplicateReview, "You have already reviewed this chef. You can edit your existing review.");
        }

        var now = DateTime.UtcNow;
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ChefProfileId = chef.Id,
            CustomerUserId = customerUserId,
            Rating = Math.Clamp(request.Rating, 1, 5),
            Comment = request.Comment.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await _reviewRepository.AddAsync(review, cancellationToken);

        var created = await _reviewRepository.GetByIdAsync(review.Id, cancellationToken);
        return ToDto(created ?? review);
    }

    public async Task<ReviewDto> UpdateReviewAsync(
        Guid customerUserId,
        Guid reviewId,
        UpdateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ReviewNotFound, "Review was not found.");

        if (review.CustomerUserId != customerUserId)
        {
            throw new BusinessException(ErrorCodes.ReviewForbidden, "You are not authorized to edit this review.");
        }

        review.Rating = Math.Clamp(request.Rating, 1, 5);
        review.Comment = request.Comment.Trim();
        review.UpdatedAtUtc = DateTime.UtcNow;

        await _reviewRepository.UpdateAsync(review, cancellationToken);

        var updated = await _reviewRepository.GetByIdAsync(review.Id, cancellationToken);
        return ToDto(updated ?? review);
    }

    public async Task DeleteReviewAsync(
        Guid customerUserId,
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ReviewNotFound, "Review was not found.");

        if (review.CustomerUserId != customerUserId)
        {
            throw new BusinessException(ErrorCodes.ReviewForbidden, "You are not authorized to delete this review.");
        }

        await _reviewRepository.DeleteAsync(review, cancellationToken);
    }

    private static ReviewDto ToDto(Review review)
    {
        var customerName = string.Empty;
        if (review.CustomerUser is not null)
        {
            customerName = $"{review.CustomerUser.FirstName} {review.CustomerUser.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(customerName))
            {
                customerName = review.CustomerUser.UserName ?? "Customer";
            }
        }

        return new ReviewDto
        {
            Id = review.Id,
            ChefProfileId = review.ChefProfileId,
            CustomerUserId = review.CustomerUserId,
            CustomerName = string.IsNullOrWhiteSpace(customerName) ? "Anonymous Customer" : customerName,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAtUtc = review.CreatedAtUtc,
            UpdatedAtUtc = review.UpdatedAtUtc,
        };
    }
}
