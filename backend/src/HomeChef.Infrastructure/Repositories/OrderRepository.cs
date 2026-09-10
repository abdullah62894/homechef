using HomeChef.Application.Features.Orders;
using HomeChef.Domain.Orders;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly HomeChefDbContext _db;

    public OrderRepository(HomeChefDbContext db) => _db = db;

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.ChefProfile)
            .Include(o => o.CustomerUser)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _db.Orders.AsNoTracking()
            .Include(o => o.Items).Include(o => o.ChefProfile)
            .Where(o => o.CustomerUserId == userId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByChefAsync(Guid chefProfileId, CancellationToken cancellationToken = default)
        => await _db.Orders.AsNoTracking()
            .Include(o => o.Items).Include(o => o.CustomerUser)
            .Where(o => o.ChefProfileId == chefProfileId)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> ListAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => await _db.Orders.AsNoTracking()
            .Include(o => o.Items).Include(o => o.ChefProfile).Include(o => o.CustomerUser)
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
