using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Dashboard;
using IPOForge.Contracts.Gmp;
using IPOForge.Contracts.Ipo;
using IPOForge.Domain.Enums;
using IPOForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IPOForge.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IpoForgeDbContext _context;
    private readonly IIpoService _ipoService;
    private readonly IGmpAnalyticsService _gmpAnalyticsService;
    private readonly ICacheService _cache;

    private const string DashboardCacheKey = "dashboard_summary_cache_key";

    public DashboardService(
        IpoForgeDbContext context,
        IIpoService ipoService,
        IGmpAnalyticsService gmpAnalyticsService,
        ICacheService cache)
    {
        _context = context;
        _ipoService = ipoService;
        _gmpAnalyticsService = gmpAnalyticsService;
        _cache = cache;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetAsync<DashboardSummaryDto>(DashboardCacheKey, cancellationToken);
        if (cached != null) return cached;

        var allIpos = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.Company).ThenInclude(c => c.Financials)
            .Include(i => i.GmpHistories)
            .Include(i => i.SubscriptionHistories)
            .Include(i => i.Risks)
            .Include(i => i.Scores)
            .ToListAsync(cancellationToken);

        var upcoming = await _ipoService.GetUpcomingIposAsync(cancellationToken);
        var open = await _ipoService.GetOpenIposAsync(cancellationToken);
        var listed = await _ipoService.GetListedIposAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var closingToday = open.Where(i => i.CloseDate.HasValue && i.CloseDate.Value.Date == today).ToList();

        // Calculate Market Overview Metrics
        var thisYear = DateTime.UtcNow.Year;
        var iposThisYear = allIpos.Where(i => (i.OpenDate.HasValue && i.OpenDate.Value.Year == thisYear) || i.Status == IpoStatus.Listed).ToList();
        var mainboardCount = iposThisYear.Count(i => i.IpoType == IpoType.Mainboard);
        var smeCount = iposThisYear.Count(i => i.IpoType == IpoType.Sme);

        var gmpPercentages = allIpos
            .Select(i => i.GmpHistories.OrderBy(g => g.ObservedAt).LastOrDefault())
            .Where(g => g != null)
            .Select(g => g!.GMPPercentage)
            .ToList();
        var avgGmp = gmpPercentages.Any() ? Math.Round(gmpPercentages.Average(), 1) : 0;

        var subValues = allIpos
            .Where(i => i.Status != IpoStatus.Upcoming)
            .Select(i => i.SubscriptionHistories.OrderBy(s => s.DayNumber).LastOrDefault())
            .Where(s => s != null)
            .Select(s => s!.TotalSubscription)
            .ToList();
        var avgSub = subValues.Any() ? Math.Round(subValues.Average(), 1) : 0;

        var listedGains = allIpos
            .Where(i => i.Status == IpoStatus.Listed && i.ListingGainPercent.HasValue)
            .Select(i => i.ListingGainPercent!.Value)
            .ToList();
        var avgGain = listedGains.Any() ? Math.Round(listedGains.Average(), 1) : 0;

        // Trending GMP Gainers: Filter exclusively for Upcoming and Open (Current) IPOs only
        var activeUpcomingIpos = allIpos
            .Where(i => i.Status == IpoStatus.Upcoming || i.Status == IpoStatus.Open)
            .ToList();

        var movers = _gmpAnalyticsService.GetTopMovers(activeUpcomingIpos, 10);
        var topGainers = movers.OrderByDescending(m => m.CurrentGmpPercentage).Take(6).ToList();
        var topLosers = movers.Where(m => m.ChangeAmount < 0).OrderBy(m => m.ChangeAmount).Take(5).ToList();

        // High Potential & High Risk
        var allSummaries = (await _ipoService.GetIposAsync(new IpoFilterRequest { PageSize = 100 }, cancellationToken)).Items;
        var highPotentialListing = allSummaries.Where(s => (s.ListingGainScore ?? 0) >= 70).OrderByDescending(s => s.ListingGainScore).Take(4).ToList();
        var highPotentialLongTerm = allSummaries.Where(s => (s.LongTermScore ?? 0) >= 70).OrderByDescending(s => s.LongTermScore).Take(4).ToList();
        var highRisk = allSummaries.Where(s => s.HighRiskCount >= 2 || (s.ListingGainScore ?? 100) < 50).OrderByDescending(s => s.HighRiskCount).Take(4).ToList();

        var result = new DashboardSummaryDto
        {
            TotalIposThisYear = iposThisYear.Count > 0 ? iposThisYear.Count : allIpos.Count,
            MainboardIposCount = mainboardCount > 0 ? mainboardCount : allIpos.Count(i => i.IpoType == IpoType.Mainboard),
            SmeIposCount = smeCount > 0 ? smeCount : allIpos.Count(i => i.IpoType == IpoType.Sme),
            AverageGmpPercent = avgGmp,
            AverageSubscriptionX = avgSub,
            AverageListingGainPercent = avgGain,
            OpenIpos = open,
            UpcomingIpos = upcoming,
            ClosingTodayIpos = closingToday,
            RecentlyListedIpos = listed.Take(8).ToList(),
            TopGmpGainers = topGainers,
            TopGmpLosers = topLosers,
            HighPotentialListingIpos = highPotentialListing,
            HighPotentialLongTermIpos = highPotentialLongTerm,
            HighRiskIpos = highRisk
        };

        await _cache.SetAsync(DashboardCacheKey, result, TimeSpan.FromMinutes(2), cancellationToken);
        return result;
    }
}
