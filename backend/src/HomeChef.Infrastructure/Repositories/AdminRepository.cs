using HomeChef.Application.Features.Admin;
using HomeChef.Application.Features.Admin.Contracts;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class AdminRepository : IAdminRepository
{
    private readonly HomeChefDbContext _db;

    public AdminRepository(HomeChefDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<AdminUserDto> Items, int Total)> ListUsersAsync(
        AdminUserQuery query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var users = _db.Users.AsNoTracking();
        var profiles = await _db.ChefProfiles
            .AsNoTracking()
            .ToDictionaryAsync(p => p.UserId, p => p.Id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            users = users.Where(u =>
                EF.Functions.ILike(u.Email!, term) ||
                EF.Functions.ILike(u.FirstName, term) ||
                EF.Functions.ILike(u.LastName, term));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = query.Role.Trim();
            users = users.Where(u => _db.UserRoles
                .Any(ur => ur.UserId == u.Id &&
                           _db.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)));
        }

        var ordered = users.OrderByDescending(u => u.CreatedAtUtc).ThenBy(u => u.Id);

        var total = await ordered.CountAsync(cancellationToken);
        var pageUsers = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = pageUsers.Select(u => u.Id).ToList();
        var roleMap = await (
            from ur in _db.UserRoles
            join r in _db.Roles on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, RoleName = r.Name! })
            .ToListAsync(cancellationToken);

        var items = pageUsers.Select(u => new AdminUserDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Roles = roleMap.Where(x => x.UserId == u.Id).Select(x => x.RoleName).OrderBy(r => r).ToArray(),
            // Locked out until a date in the past (or none) is a transient failed-login
            // lockout; a future end date is an administrative suspension.
            IsSuspended = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
            ChefProfileId = profiles.GetValueOrDefault(u.Id),
            CreatedAtUtc = u.CreatedAtUtc,
        }).ToList();

        return (items, total);
    }

    public async Task<(IReadOnlyList<AdminReviewDto> Items, int Total)> ListReviewsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query =
            from review in _db.Reviews.AsNoTracking()
            join profile in _db.ChefProfiles.AsNoTracking()
                on review.ChefProfileId equals profile.Id
            join reviewer in _db.Users.AsNoTracking()
                on review.CustomerUserId equals reviewer.Id
            orderby review.CreatedAtUtc descending, review.Id
            select new AdminReviewDto
            {
                Id = review.Id,
                ChefProfileId = review.ChefProfileId,
                ChefDisplayName = profile.DisplayName,
                CustomerUserId = review.CustomerUserId,
                ReviewerName = reviewer.FirstName + " " + reviewer.LastName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAtUtc = review.CreatedAtUtc,
            };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
