using HomeChef.Domain.Cuisines;

namespace HomeChef.Application.Features.Cuisines;

public interface ICuisineRepository
{
    Task<IReadOnlyList<Cuisine>> ListActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cuisine>> ListAllAsync(CancellationToken cancellationToken = default);
    Task<Cuisine?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Cuisine cuisine, CancellationToken cancellationToken = default);
    Task UpdateAsync(Cuisine cuisine, CancellationToken cancellationToken = default);
    Task DeleteAsync(Cuisine cuisine, CancellationToken cancellationToken = default);
}
