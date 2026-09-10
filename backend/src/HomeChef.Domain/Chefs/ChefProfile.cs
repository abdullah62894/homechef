namespace HomeChef.Domain.Chefs;

/// <summary>
/// Public profile of a home chef. One-to-one with an <c>ApplicationUser</c>
/// that holds the <c>Chef</c> role.
/// </summary>
public class ChefProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string? Area { get; set; }

    public string? Address { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    /// <summary>Normalized cuisine tags (e.g. "Pakistani", "Bakery").</summary>
    public string[] Cuisines { get; set; } = [];

    /// <summary>Approval status — new chefs start as PendingApproval.</summary>
    public ChefApprovalStatus ApprovalStatus { get; set; } = ChefApprovalStatus.PendingApproval;

    /// <summary>UTC timestamp when the chef was approved.</summary>
    public DateTime? ApprovalAtUtc { get; set; }

    /// <summary>Reason if rejected, or null.</summary>
    public string? RejectionReason { get; set; }

    /// <summary>Public phone number for contact (shown on click).</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>WhatsApp number for ordering (used in WhatsApp URL).</summary>
    public string? WhatsAppNumber { get; set; }

    /// <summary>Delivery radius in kilometers from the chef's coordinates.</summary>
    public double? DeliveryRadiusKm { get; set; }

    /// <summary>Optimized WebP photo served from image storage.</summary>
    public string? PhotoUrl { get; set; }

    /// <summary>Small WebP variant of <see cref="PhotoUrl"/> for lists.</summary>
    public string? PhotoThumbnailUrl { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}