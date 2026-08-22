using HomeChef.Application.Common;
using HomeChef.Application.Features.Notifications.Contracts;
using HomeChef.Domain.Notifications;

namespace HomeChef.Application.Features.Notifications;

public interface INotificationService
{
    /// <summary>Records a notification for a user (called by other features).</summary>
    Task NotifyAsync(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        CancellationToken cancellationToken = default);

    /// <summary>Lists the caller's notifications, newest first.</summary>
    Task<PagedResult<NotificationDto>> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Marks one of the caller's notifications as read.</summary>
    Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>Marks all of the caller's notifications as read.</summary>
    Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
