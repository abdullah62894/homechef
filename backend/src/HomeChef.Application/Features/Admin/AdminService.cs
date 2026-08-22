using HomeChef.Application.Common;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Admin.Contracts;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Reviews;
using HomeChef.Domain.Constants;
using HomeChef.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace HomeChef.Application.Features.Admin;

public sealed class AdminService : IAdminService
{
    private readonly IAdminRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFoodRepository _foodRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IChefProfileRepository _chefProfileRepository;

    public AdminService(
        IAdminRepository repository,
        UserManager<ApplicationUser> userManager,
        IFoodRepository foodRepository,
        IReviewRepository reviewRepository,
        IChefProfileRepository chefProfileRepository)
    {
        _repository = repository;
        _userManager = userManager;
        _foodRepository = foodRepository;
        _reviewRepository = reviewRepository;
        _chefProfileRepository = chefProfileRepository;
    }

    public async Task<PagedResult<AdminUserDto>> ListUsersAsync(
        AdminUserQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _repository.ListUsersAsync(query, page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<AdminUserDto>(items, page, pageSize, total, hasMore);
    }

    public async Task<AdminUserDto> SuspendUserAsync(
        Guid adminUserId,
        Guid targetUserId,
        CancellationToken cancellationToken = default)
    {
        var target = await FindUserOrThrowAsync(targetUserId);

        if (target.Id == adminUserId)
        {
            throw new BusinessException(ErrorCodes.AdminSelfSuspendForbidden, "You cannot suspend your own account.");
        }

        if (await _userManager.IsInRoleAsync(target, Roles.Admin))
        {
            throw new BusinessException(ErrorCodes.AdminSuspendAdminForbidden, "Administrators cannot be suspended.");
        }

        // A far-future lockout end reads as "suspended" until explicitly restored.
        await _userManager.SetLockoutEndDateAsync(target, DateTimeOffset.UtcNow.AddYears(100));
        await _userManager.ResetAccessFailedCountAsync(target);

        return await GetUserDtoAsync(target, cancellationToken);
    }

    public async Task<AdminUserDto> RestoreUserAsync(Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var target = await FindUserOrThrowAsync(targetUserId);

        await _userManager.SetLockoutEndDateAsync(target, null);
        await _userManager.ResetAccessFailedCountAsync(target);

        return await GetUserDtoAsync(target, cancellationToken);
    }

    public async Task<PagedResult<AdminReviewDto>> ListReviewsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _repository.ListReviewsAsync(page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<AdminReviewDto>(items, page, pageSize, total, hasMore);
    }

    public async Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ReviewNotFound, "Review was not found.");

        await _reviewRepository.DeleteAsync(review, cancellationToken);
    }

    public async Task DeleteFoodAsync(Guid foodId, CancellationToken cancellationToken = default)
    {
        var food = await _foodRepository.GetByIdAsync(foodId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.FoodItemNotFound, "Food item was not found.");

        await _foodRepository.DeleteAsync(food, cancellationToken);
    }

    public async Task DeleteChefProfileAsync(Guid chefProfileId, CancellationToken cancellationToken = default)
    {
        var profile = await _chefProfileRepository.GetByIdAsync(chefProfileId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        await _chefProfileRepository.DeleteAsync(profile, cancellationToken);
    }

    private async Task<ApplicationUser> FindUserOrThrowAsync(Guid userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new BusinessException(ErrorCodes.UserNotFound, "User was not found.");
    }

    private async Task<AdminUserDto> GetUserDtoAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var profile = await _chefProfileRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var isSuspended = await _userManager.IsLockedOutAsync(user);

        return ToDto(user, roles, isSuspended, profile?.Id);
    }

    private static AdminUserDto ToDto(ApplicationUser user, IList<string> roles, bool isSuspended, Guid? chefProfileId)
    {
        return new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = [.. roles],
            IsSuspended = isSuspended,
            ChefProfileId = chefProfileId,
            CreatedAtUtc = user.CreatedAtUtc,
        };
    }
}
