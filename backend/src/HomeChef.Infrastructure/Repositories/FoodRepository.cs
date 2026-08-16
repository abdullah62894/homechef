using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Foods.Contracts;
using HomeChef.Domain.Foods;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class FoodRepository : IFoodRepository
{
    private readonly HomeChefDbContext _db;

    public FoodRepository(HomeChefDbContext db)
    {
        _db = db;
    }

    public async Task<FoodItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.FoodItems
            .AsNoTracking()
            .Include(f => f.ChefProfile)
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<FoodWithDistance> Items, int Total)> ListAsync(
        FoodQueryFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.FoodItems
            .AsNoTracking()
            .Include(f => f.ChefProfile)
            .Include(f => f.Category)
            .AsQueryable();

        if (filter.ChefId.HasValue)
        {
            query = query.Where(f => f.ChefProfileId == filter.ChefId.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(f => f.CategoryId == filter.CategoryId.Value);
        }

        if (filter.IsAvailable.HasValue)
        {
            query = query.Where(f => f.IsAvailable == filter.IsAvailable.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim();
            query = query.Where(f => EF.Functions.ILike(f.ChefProfile.City, city));
        }

        if (!string.IsNullOrWhiteSpace(filter.Area))
        {
            var area = filter.Area.Trim();
            query = query.Where(f => f.ChefProfile.Area != null && EF.Functions.ILike(f.ChefProfile.Area, area));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = $"%{filter.Search.Trim()}%";
            query = query.Where(f =>
                EF.Functions.ILike(f.Name, term) ||
                EF.Functions.ILike(f.Description, term) ||
                EF.Functions.ILike(f.ChefProfile.DisplayName, term) ||
                EF.Functions.ILike(f.ChefProfile.City, term) ||
                (f.ChefProfile.Area != null && EF.Functions.ILike(f.ChefProfile.Area, term)));
        }

        var allMatching = await query.ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(filter.Cuisine))
        {
            var cuisine = filter.Cuisine.Trim();
            allMatching = allMatching
                .Where(f => f.ChefProfile.Cuisines.Any(c => c.Equals(cuisine, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        List<FoodWithDistance> withDistance;

        if (filter.Lat.HasValue && filter.Lng.HasValue)
        {
            var targetLat = filter.Lat.Value;
            var targetLng = filter.Lng.Value;

            withDistance = allMatching
                .Select(f =>
                {
                    double? dist = null;
                    if (f.ChefProfile.Latitude.HasValue && f.ChefProfile.Longitude.HasValue)
                    {
                        dist = Math.Round(CalculateDistanceKm(targetLat, targetLng, f.ChefProfile.Latitude.Value, f.ChefProfile.Longitude.Value), 2);
                    }
                    return new FoodWithDistance(f, dist);
                })
                .Where(x =>
                {
                    if (filter.RadiusKm.HasValue)
                    {
                        return x.DistanceKm.HasValue && x.DistanceKm.Value <= filter.RadiusKm.Value;
                    }
                    return true;
                })
                .OrderBy(x => x.DistanceKm.HasValue ? 0 : 1)
                .ThenBy(x => x.DistanceKm)
                .ThenByDescending(x => x.Item.CreatedAtUtc)
                .ThenBy(x => x.Item.Id)
                .ToList();
        }
        else
        {
            withDistance = allMatching
                .OrderByDescending(f => f.CreatedAtUtc)
                .ThenBy(f => f.Id)
                .Select(f => new FoodWithDistance(f, null))
                .ToList();
        }

        var total = withDistance.Count;
        var pagedItems = withDistance
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedItems, total);
    }

    public async Task<(IReadOnlyList<FoodItem> Items, int Total)> ListByChefProfileIdAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        bool? isAvailable = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.FoodItems
            .AsNoTracking()
            .Include(f => f.ChefProfile)
            .Include(f => f.Category)
            .Where(f => f.ChefProfileId == chefProfileId);

        if (isAvailable.HasValue)
        {
            query = query.Where(f => f.IsAvailable == isAvailable.Value);
        }

        query = query.OrderByDescending(f => f.CreatedAtUtc).ThenBy(f => f.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(FoodItem item, CancellationToken cancellationToken = default)
    {
        _db.FoodItems.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FoodItem item, CancellationToken cancellationToken = default)
    {
        _db.FoodItems.Update(item);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(FoodItem item, CancellationToken cancellationToken = default)
    {
        _db.FoodItems.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        var rlat1 = Math.PI * lat1 / 180.0;
        var rlat2 = Math.PI * lat2 / 180.0;
        var theta = lon1 - lon2;
        var rtheta = Math.PI * theta / 180.0;
        var dist = Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) * Math.Cos(rlat2) * Math.Cos(rtheta);
        dist = Math.Acos(Math.Clamp(dist, -1.0, 1.0));
        dist = dist * 180.0 / Math.PI;
        dist = dist * 60.0 * 1.1515 * 1.609344;
        return dist;
    }
}
