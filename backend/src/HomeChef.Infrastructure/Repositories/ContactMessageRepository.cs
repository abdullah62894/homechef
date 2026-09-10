using HomeChef.Application.Features.Messages;
using HomeChef.Domain.Messages;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class ContactMessageRepository : IContactMessageRepository
{
    private readonly HomeChefDbContext _db;

    public ContactMessageRepository(HomeChefDbContext db) => _db = db;

    public async Task AddAsync(ContactMessage message, CancellationToken cancellationToken = default)
    {
        _db.ContactMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ContactMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _db.ContactMessages.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<ContactMessage>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => await _db.ContactMessages.AsNoTracking()
            .OrderByDescending(cm => cm.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
        => await _db.ContactMessages.CountAsync(cm => !cm.IsRead, cancellationToken);

    public async Task UpdateAsync(ContactMessage message, CancellationToken cancellationToken = default)
    {
        _db.ContactMessages.Update(message);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
