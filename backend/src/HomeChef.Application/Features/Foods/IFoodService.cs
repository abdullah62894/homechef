using HomeChef.Application.Common;
using HomeChef.Application.Features.Foods.Contracts;
using HomeChef.Application.Features.Images.Contracts;

namespace HomeChef.Application.Features.Foods;

public interface IFoodService
{
    Task<PagedResult<FoodListItemDto>> ListFoodsAsync(
        FoodQueryFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<FoodItemDto> GetFoodByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<FoodListItemDto>> ListChefFoodsAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        bool? isAvailable = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<FoodListItemDto>> ListMyFoodsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FoodCategoryDto>> ListCategoriesAsync(CancellationToken cancellationToken = default);

    Task<FoodItemDto> CreateChefFoodAsync(
        Guid userId,
        CreateFoodItemRequest request,
        CancellationToken cancellationToken = default);

    Task<FoodItemDto> UpdateChefFoodAsync(
        Guid userId,
        Guid foodId,
        UpdateFoodItemRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteChefFoodAsync(Guid userId, Guid foodId, CancellationToken cancellationToken = default);

    Task<FoodItemDto> SetFoodAvailabilityAsync(
        Guid userId,
        Guid foodId,
        bool isAvailable,
        CancellationToken cancellationToken = default);

    /// <summary>Sets the image of a food item owned by the calling chef.</summary>
    Task<FoodItemDto> SetFoodImageAsync(
        Guid userId,
        Guid foodId,
        ImageUploadResult image,
        CancellationToken cancellationToken = default);

    /// <summary>Removes the image of a food item owned by the calling chef.</summary>
    Task<FoodItemDto> ClearFoodImageAsync(Guid userId, Guid foodId, CancellationToken cancellationToken = default);
}
