using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Reports;
using HomeChef.Application.Features.Reports.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

/// <summary>User-submitted content reports (Stage 10).</summary>
[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>Flags a kitchen, dish or review for admin moderation.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ReportDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var report = await _reportService.CreateAsync(userId, request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new ApiResponse<ReportDto>(report));
    }
}
