using HomeChef.Domain.Chefs;

namespace HomeChef.Application.Features.Chefs;

/// <summary>Persistence access for chef profiles (implemented in Infrastructure).</summary>
public interface IChefProfileRepository
{
    Task<ChefProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ChefProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ChefProfile> Items, int Total)> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(ChefProfile profile, CancellationToken cancellationToken = default);

    Task UpdateAsync(ChefProfile profile, CancellationToken cancellationToken = default);
}