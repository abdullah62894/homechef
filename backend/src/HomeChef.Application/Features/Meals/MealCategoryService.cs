using HomeChef.Domain.Meals;

namespace HomeChef.Application.Features.Meals;

public interface IMealCategoryService
{
    Task<IReadOnlyList<MealCategory>> ListAsync();
    Task<MealCategory?> GetByIdAsync(Guid id);
}

public sealed class MealCategoryService : IMealCategoryService
{
    private readonly IMealCategoryRepository _repository;

    public MealCategoryService(IMealCategoryRepository repository) => _repository = repository;

    public Task<IReadOnlyList<MealCategory>> ListAsync() => _repository.ListAsync();
    public Task<MealCategory?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);
}
