using System.Security.Claims;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Messages;
using HomeChef.Application.Features.Messages.Contracts;
using HomeChef.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public sealed class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    /// <summary>Sends a contact message from the authenticated user to a chef.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ChefMessageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Send(
        SendChefMessageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var message = await _messageService.SendToChefAsync(userId, request, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new ApiResponse<ChefMessageDto>(message));
    }

    /// <summary>Lists messages received by the authenticated chef (inbox), newest first.</summary>
    [HttpGet("inbox")]
    [Authorize(Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChefMessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Inbox(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _messageService.ListInboxAsync(userId, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ChefMessageDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Returns the authenticated chef's unread message count.</summary>
    [HttpGet("unread-count")]
    [Authorize(Policies.RequireChef)]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var count = await _messageService.CountUnreadAsync(userId, cancellationToken);

        return Ok(new ApiResponse<int>(count));
    }

    /// <summary>Lists messages the authenticated user has sent to chefs, newest first.</summary>
    [HttpGet("sent")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChefMessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Sent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _messageService.ListSentAsync(userId, page, pageSize, cancellationToken);

        return Ok(new ApiResponse<IReadOnlyList<ChefMessageDto>>(
            result.Items,
            new { result.Page, result.PageSize, result.Total, result.HasMore }));
    }

    /// <summary>Marks a message in the authenticated chef's inbox as read.</summary>
    [HttpPost("{messageId:guid}/read")]
    [Authorize(Policies.RequireChef)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _messageService.MarkAsReadAsync(userId, messageId, cancellationToken);

        return NoContent();
    }
}
