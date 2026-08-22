using HomeChef.Domain.Identity;

namespace HomeChef.Domain.Notifications;

/// <summary>What happened — drives the icon and copy in the UI.</summary>
public enum NotificationType
{
    NewMessage = 1,
    NewReview = 2,
}

/// <summary>
/// An in-app notification for a user (Stage 11). Created when a customer
/// contacts a chef or leaves a review.
/// </summary>
public class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Short body, already tailored for display.</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Null while unread.</summary>
    public DateTime? ReadAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
