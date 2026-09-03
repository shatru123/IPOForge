using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Ipo;
using IPOForge.Contracts.Watchlist;
using IPOForge.Domain.Entities;
using IPOForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IPOForge.Infrastructure.Services;

public class WatchlistService : IWatchlistService
{
    private readonly IpoForgeDbContext _context;
    private readonly IIpoService _ipoService;

    public WatchlistService(IpoForgeDbContext context, IIpoService ipoService)
    {
        _context = context;
        _ipoService = ipoService;
    }

    public async Task<IReadOnlyList<WatchlistItemDto>> GetUserWatchlistAsync(string userId, CancellationToken cancellationToken = default)
    {
        var items = await _context.Watchlists
            .AsNoTracking()
            .Include(w => w.IPO)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        var list = new List<WatchlistItemDto>();
        foreach (var item in items)
        {
            var detail = await _ipoService.GetIpoByIdAsync(item.IpoId, cancellationToken);
            if (detail != null)
            {
                list.Add(new WatchlistItemDto
                {
                    Id = item.Id,
                    IpoId = item.IpoId,
                    Ipo = detail,
                    Notes = item.Notes,
                    AlertOnGmpChange = item.AlertOnGmpChange,
                    AlertOnSubscriptionCross = item.AlertOnSubscriptionCross,
                    TargetGmpPercent = item.TargetGmpPercent,
                    AddedAt = item.CreatedAt
                });
            }
        }
        return list;
    }

    public async Task<WatchlistItemDto> AddToWatchlistAsync(string userId, AddToWatchlistRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Watchlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.IpoId == request.IpoId, cancellationToken);

        if (existing != null)
        {
            existing.Notes = request.Notes;
            existing.AlertOnGmpChange = request.AlertOnGmpChange;
            existing.AlertOnSubscriptionCross = request.AlertOnSubscriptionCross;
            existing.TargetGmpPercent = request.TargetGmpPercent;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            var ipoDetail = await _ipoService.GetIpoByIdAsync(request.IpoId, cancellationToken);
            return new WatchlistItemDto
            {
                Id = existing.Id,
                IpoId = existing.IpoId,
                Ipo = ipoDetail ?? new IpoSummaryDto(),
                Notes = existing.Notes,
                AlertOnGmpChange = existing.AlertOnGmpChange,
                AlertOnSubscriptionCross = existing.AlertOnSubscriptionCross,
                TargetGmpPercent = existing.TargetGmpPercent,
                AddedAt = existing.CreatedAt
            };
        }

        var newEntry = new Watchlist
        {
            UserId = userId,
            IpoId = request.IpoId,
            Notes = request.Notes,
            AlertOnGmpChange = request.AlertOnGmpChange,
            AlertOnSubscriptionCross = request.AlertOnSubscriptionCross,
            TargetGmpPercent = request.TargetGmpPercent
        };

        await _context.Watchlists.AddAsync(newEntry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var ipo = await _ipoService.GetIpoByIdAsync(request.IpoId, cancellationToken);
        return new WatchlistItemDto
        {
            Id = newEntry.Id,
            IpoId = newEntry.IpoId,
            Ipo = ipo ?? new IpoSummaryDto(),
            Notes = newEntry.Notes,
            AlertOnGmpChange = newEntry.AlertOnGmpChange,
            AlertOnSubscriptionCross = newEntry.AlertOnSubscriptionCross,
            TargetGmpPercent = newEntry.TargetGmpPercent,
            AddedAt = newEntry.CreatedAt
        };
    }

    public async Task<bool> RemoveFromWatchlistAsync(string userId, Guid ipoId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Watchlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.IpoId == ipoId, cancellationToken);

        if (existing == null) return false;

        _context.Watchlists.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateWatchlistItemAsync(string userId, Guid ipoId, UpdateWatchlistRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Watchlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.IpoId == ipoId, cancellationToken);

        if (existing == null) return false;

        existing.Notes = request.Notes;
        existing.AlertOnGmpChange = request.AlertOnGmpChange;
        existing.AlertOnSubscriptionCross = request.AlertOnSubscriptionCross;
        existing.TargetGmpPercent = request.TargetGmpPercent;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
