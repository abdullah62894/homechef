using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Domain.Chefs;

namespace HomeChef.Application.Features.Chefs;

public sealed record ChefProfileWithDistance(ChefProfile Profile, double? DistanceKm);

public sealed record LocationChefCount(string City, string? Area, int Count);

/// <summary>Persistence access for chef profiles (implemented in Infrastructure).</summary>
public interface IChefProfileRepository
{
    Task<ChefProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ChefProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ChefProfileWithDistance> Items, int Total)> ListAsync(
        ChefQueryFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LocationChefCount>> GetLocationCountsAsync(CancellationToken cancellationToken = default);

    Task AddAsync(ChefProfile profile, CancellationToken cancellationToken = default);

    Task UpdateAsync(ChefProfile profile, CancellationToken cancellationToken = default);

    Task DeleteAsync(ChefProfile profile, CancellationToken cancellationToken = default);
}