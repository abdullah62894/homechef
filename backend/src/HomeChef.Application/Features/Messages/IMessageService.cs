using HomeChef.Application.Common;
using HomeChef.Application.Features.Messages.Contracts;

namespace HomeChef.Application.Features.Messages;

public interface IMessageService
{
    /// <summary>Sends a contact message from the authenticated user to a chef.</summary>
    Task<ChefMessageDto> SendToChefAsync(Guid senderUserId, SendChefMessageRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lists messages received by the authenticated chef, newest first.</summary>
    Task<PagedResult<ChefMessageDto>> ListInboxAsync(Guid chefUserId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Lists messages the authenticated user has sent to chefs, newest first.</summary>
    Task<PagedResult<ChefMessageDto>> ListSentAsync(Guid senderUserId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Marks a message in the chef's inbox as read.</summary>
    Task MarkAsReadAsync(Guid chefUserId, Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>Returns the number of unread messages in the authenticated chef's inbox.</summary>
    Task<int> CountUnreadAsync(Guid chefUserId, CancellationToken cancellationToken = default);
}
