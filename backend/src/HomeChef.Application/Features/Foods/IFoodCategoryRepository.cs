using HomeChef.Domain.Foods;

namespace HomeChef.Application.Features.Foods;

public interface IFoodCategoryRepository
{
    Task<IReadOnlyList<FoodCategory>> ListAsync(CancellationToken cancellationToken = default);

    Task<FoodCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<FoodCategory?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
