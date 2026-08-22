using HomeChef.Application.Common;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Foods;
using HomeChef.Application.Features.Reports.Contracts;
using HomeChef.Application.Features.Reviews;
using HomeChef.Domain.Reports;
using Microsoft.Extensions.Options;

namespace HomeChef.Application.Features.Reports;

public sealed class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IChefProfileRepository _chefProfileRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ContentGuard _contentGuard;
    private readonly ModerationOptions _options;

    public ReportService(
        IReportRepository reportRepository,
        IChefProfileRepository chefProfileRepository,
        IFoodRepository foodRepository,
        IReviewRepository reviewRepository,
        ContentGuard contentGuard,
        IOptions<ModerationOptions> options)
    {
        _reportRepository = reportRepository;
        _chefProfileRepository = chefProfileRepository;
        _foodRepository = foodRepository;
        _reviewRepository = reviewRepository;
        _contentGuard = contentGuard;
        _options = options.Value;
    }

    public async Task<ReportDto> CreateAsync(
        Guid reporterUserId,
        CreateReportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(request.Details))
        {
            _contentGuard.EnsureAllowed(request.Details.Trim());
        }

        var existing = await _reportRepository.FindOpenAsync(
            reporterUserId, request.TargetType, request.TargetId, cancellationToken);
        if (existing is not null)
        {
            throw new BusinessException(
                ErrorCodes.ReportDuplicate,
                "You already have an open report for this content.");
        }

        var since = DateTime.UtcNow.AddDays(-1);
        if (await _reportRepository.CountByReporterSinceAsync(reporterUserId, since, cancellationToken) >= _options.MaxReportsPerDay)
        {
            throw new BusinessException(
                ErrorCodes.ReportRateLimited,
                "You have submitted the maximum number of reports for today.");
        }

        var report = new ContentReport
        {
            Id = Guid.NewGuid(),
            ReporterUserId = reporterUserId,
            TargetType = request.TargetType,
            TargetChefProfileId = await ResolveChefProfileIdAsync(request, cancellationToken),
            TargetFoodItemId = request.TargetType == ReportTargetType.FoodItem ? request.TargetId : null,
            TargetReviewId = request.TargetType == ReportTargetType.Review ? request.TargetId : null,
            Reason = request.Reason,
            Details = request.Details?.Trim() ?? string.Empty,
            Status = ReportStatus.Open,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _reportRepository.AddAsync(report, cancellationToken);

        var created = await _reportRepository.GetByIdAsync(report.Id, cancellationToken);
        return ToDto(created ?? report, label: null);
    }

    public async Task<PagedResult<ReportDto>> ListAsync(
        ReportStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _reportRepository.ListAsync(status, page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<ReportDto>(items, page, pageSize, total, hasMore);
    }

    public async Task<ReportDto> ResolveAsync(
        Guid adminUserId,
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var report = await GetOpenOrThrowAsync(reportId, cancellationToken);
        report.Status = ReportStatus.Resolved;
        report.ResolvedAtUtc = DateTime.UtcNow;
        report.ResolvedByUserId = adminUserId;
        await _reportRepository.UpdateAsync(report, cancellationToken);

        var updated = await _reportRepository.GetByIdAsync(report.Id, cancellationToken);
        return ToDto(updated ?? report, label: null);
    }

    public async Task<ReportDto> DismissAsync(
        Guid adminUserId,
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var report = await GetOpenOrThrowAsync(reportId, cancellationToken);
        report.Status = ReportStatus.Dismissed;
        report.ResolvedAtUtc = DateTime.UtcNow;
        report.ResolvedByUserId = adminUserId;
        await _reportRepository.UpdateAsync(report, cancellationToken);

        var updated = await _reportRepository.GetByIdAsync(report.Id, cancellationToken);
        return ToDto(updated ?? report, label: null);
    }

    /// <summary>
    /// Validates the target exists and returns the owning chef profile id,
    /// which every target type can be traced back to.
    /// </summary>
    private async Task<Guid> ResolveChefProfileIdAsync(
        CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        switch (request.TargetType)
        {
            case ReportTargetType.ChefProfile:
                return (await _chefProfileRepository.GetByIdAsync(request.TargetId, cancellationToken)
                    ?? throw new BusinessException(ErrorCodes.ReportTargetInvalid, "Reported chef profile was not found.")).Id;

            case ReportTargetType.FoodItem:
                return (await _foodRepository.GetByIdAsync(request.TargetId, cancellationToken)
                    ?? throw new BusinessException(ErrorCodes.ReportTargetInvalid, "Reported food item was not found.")).ChefProfileId;

            case ReportTargetType.Review:
                return (await _reviewRepository.GetByIdAsync(request.TargetId, cancellationToken)
                    ?? throw new BusinessException(ErrorCodes.ReportTargetInvalid, "Reported review was not found.")).ChefProfileId;

            default:
                throw new BusinessException(ErrorCodes.ReportTargetInvalid, "Unknown report target type.");
        }
    }

    private async Task<ContentReport> GetOpenOrThrowAsync(Guid reportId, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(reportId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ReportNotFound, "Report was not found.");

        if (report.Status != ReportStatus.Open)
        {
            throw new BusinessException(ErrorCodes.ReportAlreadyHandled, "This report has already been handled.");
        }

        return report;
    }

    private static ReportDto ToDto(ContentReport report, string? label)
    {
        var reporter = report.Reporter;
        var reporterName = reporter is null
            ? "Unknown"
            : $"{reporter.FirstName} {reporter.LastName}".Trim();

        return new ReportDto
        {
            Id = report.Id,
            ReporterUserId = report.ReporterUserId,
            ReporterName = string.IsNullOrWhiteSpace(reporterName) ? "Anonymous" : reporterName,
            TargetType = report.TargetType,
            TargetId = report.TargetType switch
            {
                ReportTargetType.FoodItem => report.TargetFoodItemId ?? Guid.Empty,
                ReportTargetType.Review => report.TargetReviewId ?? Guid.Empty,
                _ => report.TargetChefProfileId,
            },
            TargetLabel = label ?? string.Empty,
            Reason = report.Reason,
            Details = report.Details,
            Status = report.Status,
            CreatedAtUtc = report.CreatedAtUtc,
            ResolvedAtUtc = report.ResolvedAtUtc,
        };
    }
}
