using HomeChef.Domain.Messages;

namespace HomeChef.Application.Features.Messages;

public interface IContactMessageRepository
{
    Task AddAsync(ContactMessage message, CancellationToken cancellationToken = default);
    Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContactMessage>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(ContactMessage message, CancellationToken cancellationToken = default);
}
