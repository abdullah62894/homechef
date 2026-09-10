using HomeChef.Domain.Meals;

namespace HomeChef.Application.Features.Meals;

public interface IMealCategoryRepository
{
    Task<IReadOnlyList<MealCategory>> ListAsync(CancellationToken cancellationToken = default);
    Task<MealCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
