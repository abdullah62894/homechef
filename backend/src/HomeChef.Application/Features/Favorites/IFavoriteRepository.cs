using HomeChef.Domain.Chefs;
using HomeChef.Domain.Favorites;
using HomeChef.Domain.Foods;

namespace HomeChef.Application.Features.Favorites;

public interface IFavoriteRepository
{
    Task<FavoriteChef?> GetChefFavoriteAsync(Guid userId, Guid chefProfileId, CancellationToken cancellationToken = default);

    Task AddChefFavoriteAsync(FavoriteChef favorite, CancellationToken cancellationToken = default);

    Task RemoveChefFavoriteAsync(FavoriteChef favorite, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ChefProfile> Items, int Total)> ListFavoriteChefsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<FavoriteFood?> GetFoodFavoriteAsync(Guid userId, Guid foodItemId, CancellationToken cancellationToken = default);

    Task AddFoodFavoriteAsync(FavoriteFood favorite, CancellationToken cancellationToken = default);

    Task RemoveFoodFavoriteAsync(FavoriteFood favorite, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<FoodItem> Items, int Total)> ListFavoriteFoodsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Guid> ChefIds, IReadOnlyList<Guid> FoodIds)> GetUserFavoriteIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
