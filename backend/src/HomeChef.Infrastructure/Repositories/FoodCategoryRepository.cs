using HomeChef.Application.Features.Foods;
using HomeChef.Domain.Foods;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class FoodCategoryRepository : IFoodCategoryRepository
{
    private readonly HomeChefDbContext _db;

    public FoodCategoryRepository(HomeChefDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<FoodCategory>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _db.FoodCategories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<FoodCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.FoodCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<FoodCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _db.FoodCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }
}
