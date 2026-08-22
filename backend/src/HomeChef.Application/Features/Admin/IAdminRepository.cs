using HomeChef.Application.Common;
using HomeChef.Application.Features.Admin.Contracts;

namespace HomeChef.Application.Features.Admin;

public interface IAdminRepository
{
    Task<(IReadOnlyList<AdminUserDto> Items, int Total)> ListUsersAsync(
        AdminUserQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AdminReviewDto> Items, int Total)> ListReviewsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
