using HomeChef.Application.Features.Foods;
using HomeChef.Domain.Foods;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class FoodAvailabilityRepository : IFoodAvailabilityRepository
{
    private readonly HomeChefDbContext _db;

    public FoodAvailabilityRepository(HomeChefDbContext db) => _db = db;

    public async Task<IReadOnlyList<FoodAvailability>> ListByFoodAsync(Guid foodItemId, CancellationToken cancellationToken = default)
        => await _db.FoodAvailabilities.AsNoTracking()
            .Where(fa => fa.FoodItemId == foodItemId)
            .ToListAsync(cancellationToken);

    public async Task<List<FoodAvailability>> ListByFoodsAsync(List<Guid> foodItemIds, CancellationToken cancellationToken = default)
        => await _db.FoodAvailabilities.AsNoTracking()
            .Where(fa => foodItemIds.Contains(fa.FoodItemId))
            .ToListAsync(cancellationToken);

    public async Task ReplaceAllAsync(Guid foodItemId, List<FoodAvailability> schedules, CancellationToken cancellationToken = default)
    {
        var existing = await _db.FoodAvailabilities
            .Where(fa => fa.FoodItemId == foodItemId)
            .ToListAsync(cancellationToken);

        _db.FoodAvailabilities.RemoveRange(existing);

        foreach (var s in schedules)
        {
            s.Id = Guid.NewGuid();
            s.FoodItemId = foodItemId;
            s.CreatedAtUtc = DateTime.UtcNow;
            s.UpdatedAtUtc = DateTime.UtcNow;
            _db.FoodAvailabilities.Add(s);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
