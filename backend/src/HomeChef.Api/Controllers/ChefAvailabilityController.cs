using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs;
using HomeChef.Domain.Chefs;
using HomeChef.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/chefs/availability")]
public sealed class ChefAvailabilityController : ControllerBase
{
    private readonly IChefAvailabilityRepository _availabilityRepository;

    public ChefAvailabilityController(IChefAvailabilityRepository availabilityRepository)
        => _availabilityRepository = availabilityRepository;

    [HttpGet("me")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChefAvailability>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAvailability(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(new ApiResponse<IReadOnlyList<ChefAvailability>>(
            await _availabilityRepository.ListByChefAsync(userId, cancellationToken)));
    }

    [HttpGet("{chefId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChefAvailability>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChefAvailability(Guid chefId, CancellationToken cancellationToken)
    {
        return Ok(new ApiResponse<IReadOnlyList<ChefAvailability>>(
            await _availabilityRepository.ListByChefAsync(chefId, cancellationToken)));
    }

    [HttpPut("me")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMyAvailability(
        [FromBody] UpdateAvailabilityRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var windows = request.Windows.Select(w => new ChefAvailability
        {
            DayOfWeek = w.DayOfWeek,
            OpenTime = w.OpenTime,
            CloseTime = w.CloseTime,
            Label = w.Label,
            IsActive = w.IsActive,
        }).ToList();

        await _availabilityRepository.ReplaceAllAsync(userId, windows, cancellationToken);
        return NoContent();
    }
}

public record UpdateAvailabilityRequest(List<AvailabilityWindowRequest> Windows);
public record AvailabilityWindowRequest(int DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime, string? Label, bool IsActive);
