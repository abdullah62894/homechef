using HomeChef.Domain.Messages;

namespace HomeChef.Application.Features.Messages;

public interface IMessageRepository
{
    Task AddAsync(ChefMessage message, CancellationToken cancellationToken = default);

    Task<ChefMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateAsync(ChefMessage message, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ChefMessage> Items, int Total)> ListInboxAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<ChefMessage> Items, int Total)> ListSentByUserAsync(
        Guid senderUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(Guid chefProfileId, CancellationToken cancellationToken = default);

    /// <summary>Messages sent by a user since the given instant (abuse rate limiting).</summary>
    Task<int> CountSentByUserSinceAsync(Guid senderUserId, DateTime sinceUtc, CancellationToken cancellationToken = default);
}
