using HomeChef.Application.Features.Chefs;
using HomeChef.Domain.Chefs;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class ChefProfileRepository : IChefProfileRepository
{
    private readonly HomeChefDbContext _db;

    public ChefProfileRepository(HomeChefDbContext db)
    {
        _db = db;
    }

    public async Task<ChefProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.ChefProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<ChefProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _db.ChefProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<(IReadOnlyList<ChefProfile> Items, int Total)> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ChefProfiles
            .AsNoTracking()
            .OrderBy(p => p.DisplayName)
            .ThenBy(p => p.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(ChefProfile profile, CancellationToken cancellationToken = default)
    {
        _db.ChefProfiles.Add(profile);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ChefProfile profile, CancellationToken cancellationToken = default)
    {
        _db.ChefProfiles.Update(profile);
        await _db.SaveChangesAsync(cancellationToken);
    }
}