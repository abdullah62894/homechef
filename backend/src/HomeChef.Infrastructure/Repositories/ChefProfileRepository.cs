using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Chefs.Contracts;
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

    public async Task<(IReadOnlyList<ChefProfileWithDistance> Items, int Total)> ListAsync(
        ChefQueryFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ChefProfiles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim();
            query = query.Where(p => EF.Functions.ILike(p.City, city));
        }

        if (!string.IsNullOrWhiteSpace(filter.Area))
        {
            var area = filter.Area.Trim();
            query = query.Where(p => p.Area != null && EF.Functions.ILike(p.Area, area));
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = $"%{filter.Search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.DisplayName, term) ||
                EF.Functions.ILike(p.Bio, term) ||
                EF.Functions.ILike(p.City, term) ||
                (p.Area != null && EF.Functions.ILike(p.Area, term)));
        }

        var allMatching = await query.ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(filter.Cuisine))
        {
            var cuisine = filter.Cuisine.Trim();
            allMatching = allMatching
                .Where(p => p.Cuisines.Any(c => c.Equals(cuisine, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        List<ChefProfileWithDistance> withDistance;

        if (filter.Lat.HasValue && filter.Lng.HasValue)
        {
            var targetLat = filter.Lat.Value;
            var targetLng = filter.Lng.Value;

            withDistance = allMatching
                .Select(p =>
                {
                    double? dist = null;
                    if (p.Latitude.HasValue && p.Longitude.HasValue)
                    {
                        dist = Math.Round(CalculateDistanceKm(targetLat, targetLng, p.Latitude.Value, p.Longitude.Value), 2);
                    }
                    return new ChefProfileWithDistance(p, dist);
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
                .ThenBy(x => x.Profile.DisplayName)
                .ThenBy(x => x.Profile.Id)
                .ToList();
        }
        else
        {
            withDistance = allMatching
                .OrderBy(p => p.DisplayName)
                .ThenBy(p => p.Id)
                .Select(p => new ChefProfileWithDistance(p, null))
                .ToList();
        }

        var total = withDistance.Count;
        var pagedItems = withDistance
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (pagedItems, total);
    }

    public async Task<IReadOnlyList<LocationChefCount>> GetLocationCountsAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await _db.ChefProfiles
            .AsNoTracking()
            .Select(p => new { p.City, p.Area })
            .ToListAsync(cancellationToken);

        return profiles
            .GroupBy(p => new { City = p.City.Trim(), Area = string.IsNullOrWhiteSpace(p.Area) ? null : p.Area.Trim() })
            .Select(g => new LocationChefCount(g.Key.City, g.Key.Area, g.Count()))
            .OrderBy(l => l.City)
            .ThenBy(l => l.Area)
            .ToList();
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