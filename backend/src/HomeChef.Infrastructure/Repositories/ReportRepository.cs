using HomeChef.Application.Features.Reports;
using HomeChef.Application.Features.Reports.Contracts;
using HomeChef.Domain.Reports;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly HomeChefDbContext _db;

    public ReportRepository(HomeChefDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ContentReport report, CancellationToken cancellationToken = default)
    {
        _db.ContentReports.Add(report);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ContentReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.ContentReports
            .AsNoTracking()
            .Include(r => r.Reporter)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ContentReport?> FindOpenAsync(
        Guid reporterUserId,
        ReportTargetType targetType,
        Guid targetId,
        CancellationToken cancellationToken = default)
    {
        return await _db.ContentReports
            .AsNoTracking()
            .Where(r => r.ReporterUserId == reporterUserId && r.Status == ReportStatus.Open)
            .Where(r => targetType == ReportTargetType.ChefProfile
                ? r.TargetType == ReportTargetType.ChefProfile && r.TargetChefProfileId == targetId
                : targetType == ReportTargetType.FoodItem
                    ? r.TargetType == ReportTargetType.FoodItem && r.TargetFoodItemId == targetId
                    : r.TargetType == ReportTargetType.Review && r.TargetReviewId == targetId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> CountByReporterSinceAsync(
        Guid reporterUserId,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default)
    {
        return await _db.ContentReports
            .AsNoTracking()
            .CountAsync(r => r.ReporterUserId == reporterUserId && r.CreatedAtUtc >= sinceUtc, cancellationToken);
    }

    public async Task<(IReadOnlyList<ReportDto> Items, int Total)> ListAsync(
        ReportStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ContentReports.AsNoTracking();

        if (status.HasValue)
        {
            var s = status.Value;
            query = query.Where(r => r.Status == s);
        }

        // Labels for the moderation queue: kitchen name, dish name, or a
        // review snippet joined per target type.
        var chefs = await _db.ChefProfiles.AsNoTracking()
            .ToDictionaryAsync(p => p.Id, p => p.DisplayName, cancellationToken);
        var foods = await _db.FoodItems.AsNoTracking()
            .ToDictionaryAsync(f => f.Id, f => f.Name, cancellationToken);
        var reviewRows = await _db.Reviews.AsNoTracking()
            .Select(r => new { r.Id, r.Comment })
            .ToListAsync(cancellationToken);
        var reviewLabels = reviewRows.ToDictionary(
            x => x.Id,
            x => x.Comment.Length <= 80 ? x.Comment : x.Comment[..80] + "…");

        var ordered = query.OrderByDescending(r => r.CreatedAtUtc).ThenBy(r => r.Id);

        var total = await ordered.CountAsync(cancellationToken);
        var reports = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.Reporter)
            .ToListAsync(cancellationToken);

        var items = reports.Select(r => new ReportDto
        {
            Id = r.Id,
            ReporterUserId = r.ReporterUserId,
            ReporterName = r.Reporter is null
                ? "Unknown"
                : ($"{r.Reporter.FirstName} {r.Reporter.LastName}".Trim() is { Length: > 0 } name ? name : "Anonymous"),
            TargetType = r.TargetType,
            TargetId = r.TargetType switch
            {
                ReportTargetType.FoodItem => r.TargetFoodItemId ?? Guid.Empty,
                ReportTargetType.Review => r.TargetReviewId ?? Guid.Empty,
                _ => r.TargetChefProfileId,
            },
            TargetLabel = r.TargetType switch
            {
                ReportTargetType.ChefProfile => chefs.GetValueOrDefault(r.TargetChefProfileId, "removed kitchen"),
                ReportTargetType.FoodItem when r.TargetFoodItemId.HasValue => foods.GetValueOrDefault(r.TargetFoodItemId.Value, "removed dish"),
                ReportTargetType.Review when r.TargetReviewId.HasValue => reviewLabels.GetValueOrDefault(r.TargetReviewId.Value, "removed review"),
                _ => string.Empty,
            },
            Reason = r.Reason,
            Details = r.Details,
            Status = r.Status,
            CreatedAtUtc = r.CreatedAtUtc,
            ResolvedAtUtc = r.ResolvedAtUtc,
        }).ToList();

        return (items, total);
    }

    public async Task UpdateAsync(ContentReport report, CancellationToken cancellationToken = default)
    {
        _db.ContentReports.Update(report);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
