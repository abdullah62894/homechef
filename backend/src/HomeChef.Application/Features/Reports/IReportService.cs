using HomeChef.Application.Common;
using HomeChef.Application.Features.Reports.Contracts;
using HomeChef.Domain.Reports;

namespace HomeChef.Application.Features.Reports;

public interface IReportService
{
    /// <summary>Submits a report on public content (any authenticated user).</summary>
    Task<ReportDto> CreateAsync(
        Guid reporterUserId,
        CreateReportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Lists reports for the admin console (newest first).</summary>
    Task<PagedResult<ReportDto>> ListAsync(
        ReportStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Marks a report as resolved (action taken).</summary>
    Task<ReportDto> ResolveAsync(Guid adminUserId, Guid reportId, CancellationToken cancellationToken = default);

    /// <summary>Marks a report as dismissed (no action needed).</summary>
    Task<ReportDto> DismissAsync(Guid adminUserId, Guid reportId, CancellationToken cancellationToken = default);
}
