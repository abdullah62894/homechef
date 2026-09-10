using HomeChef.Api.Common;
using HomeChef.Application.Features.Cuisines;
using HomeChef.Application.Features.Images;
using HomeChef.Domain.Cuisines;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/cuisines")]
public sealed class CuisinesController : ControllerBase
{
    private readonly ICuisineService _cuisineService;
    private readonly IImageService _imageService;
    private readonly ILogger<CuisinesController> _logger;

    public CuisinesController(ICuisineService cuisineService, IImageService imageService, ILogger<CuisinesController> logger)
    {
        _cuisineService = cuisineService;
        _imageService = imageService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Cuisine>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListActive(CancellationToken cancellationToken)
        => Ok(new ApiResponse<IReadOnlyList<Cuisine>>(await _cuisineService.ListActiveAsync()));

    [HttpGet("admin")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<Cuisine>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAll(CancellationToken cancellationToken)
        => Ok(new ApiResponse<IReadOnlyList<Cuisine>>(await _cuisineService.ListAllAsync()));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<Cuisine>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cuisine = await _cuisineService.GetByIdAsync(id);
        return cuisine is null ? NotFound() : Ok(new ApiResponse<Cuisine>(cuisine));
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Cuisine>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCuisineRequest request, CancellationToken cancellationToken)
    {
        var cuisine = await _cuisineService.CreateAsync(request.Name, request.Slug, request.Description, request.DisplayOrder);
        return Created(string.Empty, new ApiResponse<Cuisine>(cuisine));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Cuisine>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCuisineRequest request, CancellationToken cancellationToken)
    {
        var cuisine = await _cuisineService.UpdateAsync(id, request.Name, request.Slug, request.Description, request.DisplayOrder, request.IsActive);
        return Ok(new ApiResponse<Cuisine>(cuisine));
    }

    [HttpPost("{id:guid}/image")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<Cuisine>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            var image = await _imageService.UploadAsync(stream, file.Length, cancellationToken);
            var cuisine = await _cuisineService.UpdateImageAsync(id, image.Url, image.ThumbnailUrl);
            return Ok(new ApiResponse<Cuisine>(cuisine));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload cuisine image {CuisineId}", id);
            throw;
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _cuisineService.DeleteAsync(id);
        return NoContent();
    }
}

public record CreateCuisineRequest(string Name, string Slug, string? Description, int DisplayOrder);
public record UpdateCuisineRequest(string Name, string Slug, string? Description, int DisplayOrder, bool IsActive);
