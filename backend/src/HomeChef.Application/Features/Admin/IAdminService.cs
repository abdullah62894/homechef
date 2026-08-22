using HomeChef.Application.Common;
using HomeChef.Application.Features.Admin.Contracts;

namespace HomeChef.Application.Features.Admin;

public interface IAdminService
{
    /// <summary>Lists accounts with roles and suspension state.</summary>
    Task<PagedResult<AdminUserDto>> ListUsersAsync(
        AdminUserQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Blocks sign-in for an account (Identity lockout, far future).</summary>
    Task<AdminUserDto> SuspendUserAsync(Guid adminUserId, Guid targetUserId, CancellationToken cancellationToken = default);

    /// <summary>Lifts a suspension.</summary>
    Task<AdminUserDto> RestoreUserAsync(Guid targetUserId, CancellationToken cancellationToken = default);

    /// <summary>Lists all reviews, newest first, for moderation.</summary>
    Task<PagedResult<AdminReviewDto>> ListReviewsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a review (moderation).</summary>
    Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);

    /// <summary>Removes a food item (moderation). Favorites cascade.</summary>
    Task DeleteFoodAsync(Guid foodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a chef profile with its foods, reviews, messages and favorites
    /// (cascade). The underlying user account is kept.
    /// </summary>
    Task DeleteChefProfileAsync(Guid chefProfileId, CancellationToken cancellationToken = default);
}
