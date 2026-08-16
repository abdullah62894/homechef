using HomeChef.Application.Features.Reviews;
using HomeChef.Application.Features.Reviews.Contracts;
using HomeChef.Domain.Reviews;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class ReviewRepository : IReviewRepository
{
    private readonly HomeChefDbContext _db;

    public ReviewRepository(HomeChefDbContext db)
    {
        _db = db;
    }

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Reviews
            .AsNoTracking()
            .Include(r => r.CustomerUser)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Review?> GetByChefAndCustomerAsync(
        Guid chefProfileId,
        Guid customerUserId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Reviews
            .AsNoTracking()
            .Include(r => r.CustomerUser)
            .FirstOrDefaultAsync(r => r.ChefProfileId == chefProfileId && r.CustomerUserId == customerUserId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Review> Items, int Total)> ListByChefIdAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Reviews
            .AsNoTracking()
            .Include(r => r.CustomerUser)
            .Where(r => r.ChefProfileId == chefProfileId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ThenBy(r => r.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<ChefRatingSummaryDto> GetSummaryByChefIdAsync(
        Guid chefProfileId,
        CancellationToken cancellationToken = default)
    {
        var ratings = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.ChefProfileId == chefProfileId)
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);

        var summary = new ChefRatingSummaryDto
        {
            ChefProfileId = chefProfileId,
            TotalReviews = ratings.Count,
            AverageRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : 0.0,
            RatingDistribution = new Dictionary<int, int>
            {
                [1] = ratings.Count(r => r == 1),
                [2] = ratings.Count(r => r == 2),
                [3] = ratings.Count(r => r == 3),
                [4] = ratings.Count(r => r == 4),
                [5] = ratings.Count(r => r == 5),
            },
        };

        return summary;
    }

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Review review, CancellationToken cancellationToken = default)
    {
        _db.Reviews.Update(review);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Review review, CancellationToken cancellationToken = default)
    {
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
