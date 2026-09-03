using System.Text.Json;
using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Admin;
using IPOForge.Domain.Entities;
using IPOForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPOForge.Infrastructure.Services;

public class DataRefreshService : IDataRefreshService
{
    private readonly IpoForgeDbContext _context;
    private readonly IGmpDataProvider _gmpProvider;
    private readonly ISubscriptionDataProvider _subProvider;
    private readonly IIpoScoringEngine _scoringEngine;
    private readonly IFinancialAnalysisEngine _financialEngine;
    private readonly IValuationEngine _valuationEngine;
    private readonly IGmpAnalyticsService _gmpAnalyticsService;
    private readonly IRiskEngine _riskEngine;
    private readonly ICacheService _cache;
    private readonly ILogger<DataRefreshService> _logger;

    public DataRefreshService(
        IpoForgeDbContext context,
        IGmpDataProvider gmpProvider,
        ISubscriptionDataProvider subProvider,
        IIpoScoringEngine scoringEngine,
        IFinancialAnalysisEngine financialEngine,
        IValuationEngine valuationEngine,
        IGmpAnalyticsService gmpAnalyticsService,
        IRiskEngine riskEngine,
        ICacheService cache,
        ILogger<DataRefreshService> logger)
    {
        _context = context;
        _gmpProvider = gmpProvider;
        _subProvider = subProvider;
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
            _logger.LogInformation("Starting IPOForge market data refresh operation. Request: {@Request}", request);

            // Fetch live / public data feeds
            var latestGmps = await _gmpProvider.GetLatestGmpAsync(cancellationToken);
            var latestSubs = await _subProvider.GetLiveSubscriptionsAsync(cancellationToken);

            var ipos = await _context.IPOs
                .Include(i => i.Company).ThenInclude(c => c.Financials)
                .Include(i => i.GmpHistories)
                .Include(i => i.SubscriptionHistories)
                .Include(i => i.Objectives)
                .Include(i => i.Risks)
                .Include(i => i.Scores)
                .ToListAsync(cancellationToken);

            var industryMetrics = await _context.IndustryMetrics.ToListAsync(cancellationToken);

            foreach (var ipo in ipos)
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

            // Invalidate dashboard and list caches
            await _cache.RemoveAsync("dashboard_summary_cache_key", cancellationToken);

            log.Status = "Completed";
            log.CompletedAt = DateTime.UtcNow;
            log.RecordsProcessed = recordsProcessed;
            log.DetailsJson = JsonSerializer.Serialize(new { Message = $"Successfully synchronized {recordsProcessed} IPO entries and recomputed analytics." });

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Market data refresh completed successfully. {RecordsProcessed} records updated.", recordsProcessed);

            return new DataRefreshStatusDto
            {
                LogId = log.Id,
                Status = log.Status,
                StartedAt = log.StartedAt,
                CompletedAt = log.CompletedAt,
                RecordsProcessed = recordsProcessed,
                Details = "Successfully refreshed IPO market data, valuations, and deterministic scores."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data refresh.");
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
                Details = "Data refresh encountered an error."
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
