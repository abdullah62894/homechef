using HomeChef.Application.Features.Favorites;
using HomeChef.Domain.Chefs;
using HomeChef.Domain.Favorites;
using HomeChef.Domain.Foods;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class FavoriteRepository : IFavoriteRepository
{
    private readonly HomeChefDbContext _db;

    public FavoriteRepository(HomeChefDbContext db)
    {
        _db = db;
    }

    public async Task<FavoriteChef?> GetChefFavoriteAsync(
        Guid userId,
        Guid chefProfileId,
        CancellationToken cancellationToken = default)
    {
        return await _db.FavoriteChefs
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ChefProfileId == chefProfileId, cancellationToken);
    }

    public async Task AddChefFavoriteAsync(FavoriteChef favorite, CancellationToken cancellationToken = default)
    {
        _db.FavoriteChefs.Add(favorite);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveChefFavoriteAsync(FavoriteChef favorite, CancellationToken cancellationToken = default)
    {
        _db.FavoriteChefs.Remove(favorite);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<ChefProfile> Items, int Total)> ListFavoriteChefsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.FavoriteChefs
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAtUtc)
            .Select(f => f.ChefProfile!);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<FavoriteFood?> GetFoodFavoriteAsync(
        Guid userId,
        Guid foodItemId,
        CancellationToken cancellationToken = default)
    {
        return await _db.FavoriteFoods
            .FirstOrDefaultAsync(f => f.UserId == userId && f.FoodItemId == foodItemId, cancellationToken);
    }

    public async Task AddFoodFavoriteAsync(FavoriteFood favorite, CancellationToken cancellationToken = default)
    {
        _db.FavoriteFoods.Add(favorite);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFoodFavoriteAsync(FavoriteFood favorite, CancellationToken cancellationToken = default)
    {
        _db.FavoriteFoods.Remove(favorite);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<FoodItem> Items, int Total)> ListFavoriteFoodsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.FavoriteFoods
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Include(f => f.FoodItem)
                .ThenInclude(food => food!.ChefProfile)
            .Include(f => f.FoodItem)
                .ThenInclude(food => food!.Category)
            .OrderByDescending(f => f.CreatedAtUtc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => f.FoodItem!)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(IReadOnlyList<Guid> ChefIds, IReadOnlyList<Guid> FoodIds)> GetUserFavoriteIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var chefIds = await _db.FavoriteChefs
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => f.ChefProfileId)
            .ToListAsync(cancellationToken);

        var foodIds = await _db.FavoriteFoods
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .Select(f => f.FoodItemId)
            .ToListAsync(cancellationToken);

        return (chefIds, foodIds);
    }
}
