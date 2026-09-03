using System.Text.Json;
using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Admin;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;
using IPOForge.Infrastructure.DataProviders;
using IPOForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPOForge.Infrastructure.Services;

public class DataRefreshService : IDataRefreshService
{
    private readonly IpoForgeDbContext _context;
    private readonly PublicScraperDataProvider _publicScraper;
    private readonly IIpoScoringEngine _scoringEngine;
    private readonly IFinancialAnalysisEngine _financialEngine;
    private readonly IValuationEngine _valuationEngine;
    private readonly IGmpAnalyticsService _gmpAnalyticsService;
    private readonly IRiskEngine _riskEngine;
    private readonly ICacheService _cache;
    private readonly ILogger<DataRefreshService> _logger;

    public DataRefreshService(
        IpoForgeDbContext context,
        PublicScraperDataProvider publicScraper,
        IIpoScoringEngine scoringEngine,
        IFinancialAnalysisEngine financialEngine,
        IValuationEngine valuationEngine,
        IGmpAnalyticsService gmpAnalyticsService,
        IRiskEngine riskEngine,
        ICacheService cache,
        ILogger<DataRefreshService> logger)
    {
        _context = context;
        _publicScraper = publicScraper;
        _scoringEngine = scoringEngine;
        _financialEngine = financialEngine;
        _valuationEngine = valuationEngine;
        _gmpAnalyticsService = gmpAnalyticsService;
        _riskEngine = riskEngine;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DataRefreshStatusDto> RefreshMarketDataAsync(DataRefreshRequest request, CancellationToken cancellationToken = default)
    {
        var log = new DataRefreshLog
        {
            TriggerType = request.ForceFullSync ? "ManualAdminFullSync" : "ManualAdminIncremental",
            Status = "InProgress",
            StartedAt = DateTime.UtcNow
        };

        await _context.DataRefreshLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        int recordsProcessed = 0;

        try
        {
            _logger.LogInformation("Starting live real-time Indian IPO market data sync...");

            // 1. Fetch live real IPOs from public market aggregator
            var liveIpos = await _publicScraper.FetchRealLiveIposAsync(cancellationToken);

            var existingIpos = await _context.IPOs
                .Include(i => i.Company).ThenInclude(c => c.Financials)
                .Include(i => i.GmpHistories)
                .Include(i => i.SubscriptionHistories)
                .Include(i => i.Objectives)
                .Include(i => i.Risks)
                .Include(i => i.Scores)
                .ToListAsync(cancellationToken);

            var industryMetrics = await _context.IndustryMetrics.ToListAsync(cancellationToken);

            // 2. Ingest or update live IPOs
            foreach (var liveIpo in liveIpos)
            {
                var match = existingIpos.FirstOrDefault(e =>
                    e.Name.Equals(liveIpo.Name, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(e.Symbol) && e.Symbol.Equals(liveIpo.Symbol, StringComparison.OrdinalIgnoreCase)));

                if (match == null)
                {
                    await _context.Companies.AddAsync(liveIpo.Company, cancellationToken);
                    await _context.IPOs.AddAsync(liveIpo, cancellationToken);
                    existingIpos.Add(liveIpo);
                }
                else
                {
                    // Update latest market price & GMP
                    match.Status = liveIpo.Status;
                    match.PriceBandHigh = liveIpo.PriceBandHigh > 0 ? liveIpo.PriceBandHigh : match.PriceBandHigh;
                    match.PriceBandLow = liveIpo.PriceBandLow > 0 ? liveIpo.PriceBandLow : match.PriceBandLow;
                    match.UpdatedAt = DateTime.UtcNow;

                    var latestGmp = liveIpo.GmpHistories.LastOrDefault();
                    if (latestGmp != null)
                    {
                        match.GmpHistories.Add(new IPOGmpHistory
                        {
                            IpoId = match.Id,
                            GMP = latestGmp.GMP,
                            GMPPercentage = latestGmp.GMPPercentage,
                            EstimatedListingPrice = latestGmp.EstimatedListingPrice,
                            Source = "Live Real-Time Unofficial Aggregator",
                            ObservedAt = DateTime.UtcNow,
                            RetrievedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            // 3. Recalculate deterministic scores and valuations across all active & real IPOs
            var allIpos = await _context.IPOs
                .Include(i => i.Company).ThenInclude(c => c.Financials)
                .Include(i => i.GmpHistories)
                .Include(i => i.SubscriptionHistories)
                .Include(i => i.Objectives)
                .Include(i => i.Risks)
                .Include(i => i.Scores)
                .ToListAsync(cancellationToken);

            foreach (var ipo in allIpos)
            {
                recordsProcessed++;
                var ind = industryMetrics.FirstOrDefault(m => m.Sector == ipo.Company.Sector);
                var finReport = _financialEngine.AnalyzeFinancials(ipo.Company);
                var latestFin = ipo.Company.Financials.OrderBy(f => f.PeriodEnding).LastOrDefault();
                var valDto = _valuationEngine.EvaluateValuation(ipo, latestFin, ind);
                var gmpDto = _gmpAnalyticsService.AnalyzeGmpHistory(ipo);
                var subHistory = ipo.SubscriptionHistories.OrderBy(s => s.DayNumber).ToList();
                var latestSub = subHistory.LastOrDefault();

                var subBreakdown = new Contracts.Subscription.SubscriptionBreakdownDto
                {
                    IpoId = ipo.Id,
                    IpoName = ipo.Name,
                    LatestTotalSubscription = latestSub?.TotalSubscription ?? 0,
                    LatestQibSubscription = latestSub?.QibSubscription ?? 0,
                    LatestNiiSubscription = latestSub?.NiiSubscription ?? 0,
                    LatestRetailSubscription = latestSub?.RetailSubscription ?? 0
                };

                var scoreBreakdown = _scoringEngine.CalculateScore(ipo, finReport, valDto, gmpDto, subBreakdown, ipo.Risks.ToList());

                var existingScore = ipo.Scores.OrderByDescending(s => s.CalculatedAt).FirstOrDefault();
                if (existingScore != null)
                {
                    existingScore.ListingGainScore = scoreBreakdown.ListingGainScore;
                    existingScore.ListingRecommendation = scoreBreakdown.ListingRecommendation;
                    existingScore.ListingGainVerdict = scoreBreakdown.ListingGainVerdict;
                    existingScore.LongTermScore = scoreBreakdown.LongTermScore;
                    existingScore.LongTermRecommendation = scoreBreakdown.LongTermRecommendation;
                    existingScore.LongTermVerdict = scoreBreakdown.LongTermVerdict;
                    existingScore.BreakdownJson = JsonSerializer.Serialize(scoreBreakdown);
                    existingScore.CalculatedAt = DateTime.UtcNow;
                }
                else
                {
                    ipo.Scores.Add(new IPOScore
                    {
                        IpoId = ipo.Id,
                        ListingGainScore = scoreBreakdown.ListingGainScore,
                        ListingRecommendation = scoreBreakdown.ListingRecommendation,
                        ListingGainVerdict = scoreBreakdown.ListingGainVerdict,
                        LongTermScore = scoreBreakdown.LongTermScore,
                        LongTermRecommendation = scoreBreakdown.LongTermRecommendation,
                        LongTermVerdict = scoreBreakdown.LongTermVerdict,
                        BreakdownJson = JsonSerializer.Serialize(scoreBreakdown),
                        CalculatedAt = DateTime.UtcNow
                    });
                }
            }

            // Invalidate cache
            await _cache.RemoveAsync("dashboard_summary_cache_key", cancellationToken);

            log.Status = "Completed";
            log.CompletedAt = DateTime.UtcNow;
            log.RecordsProcessed = recordsProcessed;
            log.DetailsJson = JsonSerializer.Serialize(new { Message = $"Live sync successfully parsed {liveIpos.Count} real IPOs and recomputed metrics for {recordsProcessed} total entries." });

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Market data sync finished successfully. {RecordsProcessed} live records active.", recordsProcessed);

            return new DataRefreshStatusDto
            {
                LogId = log.Id,
                Status = log.Status,
                StartedAt = log.StartedAt,
                CompletedAt = log.CompletedAt,
                RecordsProcessed = recordsProcessed,
                Details = $"Successfully synchronized {liveIpos.Count} live real-time Indian IPOs with 19-pillar deterministic scoring."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during live data refresh.");
            log.Status = "Failed";
            log.CompletedAt = DateTime.UtcNow;
            log.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync(cancellationToken);

            return new DataRefreshStatusDto
            {
                LogId = log.Id,
                Status = "Failed",
                StartedAt = log.StartedAt,
                CompletedAt = log.CompletedAt,
                ErrorMessage = ex.Message,
                Details = "Live data sync encountered an error."
            };
        }
    }

    public async Task<IReadOnlyList<DataRefreshLog>> GetRefreshLogsAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.DataRefreshLogs
            .AsNoTracking()
            .OrderByDescending(l => l.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataSource>> GetDataSourcesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DataSources
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }
}
