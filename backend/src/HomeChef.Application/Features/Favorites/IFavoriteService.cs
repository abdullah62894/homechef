using HomeChef.Application.Common;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Favorites.Contracts;
using HomeChef.Application.Features.Foods.Contracts;

namespace HomeChef.Application.Features.Favorites;

public interface IFavoriteService
{
    Task<bool> AddChefFavoriteAsync(Guid userId, Guid chefProfileId, CancellationToken cancellationToken = default);

    Task<bool> RemoveChefFavoriteAsync(Guid userId, Guid chefProfileId, CancellationToken cancellationToken = default);

    Task<PagedResult<ChefListItemDto>> ListFavoriteChefsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<bool> AddFoodFavoriteAsync(Guid userId, Guid foodItemId, CancellationToken cancellationToken = default);

    Task<bool> RemoveFoodFavoriteAsync(Guid userId, Guid foodItemId, CancellationToken cancellationToken = default);

    Task<PagedResult<FoodListItemDto>> ListFavoriteFoodsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<UserFavoriteIdsDto> GetUserFavoriteIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
