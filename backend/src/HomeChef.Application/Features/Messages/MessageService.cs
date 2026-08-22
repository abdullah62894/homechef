using HomeChef.Application.Common;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Messages.Contracts;
using HomeChef.Application.Features.Notifications;
using HomeChef.Application.Features.Reports;
using HomeChef.Domain.Messages;
using HomeChef.Domain.Notifications;
using Microsoft.Extensions.Options;

namespace HomeChef.Application.Features.Messages;

public sealed class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChefProfileRepository _chefRepository;
    private readonly ContentGuard _contentGuard;
    private readonly MessagingOptions _options;
    private readonly INotificationService _notificationService;

    public MessageService(
        IMessageRepository messageRepository,
        IChefProfileRepository chefRepository,
        ContentGuard contentGuard,
        IOptions<MessagingOptions> options,
        INotificationService notificationService)
    {
        _messageRepository = messageRepository;
        _chefRepository = chefRepository;
        _contentGuard = contentGuard;
        _options = options.Value;
        _notificationService = notificationService;
    }

    public async Task<ChefMessageDto> SendToChefAsync(
        Guid senderUserId,
        SendChefMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var chef = await _chefRepository.GetByIdAsync(request.ChefProfileId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        if (chef.UserId == senderUserId)
        {
            throw new BusinessException(ErrorCodes.SelfMessageForbidden, "You cannot contact your own kitchen.");
        }

        _contentGuard.EnsureAllowed(request.Body);

        var since = DateTime.UtcNow.AddDays(-1);
        if (await _messageRepository.CountSentByUserSinceAsync(senderUserId, since, cancellationToken) >= _options.MaxPerDay)
        {
            throw new BusinessException(
                ErrorCodes.MessageRateLimited,
                $"You have sent the maximum number of {_options.MaxPerDay} messages for today.");
        }

        var message = new ChefMessage
        {
            Id = Guid.NewGuid(),
            ChefProfileId = chef.Id,
            SenderUserId = senderUserId,
            Body = request.Body.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _messageRepository.AddAsync(message, cancellationToken);

        var preview = message.Body.Length <= 100 ? message.Body : message.Body[..100] + "…";
        await _notificationService.NotifyAsync(
            chef.UserId,
            NotificationType.NewMessage,
            "New message",
            preview,
            cancellationToken);

        return ToDto(message, chef.DisplayName);
    }

    public async Task<PagedResult<ChefMessageDto>> ListInboxAsync(
        Guid chefUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var chef = await RequireChefProfileAsync(chefUserId, cancellationToken);
        var (items, total) = await _messageRepository.ListInboxAsync(chef.Id, page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<ChefMessageDto>(
            items.Select(m => ToDto(m, chef.DisplayName)).ToList(),
            page,
            pageSize,
            total,
            hasMore);
    }

    public async Task<PagedResult<ChefMessageDto>> ListSentAsync(
        Guid senderUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _messageRepository.ListSentByUserAsync(senderUserId, page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<ChefMessageDto>(
            items.Select(m => ToDto(m)).ToList(),
            page,
            pageSize,
            total,
            hasMore);
    }

    public async Task MarkAsReadAsync(
        Guid chefUserId,
        Guid messageId,
        CancellationToken cancellationToken = default)
    {
        var chef = await RequireChefProfileAsync(chefUserId, cancellationToken);

        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.MessageNotFound, "Message was not found.");

        if (message.ChefProfileId != chef.Id)
        {
            throw new BusinessException(ErrorCodes.MessageForbidden, "This message is not in your inbox.");
        }

        if (message.ReadAtUtc is null)
        {
            message.ReadAtUtc = DateTime.UtcNow;
            await _messageRepository.UpdateAsync(message, cancellationToken);
        }
    }

    public async Task<int> CountUnreadAsync(
        Guid chefUserId,
        CancellationToken cancellationToken = default)
    {
        var chef = await RequireChefProfileAsync(chefUserId, cancellationToken);
        return await _messageRepository.CountUnreadAsync(chef.Id, cancellationToken);
    }

    private async Task<Domain.Chefs.ChefProfile> RequireChefProfileAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _chefRepository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileMissing, "A chef profile is required for this action.");
    }

    private static ChefMessageDto ToDto(ChefMessage message, string? chefDisplayName = null)
    {
        var sender = message.Sender;
        var senderName = sender is null
            ? "Unknown"
            : $"{sender.FirstName} {sender.LastName}".Trim();

        return new ChefMessageDto
        {
            Id = message.Id,
            ChefProfileId = message.ChefProfileId,
            ChefDisplayName = chefDisplayName ?? message.ChefProfile?.DisplayName ?? string.Empty,
            SenderUserId = message.SenderUserId,
            SenderName = senderName,
            Body = message.Body,
            ReadAtUtc = message.ReadAtUtc,
            CreatedAtUtc = message.CreatedAtUtc,
        };
    }
}
