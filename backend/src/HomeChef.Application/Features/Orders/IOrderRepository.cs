using HomeChef.Domain.Orders;

namespace HomeChef.Application.Features.Orders;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListByCustomerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListByChefAsync(Guid chefProfileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> ListAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
