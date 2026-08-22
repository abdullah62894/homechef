using HomeChef.Application.Features.Notifications;
using HomeChef.Application.Features.Notifications.Contracts;
using HomeChef.Domain.Notifications;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly HomeChefDbContext _db;

    public NotificationRepository(HomeChefDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyList<NotificationDto> Items, int Total)> ListByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ThenBy(n => n.Id);

        var total = await query.CountAsync(cancellationToken);
        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type,
            Title = n.Title,
            Body = n.Body,
            ReadAtUtc = n.ReadAtUtc,
            CreatedAtUtc = n.CreatedAtUtc,
        }).ToList();

        return (items, total);
    }

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && n.ReadAtUtc == null, cancellationToken);
    }

    public async Task<int> MarkAllReadAsync(Guid userId, DateTime readAtUtc, CancellationToken cancellationToken = default)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId && n.ReadAtUtc == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(n => n.ReadAtUtc, readAtUtc),
                cancellationToken);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _db.Notifications.Update(notification);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
