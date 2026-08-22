using HomeChef.Application.Features.Messages;
using HomeChef.Domain.Messages;
using HomeChef.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeChef.Infrastructure.Repositories;

public sealed class ChefMessageRepository : IMessageRepository
{
    private readonly HomeChefDbContext _db;

    public ChefMessageRepository(HomeChefDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ChefMessage message, CancellationToken cancellationToken = default)
    {
        _db.ChefMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChefMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.ChefMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Include(m => m.ChefProfile)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task UpdateAsync(ChefMessage message, CancellationToken cancellationToken = default)
    {
        _db.ChefMessages.Update(message);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<ChefMessage> Items, int Total)> ListInboxAsync(
        Guid chefProfileId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ChefMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ChefProfileId == chefProfileId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .ThenBy(m => m.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(IReadOnlyList<ChefMessage> Items, int Total)> ListSentByUserAsync(
        Guid senderUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.ChefMessages
            .AsNoTracking()
            .Include(m => m.ChefProfile)
            .Where(m => m.SenderUserId == senderUserId)
            .OrderByDescending(m => m.CreatedAtUtc)
            .ThenBy(m => m.Id);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<int> CountUnreadAsync(Guid chefProfileId, CancellationToken cancellationToken = default)
    {
        return await _db.ChefMessages
            .AsNoTracking()
            .CountAsync(m => m.ChefProfileId == chefProfileId && m.ReadAtUtc == null, cancellationToken);
    }

    public async Task<int> CountSentByUserSinceAsync(Guid senderUserId, DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        return await _db.ChefMessages
            .AsNoTracking()
            .CountAsync(m => m.SenderUserId == senderUserId && m.CreatedAtUtc >= sinceUtc, cancellationToken);
    }
}
