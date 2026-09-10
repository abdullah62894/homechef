using HomeChef.Api.Common;
using HomeChef.Application.Features.Messages;
using HomeChef.Domain.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/contact")]
public sealed class ContactController : ControllerBase
{
    private readonly IContactMessageService _contactMessageService;

    public ContactController(IContactMessageService contactMessageService) => _contactMessageService = contactMessageService;

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ContactMessage>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit([FromBody] SubmitContactRequest request, CancellationToken cancellationToken)
    {
        var message = await _contactMessageService.SubmitAsync(request.Name, request.Phone, request.Email, request.Message);
        return Created(string.Empty, new ApiResponse<ContactMessage>(message));
    }

    [HttpGet("admin")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ContactMessage>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return Ok(new ApiResponse<IReadOnlyList<ContactMessage>>(await _contactMessageService.ListAsync(page, pageSize)));
    }

    [HttpGet("admin/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ContactMessage>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var message = await _contactMessageService.GetByIdAsync(id);
        return message is null ? NotFound() : Ok(new ApiResponse<ContactMessage>(message));
    }

    [HttpPut("admin/{id:guid}/read")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _contactMessageService.MarkAsReadAsync(id);
        return NoContent();
    }

    [HttpPut("admin/{id:guid}/status")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateMessageStatusRequest request, CancellationToken cancellationToken)
    {
        await _contactMessageService.UpdateStatusAsync(id, request.Status);
        return NoContent();
    }

    [HttpGet("admin/unread-count")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        return Ok(new ApiResponse<int>(await _contactMessageService.GetUnreadCountAsync()));
    }
}

public record SubmitContactRequest(string Name, string Phone, string Email, string Message);
public record UpdateMessageStatusRequest(string Status);
