using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Domain.Cuisines;

namespace HomeChef.Application.Features.Cuisines;

public interface ICuisineService
{
    Task<IReadOnlyList<Cuisine>> ListActiveAsync();
    Task<IReadOnlyList<Cuisine>> ListAllAsync();
    Task<Cuisine?> GetByIdAsync(Guid id);
    Task<Cuisine> CreateAsync(string name, string slug, string? description, int displayOrder);
    Task<Cuisine> UpdateAsync(Guid id, string name, string slug, string? description, int displayOrder, bool isActive);
    Task DeleteAsync(Guid id);
    Task<Cuisine> UpdateImageAsync(Guid id, string imageUrl, string imageThumbnailUrl);
}

public sealed class CuisineService : ICuisineService
{
    private readonly ICuisineRepository _repository;

    public CuisineService(ICuisineRepository repository) => _repository = repository;

    public Task<IReadOnlyList<Cuisine>> ListActiveAsync() => _repository.ListActiveAsync();
    public Task<IReadOnlyList<Cuisine>> ListAllAsync() => _repository.ListAllAsync();
    public Task<Cuisine?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

    public async Task<Cuisine> CreateAsync(string name, string slug, string? description, int displayOrder)
    {
        var cuisine = new Cuisine
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Description = description?.Trim(),
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
        await _repository.AddAsync(cuisine);
        return cuisine;
    }

    public async Task<Cuisine> UpdateAsync(Guid id, string name, string slug, string? description, int displayOrder, bool isActive)
    {
        var cuisine = await _repository.GetByIdAsync(id)
            ?? throw new BusinessException(ErrorCodes.CuisineNotFound, "Cuisine not found.");

        cuisine.Name = name.Trim();
        cuisine.Slug = slug.Trim().ToLowerInvariant();
        cuisine.Description = description?.Trim();
        cuisine.DisplayOrder = displayOrder;
        cuisine.IsActive = isActive;

        await _repository.UpdateAsync(cuisine);
        return cuisine;
    }

    public async Task DeleteAsync(Guid id)
    {
        var cuisine = await _repository.GetByIdAsync(id)
            ?? throw new BusinessException(ErrorCodes.CuisineNotFound, "Cuisine not found.");
        await _repository.DeleteAsync(cuisine);
    }

    public async Task<Cuisine> UpdateImageAsync(Guid id, string imageUrl, string imageThumbnailUrl)
    {
        var cuisine = await _repository.GetByIdAsync(id)
            ?? throw new BusinessException(ErrorCodes.CuisineNotFound, "Cuisine not found.");

        cuisine.ImageUrl = imageUrl;
        cuisine.ImageThumbnailUrl = imageThumbnailUrl;
        await _repository.UpdateAsync(cuisine);
        return cuisine;
    }
}
