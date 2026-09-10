using HomeChef.Application.Features.Chefs;
using HomeChef.Domain.Chefs;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class ChefDeliveryMethodRepository : IChefDeliveryMethodRepository
{
    private readonly HomeChefDbContext _db;

    public ChefDeliveryMethodRepository(HomeChefDbContext db) => _db = db;

    public async Task<IReadOnlyList<ChefDeliveryMethod>> ListByChefAsync(Guid chefProfileId, CancellationToken cancellationToken = default)
        => await _db.ChefDeliveryMethods.AsNoTracking()
            .Where(dm => dm.ChefProfileId == chefProfileId)
            .ToListAsync(cancellationToken);

    public async Task ReplaceAllAsync(Guid chefProfileId, List<DeliveryMethodType> methods, CancellationToken cancellationToken = default)
    {
        var existing = await _db.ChefDeliveryMethods
            .Where(dm => dm.ChefProfileId == chefProfileId)
            .ToListAsync(cancellationToken);

        _db.ChefDeliveryMethods.RemoveRange(existing);

        foreach (var method in methods)
        {
            _db.ChefDeliveryMethods.Add(new ChefDeliveryMethod
            {
                Id = Guid.NewGuid(),
                ChefProfileId = chefProfileId,
                Method = method,
                CreatedAtUtc = DateTime.UtcNow,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
