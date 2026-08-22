using HomeChef.Domain.Reports;

namespace HomeChef.Application.Features.Reports.Contracts;

public sealed class CreateReportRequest
{
    /// <summary>"ChefProfile", "FoodItem" or "Review".</summary>
    public ReportTargetType TargetType { get; set; }

    /// <summary>Id of the reported content.</summary>
    public Guid TargetId { get; set; }

    public ReportReason Reason { get; set; }

    /// <summary>Optional context for the moderators (max 1000 chars).</summary>
    public string? Details { get; set; }
}

public sealed class ReportDto
{
    public required Guid Id { get; init; }

    public required Guid ReporterUserId { get; init; }

    public required string ReporterName { get; init; }

    public required ReportTargetType TargetType { get; init; }

    public required Guid TargetId { get; init; }

    /// <summary>Display label of the reported content (e.g. dish name).</summary>
    public required string TargetLabel { get; init; }

    public required ReportReason Reason { get; init; }

    public required string Details { get; init; }

    public required ReportStatus Status { get; init; }

    public required DateTime CreatedAtUtc { get; init; }

    public DateTime? ResolvedAtUtc { get; init; }
}
