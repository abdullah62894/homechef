using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Images;
using HomeChef.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/chefs")]
public sealed class ChefsController : ControllerBase
{
    private readonly IChefService _chefService;
    private readonly IImageService _imageService;
    private readonly ILogger<ChefsController> _logger;

    public ChefsController(IChefService chefService, IImageService imageService, ILogger<ChefsController> logger)
    {
        _chefService = chefService;
        _imageService = imageService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChefListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] string? area = null,
        [FromQuery] string? cuisine = null,
        [FromQuery] double? lat = null,
        [FromQuery] double? lng = null,
        [FromQuery] double? radiusKm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var filter = new ChefQueryFilter
        {
            Search = search,
            City = city,
            Area = area,
            Cuisine = cuisine,
            Lat = lat,
            Lng = lng,
            RadiusKm = radiusKm,
        };

        var result = await _chefService.ListAsync(filter, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ChefListItemDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ChefProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(new ApiResponse<ChefProfileDto>(await _chefService.GetByIdAsync(id, cancellationToken)));
    }

    [HttpGet("me")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<ChefProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(new ApiResponse<ChefProfileDto>(await _chefService.GetMyProfileAsync(userId, cancellationToken)));
    }

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

    [HttpPost("me/photo")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<ChefProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhoto(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await using var stream = file.OpenReadStream();
            var image = await _imageService.UploadAsync(stream, file.Length, cancellationToken);
            var profile = await _chefService.SetMyPhotoAsync(userId, image, cancellationToken);
            return Ok(new ApiResponse<ChefProfileDto>(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload chef photo for user {UserId}", userId);
            throw;
        }
    }

    [HttpDelete("me/photo")]
    [Authorize(Policy = Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<ChefProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhoto(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(new ApiResponse<ChefProfileDto>(await _chefService.ClearMyPhotoAsync(userId, cancellationToken)));
    }
}
