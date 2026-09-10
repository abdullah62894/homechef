using HomeChef.Application.Features.Meals;
using HomeChef.Domain.Meals;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class MealCategoryRepository : IMealCategoryRepository
{
    private readonly HomeChefDbContext _db;

    public MealCategoryRepository(HomeChefDbContext db) => _db = db;

    public async Task<IReadOnlyList<MealCategory>> ListAsync(CancellationToken cancellationToken = default)
        => await _db.MealCategories.AsNoTracking().OrderBy(m => m.DisplayOrder).ToListAsync(cancellationToken);

    public async Task<MealCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.MealCategories.FindAsync([id], cancellationToken);
}
