using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/chefs")]
public sealed class ChefsController : ControllerBase
{
    private readonly IChefService _chefService;

    public ChefsController(IChefService chefService)
    {
        _chefService = chefService;
    }

    /// <summary>Lists public chef profiles (paginated).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChefListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _chefService.ListAsync(page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ChefListItemDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Returns a public chef profile.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ChefProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(new ApiResponse<ChefProfileDto>(await _chefService.GetByIdAsync(id, cancellationToken)));
    }

    /// <summary>Returns the calling chef's own profile.</summary>
    [HttpGet("me")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<ChefProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return Ok(new ApiResponse<ChefProfileDto>(await _chefService.GetMyProfileAsync(userId, cancellationToken)));
    }

    /// <summary>Creates the calling chef's profile.</summary>
    [HttpPost("me")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<ChefProfileDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateChefProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var profile = await _chefService.CreateAsync(userId, request, cancellationToken);

        return Created(string.Empty, new ApiResponse<ChefProfileDto>(profile));
    }

    /// <summary>Updates the calling chef's profile.</summary>
    [HttpPut("me")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<ChefProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromBody] UpdateChefProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return Ok(new ApiResponse<ChefProfileDto>(await _chefService.UpdateAsync(userId, request, cancellationToken)));
    }
}