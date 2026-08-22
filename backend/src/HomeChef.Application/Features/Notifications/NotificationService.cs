using HomeChef.Application.Common;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Notifications.Contracts;
using HomeChef.Domain.Notifications;

namespace HomeChef.Application.Features.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task NotifyAsync(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Body = body.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _repository.AddAsync(notification, cancellationToken);
    }

    public async Task<PagedResult<NotificationDto>> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _repository.ListByUserAsync(userId, page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<NotificationDto>(items, page, pageSize, total, hasMore);
    }

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _repository.CountUnreadAsync(userId, cancellationToken);
    }

    public async Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotificationNotFound, "Notification was not found.");

        if (notification.UserId != userId)
        {
            throw new BusinessException(ErrorCodes.NotificationForbidden, "This notification does not belong to you.");
        }

        if (notification.ReadAtUtc is null)
        {
            notification.ReadAtUtc = DateTime.UtcNow;
            await _repository.UpdateAsync(notification, cancellationToken);
        }
    }

    public Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _repository.MarkAllReadAsync(userId, DateTime.UtcNow, cancellationToken);
    }
}
