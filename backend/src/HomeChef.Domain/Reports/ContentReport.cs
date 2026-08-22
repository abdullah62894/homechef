using HomeChef.Domain.Identity;

namespace HomeChef.Domain.Reports;

/// <summary>What kind of content a report refers to.</summary>
public enum ReportTargetType
{
    ChefProfile = 1,
    FoodItem = 2,
    Review = 3,
}

/// <summary>Why the reporter flagged the content.</summary>
public enum ReportReason
{
    Spam = 1,
    AbusiveContent = 2,
    InappropriateImage = 3,
    Misleading = 4,
    Other = 5,
}

/// <summary>Moderation workflow state of a report.</summary>
public enum ReportStatus
{
    Open = 1,
    Resolved = 2,
    Dismissed = 3,
}

/// <summary>
/// A user-submitted flag on public content (kitchen, dish or review) for
/// admin moderation (Stage 10).
/// </summary>
public class ContentReport
{
    public Guid Id { get; set; }

    public Guid ReporterUserId { get; set; }

    public ApplicationUser? Reporter { get; set; }

    public ReportTargetType TargetType { get; set; }

    public Guid TargetChefProfileId { get; set; }

    public Guid? TargetFoodItemId { get; set; }

    public Guid? TargetReviewId { get; set; }

    public ReportReason Reason { get; set; }

    /// <summary>Free-text context from the reporter (optional).</summary>
    public string Details { get; set; } = string.Empty;

    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ResolvedAtUtc { get; set; }

    public Guid? ResolvedByUserId { get; set; }
}
