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

    public async Task<AdminUserDto> CreateUserAsync(
        CreateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var role = request.Role.Trim();
        if (!AdminAssignableRoles.Contains(role))
        {
            throw new BusinessException(ErrorCodes.InvalidRole, $"Role '{role}' cannot be assigned.");
        }

        var email = request.Email.Trim();
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new BusinessException(ErrorCodes.EmailTaken, "An account with this email already exists.");
        }

        var now = DateTime.UtcNow;
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            throw new BusinessException(
                ErrorCodes.PasswordRejected,
                string.Join(" ", created.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, role);

        return await GetUserDtoAsync(user, cancellationToken);
    }

    public async Task<AdminUserDto> UpdateUserAsync(
        Guid adminUserId,
        Guid targetUserId,
        UpdateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var target = await FindUserOrThrowAsync(targetUserId);

        target.FirstName = request.FirstName.Trim();
        target.LastName = request.LastName.Trim();
        target.UpdatedAtUtc = DateTime.UtcNow;
        var updated = await _userManager.UpdateAsync(target);
        if (!updated.Succeeded)
        {
            throw new BusinessException(
                ErrorCodes.PasswordRejected,
                string.Join(" ", updated.Errors.Select(e => e.Description)));
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var newRole = request.Role.Trim();
            if (!AdminAssignableRoles.Contains(newRole))
            {
                throw new BusinessException(ErrorCodes.InvalidRole, $"Role '{newRole}' cannot be assigned.");
            }

            if (targetUserId == adminUserId)
            {
                throw new BusinessException(ErrorCodes.AdminSelfRoleForbidden, "You cannot change your own role.");
            }

            var currentRoles = await _userManager.GetRolesAsync(target);
            if (!currentRoles.Contains(newRole))
            {
                // Demoting a chef removes their kitchen (and its content) with it.
                if (currentRoles.Contains(Roles.Chef) && newRole != Roles.Chef)
                {
                    var profile = await _chefProfileRepository.GetByUserIdAsync(targetUserId, cancellationToken);
                    if (profile is not null)
                    {
                        await _chefProfileRepository.DeleteAsync(profile, cancellationToken);
                    }
                }

                await _userManager.RemoveFromRolesAsync(target, currentRoles);
                await _userManager.AddToRoleAsync(target, newRole);
            }
        }

        return await GetUserDtoAsync(target, cancellationToken);
    }

    public async Task<AdminUserDto> SetPasswordAsync(
        Guid targetUserId,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var target = await FindUserOrThrowAsync(targetUserId);

        await _userManager.RemovePasswordAsync(target);
        var added = await _userManager.AddPasswordAsync(target, newPassword);
        if (!added.Succeeded)
        {
            throw new BusinessException(
                ErrorCodes.PasswordRejected,
                string.Join(" ", added.Errors.Select(e => e.Description)));
        }

        await _userManager.ResetAccessFailedCountAsync(target);

        return await GetUserDtoAsync(target, cancellationToken);
    }

    public async Task DeleteUserAsync(Guid adminUserId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        if (targetUserId == adminUserId)
        {
            throw new BusinessException(ErrorCodes.AdminSelfDeleteForbidden, "You cannot delete your own account.");
        }

        var target = await FindUserOrThrowAsync(targetUserId);

        // The chef profile (and its content) cascades with the account.
        var result = await _userManager.DeleteAsync(target);
        if (!result.Succeeded)
        {
            throw new BusinessException(
                ErrorCodes.PasswordRejected,
                string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }

    /// <summary>Roles an admin may grant: everything except the reserved Moderator role.</summary>
    private static readonly string[] AdminAssignableRoles = [Roles.Customer, Roles.Chef, Roles.Admin];

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

        return ToDto(user, roles, isSuspended, profile?.Id, profile?.ApprovalStatus.ToString());
    }

    private static AdminUserDto ToDto(ApplicationUser user, IList<string> roles, bool isSuspended, Guid? chefProfileId, string? chefApprovalStatus)
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
            ChefApprovalStatus = chefApprovalStatus,
            CreatedAtUtc = user.CreatedAtUtc,
        };
    }

    public async Task<AdminUserDto> ApproveChefAsync(Guid chefProfileId, CancellationToken cancellationToken = default)
    {
        var profile = await _chefProfileRepository.GetByIdAsync(chefProfileId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile not found.");

        profile.ApprovalStatus = Domain.Chefs.ChefApprovalStatus.Approved;
        profile.ApprovalAtUtc = DateTime.UtcNow;
        await _chefProfileRepository.UpdateAsync(profile, cancellationToken);

        var user = await _userManager.FindByIdAsync(profile.UserId.ToString())
            ?? throw new BusinessException(ErrorCodes.UserNotFound, "User not found.");

        return await GetUserDtoAsync(user, cancellationToken);
    }

    public async Task<AdminUserDto> RejectChefAsync(Guid chefProfileId, string? reason, CancellationToken cancellationToken = default)
    {
        var profile = await _chefProfileRepository.GetByIdAsync(chefProfileId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile not found.");

        profile.ApprovalStatus = Domain.Chefs.ChefApprovalStatus.Rejected;
        profile.RejectionReason = reason;
        await _chefProfileRepository.UpdateAsync(profile, cancellationToken);

        var user = await _userManager.FindByIdAsync(profile.UserId.ToString())
            ?? throw new BusinessException(ErrorCodes.UserNotFound, "User not found.");

        return await GetUserDtoAsync(user, cancellationToken);
    }
}
