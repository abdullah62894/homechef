using HomeChef.Application.Features.Chefs;
using HomeChef.Domain.Chefs;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class ChefAvailabilityRepository : IChefAvailabilityRepository
{
    private readonly HomeChefDbContext _db;

    public ChefAvailabilityRepository(HomeChefDbContext db) => _db = db;

    public async Task<IReadOnlyList<ChefAvailability>> ListByChefAsync(Guid chefProfileId, CancellationToken cancellationToken = default)
        => await _db.ChefAvailabilities.AsNoTracking()
            .Where(a => a.ChefProfileId == chefProfileId)
            .OrderBy(a => a.DayOfWeek).ThenBy(a => a.OpenTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ChefAvailability>> ListByChefAndDayAsync(Guid chefProfileId, int dayOfWeek, CancellationToken cancellationToken = default)
        => await _db.ChefAvailabilities.AsNoTracking()
            .Where(a => a.ChefProfileId == chefProfileId && a.DayOfWeek == dayOfWeek && a.IsActive)
            .OrderBy(a => a.OpenTime)
            .ToListAsync(cancellationToken);

    public async Task ReplaceAllAsync(Guid chefProfileId, List<ChefAvailability> windows, CancellationToken cancellationToken = default)
    {
        var existing = await _db.ChefAvailabilities
            .Where(a => a.ChefProfileId == chefProfileId)
            .ToListAsync(cancellationToken);

        _db.ChefAvailabilities.RemoveRange(existing);

        foreach (var w in windows)
        {
            w.Id = Guid.NewGuid();
            w.ChefProfileId = chefProfileId;
            w.CreatedAtUtc = DateTime.UtcNow;
            _db.ChefAvailabilities.Add(w);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
