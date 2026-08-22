using HomeChef.Application.Features.Reports.Contracts;
using HomeChef.Domain.Reports;

namespace HomeChef.Application.Features.Reports;

public interface IReportRepository
{
    Task AddAsync(ContentReport report, CancellationToken cancellationToken = default);

    Task<ContentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Reporter's still-open report for the same target, if any.</summary>
    Task<ContentReport?> FindOpenAsync(
        Guid reporterUserId,
        ReportTargetType targetType,
        Guid targetId,
        CancellationToken cancellationToken = default);

    Task<int> CountByReporterSinceAsync(
        Guid reporterUserId,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ReportDto> Items, int Total)> ListAsync(
        ReportStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(ContentReport report, CancellationToken cancellationToken = default);
}
