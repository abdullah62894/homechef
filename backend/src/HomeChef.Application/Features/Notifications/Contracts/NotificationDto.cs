using HomeChef.Domain.Notifications;

namespace HomeChef.Application.Features.Notifications.Contracts;

public sealed class NotificationDto
{
    public required Guid Id { get; init; }

    public required NotificationType Type { get; init; }

    public required string Title { get; init; }

    public required string Body { get; init; }

    public DateTime? ReadAtUtc { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
