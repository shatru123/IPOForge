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
            Id = Guid.NewGuid(),
            TriggerType = request.ForceFullSync ? "ManualAdminFullSync" : "ManualAdminIncremental",
            Status = "InProgress",
            StartedAt = DateTime.UtcNow
        };

        int recordsProcessed = 0;

        try
        {
            _logger.LogInformation("Starting live real-time Indian IPO market data sync...");

            // 1. Fetch live real IPOs and real listed performance from public market aggregators
            var liveIpos = await _publicScraper.FetchRealLiveIposAsync(cancellationToken);
            var listedIpos = await _publicScraper.FetchRealListedIposAsync(cancellationToken);
            var allScrapedIpos = liveIpos.Concat(listedIpos).ToList();

            var existingIpos = await _context.IPOs
                .Include(i => i.Company).ThenInclude(c => c.Financials)
                .Include(i => i.GmpHistories)
                .Include(i => i.SubscriptionHistories)
                .Include(i => i.Objectives)
                .Include(i => i.Risks)
                .Include(i => i.Scores)
                .ToListAsync(cancellationToken);

            var industryMetrics = await _context.IndustryMetrics.ToListAsync(cancellationToken);

            // 2. Ingest or update live and listed IPOs
            foreach (var liveIpo in allScrapedIpos)
            {
                var match = existingIpos.FirstOrDefault(e =>
                    e.Name.Equals(liveIpo.Name, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(e.Symbol) && e.Symbol.Equals(liveIpo.Symbol, StringComparison.OrdinalIgnoreCase)));

                if (match == null)
                {
                    await _context.IPOs.AddAsync(liveIpo, cancellationToken);
                    existingIpos.Add(liveIpo);
                }
                else
                {
                    // Update latest market price, lot metrics & company metadata
                    match.Status = liveIpo.Status;
                    match.PriceBandHigh = liveIpo.PriceBandHigh > 0 ? liveIpo.PriceBandHigh : match.PriceBandHigh;
                    match.PriceBandLow = liveIpo.PriceBandLow > 0 ? liveIpo.PriceBandLow : match.PriceBandLow;
                    match.LotSize = liveIpo.LotSize > 0 ? liveIpo.LotSize : match.LotSize;
                    match.IssueSize = liveIpo.IssueSize > 0 ? liveIpo.IssueSize : match.IssueSize;
                    match.FreshIssueAmount = liveIpo.FreshIssueAmount > 0 ? liveIpo.FreshIssueAmount : match.FreshIssueAmount;
                    match.OFSAmount = liveIpo.OFSAmount > 0 ? liveIpo.OFSAmount : match.OFSAmount;
                    match.OpenDate = liveIpo.OpenDate ?? match.OpenDate;
                    match.CloseDate = liveIpo.CloseDate ?? match.CloseDate;
                    match.AllotmentDate = liveIpo.AllotmentDate ?? match.AllotmentDate;
                    match.ListingDate = liveIpo.ListingDate ?? match.ListingDate;
                    if (liveIpo.Status == IpoStatus.Listed)
                    {
                        match.ListingPrice = liveIpo.ListingPrice ?? match.ListingPrice;
                        match.ListingGainPercent = liveIpo.ListingGainPercent ?? match.ListingGainPercent;
                        match.Day1ClosePrice = liveIpo.Day1ClosePrice ?? match.Day1ClosePrice;
                    }

                    if (match.Company != null && liveIpo.Company != null)
                    {
                        match.Company.Sector = !string.IsNullOrWhiteSpace(liveIpo.Company.Sector) ? liveIpo.Company.Sector : match.Company.Sector;
                        match.Company.Industry = !string.IsNullOrWhiteSpace(liveIpo.Company.Industry) ? liveIpo.Company.Industry : match.Company.Industry;
                        match.Company.PromoterHoldingPreIssue = liveIpo.Company.PromoterHoldingPreIssue > 0 ? liveIpo.Company.PromoterHoldingPreIssue : match.Company.PromoterHoldingPreIssue;
                        match.Company.PromoterHoldingPostIssue = liveIpo.Company.PromoterHoldingPostIssue > 0 ? liveIpo.Company.PromoterHoldingPostIssue : match.Company.PromoterHoldingPostIssue;

                        if (!match.Company.Financials.Any() && liveIpo.Company.Financials.Any())
                        {
                            foreach (var fin in liveIpo.Company.Financials)
                            {
                                var finEntity = new CompanyFinancial
                                {
                                    Id = Guid.NewGuid(),
                                    CompanyId = match.Company.Id,
                                    FiscalYear = fin.FiscalYear,
                                    PeriodEnding = fin.PeriodEnding,
                                    Revenue = fin.Revenue,
                                    EBITDA = fin.EBITDA,
                                    EBIT = fin.EBIT,
                                    PAT = fin.PAT,
                                    EPS = fin.EPS,
                                    TotalAssets = fin.TotalAssets,
                                    TotalDebt = fin.TotalDebt,
                                    NetWorth = fin.NetWorth,
                                    CurrentAssets = fin.CurrentAssets,
                                    CurrentLiabilities = fin.CurrentLiabilities,
                                    OperatingCashFlow = fin.OperatingCashFlow,
                                    FreeCashFlow = fin.FreeCashFlow,
                                    EbitdaMargin = fin.EbitdaMargin,
                                    PatMargin = fin.PatMargin,
                                    ROE = fin.ROE,
                                    DebtToEquity = fin.DebtToEquity,
                                    CurrentRatio = fin.CurrentRatio,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                match.Company.Financials.Add(finEntity);
                                _context.Entry(finEntity).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                            }
                        }
                    }

                    if (!match.SubscriptionHistories.Any() && liveIpo.SubscriptionHistories.Any())
                    {
                        foreach (var sub in liveIpo.SubscriptionHistories)
                        {
                            var subEntity = new IPOSubscriptionHistory
                            {
                                Id = Guid.NewGuid(),
                                IpoId = match.Id,
                                DayNumber = sub.DayNumber,
                                QibSubscription = sub.QibSubscription,
                                NiiSubscription = sub.NiiSubscription,
                                RetailSubscription = sub.RetailSubscription,
                                EmployeeSubscription = sub.EmployeeSubscription,
                                TotalSubscription = sub.TotalSubscription,
                                SnapshotDate = sub.SnapshotDate,
                                Source = sub.Source
                            };
                            match.SubscriptionHistories.Add(subEntity);
                            _context.Entry(subEntity).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                        }
                    }

                    match.UpdatedAt = DateTime.UtcNow;

                    var latestGmp = liveIpo.GmpHistories.LastOrDefault();
                    if (latestGmp != null)
                    {
                        var gmpHistory = new IPOGmpHistory
                        {
                            Id = Guid.NewGuid(),
                            IpoId = match.Id,
                            GMP = latestGmp.GMP,
                            GMPPercentage = latestGmp.GMPPercentage,
                            EstimatedListingPrice = latestGmp.EstimatedListingPrice,
                            Source = "Live Real-Time Unofficial Aggregator",
                            ObservedAt = DateTime.UtcNow,
                            RetrievedAt = DateTime.UtcNow
                        };
                        match.GmpHistories.Add(gmpHistory);
                        _context.Entry(gmpHistory).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                    }
                }
            }

            // 3. Recalculate deterministic scores and valuations across all active & real IPOs
            foreach (var ipo in existingIpos)
            {
                recordsProcessed++;
                var ind = industryMetrics.FirstOrDefault(m => m.Sector == ipo.Company?.Sector);
                var finReport = ipo.Company != null ? _financialEngine.AnalyzeFinancials(ipo.Company) : null;
                var latestFin = ipo.Company?.Financials?.OrderBy(f => f.PeriodEnding).LastOrDefault();
                var valDto = _valuationEngine.EvaluateValuation(ipo, latestFin, ind);
                var gmpDto = _gmpAnalyticsService.AnalyzeGmpHistory(ipo);
                var subHistory = ipo.SubscriptionHistories?.OrderBy(s => s.DayNumber).ToList() ?? new();
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

                var scoreBreakdown = _scoringEngine.CalculateScore(ipo, finReport, valDto, gmpDto, subBreakdown, ipo.Risks?.ToList() ?? new());

                var existingScore = ipo.Scores?.OrderByDescending(s => s.CalculatedAt).FirstOrDefault();
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
                    var newScore = new IPOScore
                    {
                        Id = Guid.NewGuid(),
                        IpoId = ipo.Id,
                        ListingGainScore = scoreBreakdown.ListingGainScore,
                        ListingRecommendation = scoreBreakdown.ListingRecommendation,
                        ListingGainVerdict = scoreBreakdown.ListingGainVerdict,
                        LongTermScore = scoreBreakdown.LongTermScore,
                        LongTermRecommendation = scoreBreakdown.LongTermRecommendation,
                        LongTermVerdict = scoreBreakdown.LongTermVerdict,
                        BreakdownJson = JsonSerializer.Serialize(scoreBreakdown),
                        CalculatedAt = DateTime.UtcNow
                    };
                    ipo.Scores ??= new List<IPOScore>();
                    ipo.Scores.Add(newScore);
                    _context.Entry(newScore).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                }
            }

            log.Status = "Completed";
            log.CompletedAt = DateTime.UtcNow;
            log.RecordsProcessed = recordsProcessed;
            log.DetailsJson = JsonSerializer.Serialize(new { Message = $"Live sync successfully parsed {liveIpos.Count} real IPOs and recomputed metrics for {recordsProcessed} total entries." });

            await _context.DataRefreshLogs.AddAsync(log, cancellationToken);
            await _cache.RemoveAsync("dashboard_summary_cache_key", cancellationToken);
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
            var err = ex.Message;
            if (ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException concEx)
            {
                var entries = string.Join("; ", concEx.Entries.Select(e => $"{e.Entity.GetType().Name} (Id: {e.Property("Id").CurrentValue}, State: {e.State}, ModProps: [{string.Join(", ", e.Properties.Where(p => p.IsModified).Select(p => $"{p.Metadata.Name}: {p.OriginalValue}->{p.CurrentValue}"))}])"));
                err = $"Concurrency Exception on entities: {entries}. Details: {ex.Message}";
            }
            _logger.LogError(ex, "Error during live data refresh: {ErrMsg}", err);
            log.Status = "Failed";
            log.CompletedAt = DateTime.UtcNow;
            log.ErrorMessage = err;
            try
            {
                await _context.DataRefreshLogs.AddAsync(log, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch { }

            return new DataRefreshStatusDto
            {
                LogId = log.Id,
                Status = "Failed",
                StartedAt = log.StartedAt,
                CompletedAt = log.CompletedAt,
                ErrorMessage = err,
                Details = ex.ToString()
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
