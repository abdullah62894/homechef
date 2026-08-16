using HomeChef.Application.Common;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Favorites.Contracts;
using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Foods.Contracts;
using HomeChef.Domain.Favorites;

namespace HomeChef.Application.Features.Favorites;

public sealed class FavoriteService : IFavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly IChefProfileRepository _chefRepository;
    private readonly IFoodRepository _foodRepository;

    public FavoriteService(
        IFavoriteRepository favoriteRepository,
        IChefProfileRepository chefRepository,
        IFoodRepository foodRepository)
    {
        _favoriteRepository = favoriteRepository;
        _chefRepository = chefRepository;
        _foodRepository = foodRepository;
    }

    public async Task<bool> AddChefFavoriteAsync(
        Guid userId,
        Guid chefProfileId,
        CancellationToken cancellationToken = default)
    {
        _ = await _chefRepository.GetByIdAsync(chefProfileId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        var existing = await _favoriteRepository.GetChefFavoriteAsync(userId, chefProfileId, cancellationToken);
        if (existing is not null)
        {
            return true; // Already favorited
        }

        var favorite = new FavoriteChef
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ChefProfileId = chefProfileId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _favoriteRepository.AddChefFavoriteAsync(favorite, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveChefFavoriteAsync(
        Guid userId,
        Guid chefProfileId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _favoriteRepository.GetChefFavoriteAsync(userId, chefProfileId, cancellationToken);
        if (existing is not null)
        {
            await _favoriteRepository.RemoveChefFavoriteAsync(existing, cancellationToken);
        }

        return true;
    }

    public async Task<PagedResult<ChefListItemDto>> ListFavoriteChefsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _favoriteRepository.ListFavoriteChefsAsync(userId, page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<ChefListItemDto>(
            items.Select(c => ChefService.ToListItem(c, null)).ToList(),
            page,
            pageSize,
            total,
            hasMore);
    }

    public async Task<bool> AddFoodFavoriteAsync(
        Guid userId,
        Guid foodItemId,
        CancellationToken cancellationToken = default)
    {
        _ = await _foodRepository.GetByIdAsync(foodItemId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.FoodItemNotFound, "Food item was not found.");

        var existing = await _favoriteRepository.GetFoodFavoriteAsync(userId, foodItemId, cancellationToken);
        if (existing is not null)
        {
            return true; // Already favorited
        }

        var favorite = new FavoriteFood
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FoodItemId = foodItemId,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _favoriteRepository.AddFoodFavoriteAsync(favorite, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveFoodFavoriteAsync(
        Guid userId,
        Guid foodItemId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _favoriteRepository.GetFoodFavoriteAsync(userId, foodItemId, cancellationToken);
        if (existing is not null)
        {
            await _favoriteRepository.RemoveFoodFavoriteAsync(existing, cancellationToken);
        }

        return true;
    }

    public async Task<PagedResult<FoodListItemDto>> ListFavoriteFoodsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _favoriteRepository.ListFavoriteFoodsAsync(userId, page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<FoodListItemDto>(
            items.Select(f => FoodService.ToListItem(f, null)).ToList(),
            page,
            pageSize,
            total,
            hasMore);
    }

    public async Task<UserFavoriteIdsDto> GetUserFavoriteIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var (chefIds, foodIds) = await _favoriteRepository.GetUserFavoriteIdsAsync(userId, cancellationToken);

        return new UserFavoriteIdsDto
        {
            ChefIds = chefIds,
            FoodIds = foodIds,
        };
    }
}
