using HomeChef.Application.Common;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Foods.Contracts;
using HomeChef.Application.Features.Images.Contracts;
using HomeChef.Domain.Foods;

namespace HomeChef.Application.Features.Foods;

public sealed class FoodService : IFoodService
{
    private readonly IFoodRepository _foodRepository;
    private readonly IFoodCategoryRepository _categoryRepository;
    private readonly IChefProfileRepository _chefProfileRepository;

    public FoodService(
        IFoodRepository foodRepository,
        IFoodCategoryRepository categoryRepository,
        IChefProfileRepository chefProfileRepository)
    {
        _foodRepository = foodRepository;
        _categoryRepository = categoryRepository;
        _chefProfileRepository = chefProfileRepository;
    }

    public async Task<PagedResult<FoodListItemDto>> ListFoodsAsync(
        FoodQueryFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _foodRepository.ListAsync(filter, page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<FoodListItemDto>(
            items.Select(x => ToListItem(x.Item, x.DistanceKm)).ToList(),
            page,
            pageSize,
            total,
            hasMore);
    }

    public async Task<FoodItemDto> GetFoodByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var food = await _foodRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.FoodItemNotFound, "Food item was not found.");

        return ToDto(food);
    }

    public async Task<PagedResult<FoodListItemDto>> ListChefFoodsAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        bool? isAvailable = null,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefProfileRepository.GetByIdAsync(chefProfileId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _foodRepository.ListByChefProfileIdAsync(
            chef.Id, page, pageSize, isAvailable, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<FoodListItemDto>(
            items.Select(f => ToListItem(f, null)).ToList(),
            page,
            pageSize,
            total,
            hasMore);
    }

    public async Task<PagedResult<FoodListItemDto>> ListMyFoodsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _foodRepository.ListByChefProfileIdAsync(
            chef.Id, page, pageSize, null, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<FoodListItemDto>(
            items.Select(f => ToListItem(f, null)).ToList(),
            page,
            pageSize,
            total,
            hasMore);
    }

    public async Task<IReadOnlyList<FoodCategoryDto>> ListCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.ListAsync(cancellationToken);

        return categories.Select(c => new FoodCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Description = c.Description,
            DisplayOrder = c.DisplayOrder,
        }).ToList();
    }

    public async Task<FoodItemDto> CreateChefFoodAsync(
        Guid userId,
        CreateFoodItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileRequired, "A chef profile is required before adding food items.");

        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken)
                ?? throw new BusinessException(ErrorCodes.FoodCategoryNotFound, "Specified category was not found.");
        }

        var now = DateTime.UtcNow;
        var food = new FoodItem
        {
            Id = Guid.NewGuid(),
            ChefProfileId = chef.Id,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "PKR" : request.Currency.Trim().ToUpperInvariant(),
            IsAvailable = request.IsAvailable,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            PreparationTimeMinutes = request.PreparationTimeMinutes,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await _foodRepository.AddAsync(food, cancellationToken);

        var created = await _foodRepository.GetByIdAsync(food.Id, cancellationToken);
        return ToDto(created ?? food);
    }

    public async Task<FoodItemDto> UpdateChefFoodAsync(
        Guid userId,
        Guid foodId,
        UpdateFoodItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        var food = await _foodRepository.GetByIdAsync(foodId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.FoodItemNotFound, "Food item was not found.");

        if (food.ChefProfileId != chef.Id)
        {
            throw new BusinessException(ErrorCodes.FoodItemForbidden, "You are not authorized to modify this food item.");
        }

        if (request.CategoryId.HasValue)
        {
            _ = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken)
                ?? throw new BusinessException(ErrorCodes.FoodCategoryNotFound, "Specified category was not found.");
        }

        food.Name = request.Name.Trim();
        food.Description = request.Description.Trim();
        food.Price = request.Price;
        if (!string.IsNullOrWhiteSpace(request.Currency))
        {
            food.Currency = request.Currency.Trim().ToUpperInvariant();
        }
        food.CategoryId = request.CategoryId;
        food.IsAvailable = request.IsAvailable;
        food.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        food.PreparationTimeMinutes = request.PreparationTimeMinutes;
        food.UpdatedAtUtc = DateTime.UtcNow;

        await _foodRepository.UpdateAsync(food, cancellationToken);

        var updated = await _foodRepository.GetByIdAsync(food.Id, cancellationToken);
        return ToDto(updated ?? food);
    }

    public async Task DeleteChefFoodAsync(
        Guid userId,
        Guid foodId,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        var food = await _foodRepository.GetByIdAsync(foodId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.FoodItemNotFound, "Food item was not found.");

        if (food.ChefProfileId != chef.Id)
        {
            throw new BusinessException(ErrorCodes.FoodItemForbidden, "You are not authorized to delete this food item.");
        }

        await _foodRepository.DeleteAsync(food, cancellationToken);
    }

    public async Task<FoodItemDto> SetFoodAvailabilityAsync(
        Guid userId,
        Guid foodId,
        bool isAvailable,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        var food = await _foodRepository.GetByIdAsync(foodId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.FoodItemNotFound, "Food item was not found.");

        if (food.ChefProfileId != chef.Id)
        {
            throw new BusinessException(ErrorCodes.FoodItemForbidden, "You are not authorized to update this food item.");
        }

        food.IsAvailable = isAvailable;
        food.UpdatedAtUtc = DateTime.UtcNow;

        await _foodRepository.UpdateAsync(food, cancellationToken);

        var updated = await _foodRepository.GetByIdAsync(food.Id, cancellationToken);
        return ToDto(updated ?? food);
    }

    public async Task<FoodItemDto> SetFoodImageAsync(
        Guid userId,
        Guid foodId,
        ImageUploadResult image,
        CancellationToken cancellationToken = default)
    {
        var food = await GetOwnedFoodAsync(userId, foodId, cancellationToken);

        food.ImageUrl = image.Url;
        food.ImageThumbnailUrl = image.ThumbnailUrl;
        food.UpdatedAtUtc = DateTime.UtcNow;

        await _foodRepository.UpdateAsync(food, cancellationToken);

        var updated = await _foodRepository.GetByIdAsync(food.Id, cancellationToken);
        return ToDto(updated ?? food);
    }

    public async Task<FoodItemDto> ClearFoodImageAsync(Guid userId, Guid foodId, CancellationToken cancellationToken = default)
    {
        var food = await GetOwnedFoodAsync(userId, foodId, cancellationToken);

        food.ImageUrl = null;
        food.ImageThumbnailUrl = null;
        food.UpdatedAtUtc = DateTime.UtcNow;

        await _foodRepository.UpdateAsync(food, cancellationToken);

        var updated = await _foodRepository.GetByIdAsync(food.Id, cancellationToken);
        return ToDto(updated ?? food);
    }

    private async Task<FoodItem> GetOwnedFoodAsync(Guid userId, Guid foodId, CancellationToken cancellationToken)
    {
        var chef = await _chefProfileRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        var food = await _foodRepository.GetByIdAsync(foodId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.FoodItemNotFound, "Food item was not found.");

        if (food.ChefProfileId != chef.Id)
        {
            throw new BusinessException(ErrorCodes.FoodItemForbidden, "You are not authorized to modify this food item.");
        }

        return food;
    }

    public static FoodListItemDto ToListItem(FoodItem food, double? distanceKm = null)
    {
        return new FoodListItemDto
        {
            Id = food.Id,
            ChefProfileId = food.ChefProfileId,
            ChefDisplayName = food.ChefProfile?.DisplayName ?? string.Empty,
            ChefCity = food.ChefProfile?.City ?? string.Empty,
            ChefArea = food.ChefProfile?.Area,
            ChefAddress = food.ChefProfile?.Address,
            DistanceKm = distanceKm,
            CategoryId = food.CategoryId,
            CategoryName = food.Category?.Name,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            Currency = food.Currency,
            IsAvailable = food.IsAvailable,
            ImageUrl = food.ImageUrl,
            ImageThumbnailUrl = food.ImageThumbnailUrl,
            PreparationTimeMinutes = food.PreparationTimeMinutes,
        };
    }

    public static FoodItemDto ToDto(FoodItem food, double? distanceKm = null)
    {
        return new FoodItemDto
        {
            Id = food.Id,
            ChefProfileId = food.ChefProfileId,
            ChefDisplayName = food.ChefProfile?.DisplayName ?? string.Empty,
            ChefCity = food.ChefProfile?.City ?? string.Empty,
            ChefArea = food.ChefProfile?.Area,
            ChefAddress = food.ChefProfile?.Address,
            ChefLatitude = food.ChefProfile?.Latitude,
            ChefLongitude = food.ChefProfile?.Longitude,
            DistanceKm = distanceKm,
            CategoryId = food.CategoryId,
            CategoryName = food.Category?.Name,
            Name = food.Name,
            Description = food.Description,
            Price = food.Price,
            Currency = food.Currency,
            IsAvailable = food.IsAvailable,
            ImageUrl = food.ImageUrl,
            ImageThumbnailUrl = food.ImageThumbnailUrl,
            PreparationTimeMinutes = food.PreparationTimeMinutes,
            CreatedAtUtc = food.CreatedAtUtc,
            UpdatedAtUtc = food.UpdatedAtUtc,
        };
    }
}
