using HomeChef.Domain.Foods;

namespace HomeChef.Application.Features.Foods;

public interface IFoodAvailabilityRepository
{
    Task<IReadOnlyList<FoodAvailability>> ListByFoodAsync(Guid foodItemId, CancellationToken cancellationToken = default);
    Task<List<FoodAvailability>> ListByFoodsAsync(List<Guid> foodItemIds, CancellationToken cancellationToken = default);
    Task ReplaceAllAsync(Guid foodItemId, List<FoodAvailability> schedules, CancellationToken cancellationToken = default);
}
