using HomeChef.Application.Features.Reviews.Contracts;
using HomeChef.Domain.Reviews;

namespace HomeChef.Application.Features.Reviews;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Review?> GetByChefAndCustomerAsync(Guid chefProfileId, Guid customerUserId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Review> Items, int Total)> ListByChefIdAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ChefRatingSummaryDto> GetSummaryByChefIdAsync(Guid chefProfileId, CancellationToken cancellationToken = default);

    Task AddAsync(Review review, CancellationToken cancellationToken = default);

    Task UpdateAsync(Review review, CancellationToken cancellationToken = default);

    Task DeleteAsync(Review review, CancellationToken cancellationToken = default);
}
