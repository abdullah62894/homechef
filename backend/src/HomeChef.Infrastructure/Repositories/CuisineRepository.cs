using HomeChef.Application.Features.Cuisines;
using HomeChef.Domain.Cuisines;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class CuisineRepository : ICuisineRepository
{
    private readonly HomeChefDbContext _db;

    public CuisineRepository(HomeChefDbContext db) => _db = db;

    public async Task<IReadOnlyList<Cuisine>> ListActiveAsync(CancellationToken cancellationToken = default)
        => await _db.Cuisines.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.DisplayOrder).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Cuisine>> ListAllAsync(CancellationToken cancellationToken = default)
        => await _db.Cuisines.AsNoTracking().OrderBy(c => c.DisplayOrder).ToListAsync(cancellationToken);

    public async Task<Cuisine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.Cuisines.FindAsync([id], cancellationToken);

    public async Task AddAsync(Cuisine cuisine, CancellationToken cancellationToken = default)
    {
        _db.Cuisines.Add(cuisine);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Cuisine cuisine, CancellationToken cancellationToken = default)
    {
        _db.Cuisines.Update(cuisine);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Cuisine cuisine, CancellationToken cancellationToken = default)
    {
        _db.Cuisines.Remove(cuisine);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
