using HomeChef.Application.Features.Foods.Contracts;
using HomeChef.Domain.Foods;

namespace HomeChef.Application.Features.Foods;

public sealed record FoodWithDistance(FoodItem Item, double? DistanceKm);

public interface IFoodRepository
{
    Task<FoodItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<FoodWithDistance> Items, int Total)> ListAsync(
        FoodQueryFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<FoodItem> Items, int Total)> ListByChefProfileIdAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        bool? isAvailable = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(FoodItem item, CancellationToken cancellationToken = default);

    Task UpdateAsync(FoodItem item, CancellationToken cancellationToken = default);

    Task DeleteAsync(FoodItem item, CancellationToken cancellationToken = default);
}
