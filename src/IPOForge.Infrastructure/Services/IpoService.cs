using System.Text.Json;
using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Analysis;
using IPOForge.Contracts.Common;
using IPOForge.Contracts.Company;
using IPOForge.Contracts.Financial;
using IPOForge.Contracts.Gmp;
using IPOForge.Contracts.Ipo;
using IPOForge.Contracts.Subscription;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;
using IPOForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IPOForge.Infrastructure.Services;

public class IpoService : IIpoService
{
    private readonly IpoForgeDbContext _context;
    private readonly IIpoScoringEngine _scoringEngine;
    private readonly IFinancialAnalysisEngine _financialEngine;
    private readonly IValuationEngine _valuationEngine;
    private readonly IGmpAnalyticsService _gmpAnalyticsService;
    private readonly IRiskEngine _riskEngine;
    private readonly IFundUtilizationService _fundUtilizationService;
    private readonly ICacheService _cache;

    public IpoService(
        IpoForgeDbContext context,
        IIpoScoringEngine scoringEngine,
        IFinancialAnalysisEngine financialEngine,
        IValuationEngine valuationEngine,
        IGmpAnalyticsService gmpAnalyticsService,
        IRiskEngine riskEngine,
        IFundUtilizationService fundUtilizationService,
        ICacheService cache)
    {
        _context = context;
        _scoringEngine = scoringEngine;
        _financialEngine = financialEngine;
        _valuationEngine = valuationEngine;
        _gmpAnalyticsService = gmpAnalyticsService;
        _riskEngine = riskEngine;
        _fundUtilizationService = fundUtilizationService;
        _cache = cache;
    }

    public async Task<PagedResult<IpoSummaryDto>> GetIposAsync(IpoFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var query = _context.IPOs
            .AsNoTracking()
            .Include(i => i.Company)
                .ThenInclude(c => c.Financials)
            .Include(i => i.GmpHistories)
            .Include(i => i.SubscriptionHistories)
            .Include(i => i.Risks)
            .Include(i => i.Scores)
            .AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(s) ||
                                     i.Symbol.ToLower().Contains(s) ||
                                     i.Company.Name.ToLower().Contains(s) ||
                                     i.Company.Sector.ToLower().Contains(s));
        }

        if (filter.IpoType.HasValue)
        {
            query = query.Where(i => i.IpoType == filter.IpoType.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(i => i.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Sector))
        {
            query = query.Where(i => i.Company.Sector.ToLower() == filter.Sector.ToLower());
        }

        var list = await query.ToListAsync(cancellationToken);

        // Map to Summary DTOs
        var summaryList = list.Select(MapToSummaryDto).ToList();

        // In-memory filters for dynamic/computed values
        if (filter.MinGmpPercent.HasValue)
        {
            summaryList = summaryList.Where(s => (s.LatestGmpPercentage ?? 0) >= filter.MinGmpPercent.Value).ToList();
        }

        if (filter.MaxGmpPercent.HasValue)
        {
            summaryList = summaryList.Where(s => (s.LatestGmpPercentage ?? 0) <= filter.MaxGmpPercent.Value).ToList();
        }

        if (filter.MinSubscription.HasValue)
        {
            summaryList = summaryList.Where(s => (s.TotalSubscription ?? 0) >= filter.MinSubscription.Value).ToList();
        }

        if (filter.MinListingScore.HasValue)
        {
            summaryList = summaryList.Where(s => (s.ListingGainScore ?? 0) >= filter.MinListingScore.Value).ToList();
        }

        if (filter.MinLongTermScore.HasValue)
        {
            summaryList = summaryList.Where(s => (s.LongTermScore ?? 0) >= filter.MinLongTermScore.Value).ToList();
        }

        if (filter.Recommendation.HasValue)
        {
            summaryList = summaryList.Where(s => s.ListingRecommendation == filter.Recommendation.Value || s.LongTermRecommendation == filter.Recommendation.Value).ToList();
        }

        if (filter.Valuation.HasValue)
        {
            summaryList = summaryList.Where(s => s.ValuationClassification == filter.Valuation.Value).ToList();
        }

        // Sorting
        summaryList = (filter.SortBy?.ToLower(), filter.SortDescending) switch
        {
            ("gmp", true) => summaryList.OrderByDescending(s => s.LatestGmp ?? 0).ToList(),
            ("gmp", false) => summaryList.OrderBy(s => s.LatestGmp ?? 0).ToList(),
            ("gmppercent", true) => summaryList.OrderByDescending(s => s.LatestGmpPercentage ?? 0).ToList(),
            ("gmppercent", false) => summaryList.OrderBy(s => s.LatestGmpPercentage ?? 0).ToList(),
            ("subscription", true) => summaryList.OrderByDescending(s => s.TotalSubscription ?? 0).ToList(),
            ("subscription", false) => summaryList.OrderBy(s => s.TotalSubscription ?? 0).ToList(),
            ("listingscore", true) => summaryList.OrderByDescending(s => s.ListingGainScore ?? 0).ToList(),
            ("listingscore", false) => summaryList.OrderBy(s => s.ListingGainScore ?? 0).ToList(),
            ("longtermscore", true) => summaryList.OrderByDescending(s => s.LongTermScore ?? 0).ToList(),
            ("longtermscore", false) => summaryList.OrderBy(s => s.LongTermScore ?? 0).ToList(),
            ("issuesize", true) => summaryList.OrderByDescending(s => s.IssueSize).ToList(),
            ("issuesize", false) => summaryList.OrderBy(s => s.IssueSize).ToList(),
            ("opendate", false) => summaryList.OrderBy(s => s.OpenDate ?? DateTime.MaxValue).ToList(),
            _ => summaryList.OrderByDescending(s => s.OpenDate ?? DateTime.MinValue).ToList()
        };

        var total = summaryList.Count;
        var pagedItems = summaryList
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return new PagedResult<IpoSummaryDto>
        {
            Items = pagedItems,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = total
        };
    }

    public async Task<IpoDetailDto?> GetIpoByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ipo = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.Company)
                .ThenInclude(c => c.Financials)
            .Include(i => i.GmpHistories)
            .Include(i => i.SubscriptionHistories)
            .Include(i => i.Objectives)
            .Include(i => i.Risks)
            .Include(i => i.Scores)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        if (ipo == null) return null;

        var summary = MapToSummaryDto(ipo);

        return new IpoDetailDto
        {
            Id = summary.Id,
            CompanyId = summary.CompanyId,
            Name = summary.Name,
            Symbol = summary.Symbol,
            Sector = summary.Sector,
            Industry = summary.Industry,
            IpoType = summary.IpoType,
            Status = summary.Status,
            OpenDate = summary.OpenDate,
            CloseDate = summary.CloseDate,
            AllotmentDate = summary.AllotmentDate,
            ListingDate = summary.ListingDate,
            PriceBandLow = summary.PriceBandLow,
            PriceBandHigh = summary.PriceBandHigh,
            LotSize = summary.LotSize,
            MinimumInvestment = summary.MinimumInvestment,
            IssueSize = summary.IssueSize,
            FreshIssueAmount = summary.FreshIssueAmount,
            OFSAmount = summary.OFSAmount,
            LatestGmp = summary.LatestGmp,
            LatestGmpPercentage = summary.LatestGmpPercentage,
            EstimatedListingPrice = summary.EstimatedListingPrice,
            EstimatedProfitPerLot = summary.EstimatedProfitPerLot,
            ListingPrice = summary.ListingPrice,
            ListingGainPercent = summary.ListingGainPercent,
            ActualListingGainAmount = summary.ActualListingGainAmount,
            ActualListingGainPerLot = summary.ActualListingGainPerLot,
            Day1ClosePrice = summary.Day1ClosePrice,
            GmpTrend = summary.GmpTrend,
            Gmp24hChange = summary.Gmp24hChange,
            TotalSubscription = summary.TotalSubscription,
            QibSubscription = summary.QibSubscription,
            RetailSubscription = summary.RetailSubscription,
            NiiSubscription = summary.NiiSubscription,
            ListingGainScore = summary.ListingGainScore,
            ListingRecommendation = summary.ListingRecommendation,
            LongTermScore = summary.LongTermScore,
            LongTermRecommendation = summary.LongTermRecommendation,
            ValuationClassification = summary.ValuationClassification,
            HighRiskCount = summary.HighRiskCount,
            CompanyPE = summary.CompanyPE,
            IndustryPE = summary.IndustryPE,
            DataLastUpdatedAt = summary.DataLastUpdatedAt,

            LegalName = ipo.Company.LegalName,
            CIN = ipo.Company.CIN,
            CompanyDescription = ipo.Company.Description,
            Website = ipo.Company.Website,
            FoundedYear = ipo.Company.FoundedYear,
            Headquarters = ipo.Company.Headquarters,
            PromoterInformation = ipo.Company.PromoterInformation,
            ManagingDirector = ipo.Company.ManagingDirector,
            PromoterHoldingPreIssue = ipo.Company.PromoterHoldingPreIssue,
            PromoterHoldingPostIssue = ipo.Company.PromoterHoldingPostIssue,
            FaceValue = ipo.FaceValue,
            Registrar = ipo.Registrar,
            LeadManagers = ipo.LeadManagers,
            Exchange = ipo.Exchange
        };
    }

    public async Task<IReadOnlyList<IpoSearchDto>> SearchIposAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<IpoSearchDto>();

        var s = query.Trim().ToLower();
        var ipos = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.Company)
            .Include(i => i.GmpHistories)
            .Include(i => i.Scores)
            .Where(i => i.Name.ToLower().Contains(s) ||
                        i.Symbol.ToLower().Contains(s) ||
                        i.Company.Name.ToLower().Contains(s) ||
                        i.Company.Sector.ToLower().Contains(s))
            .Take(10)
            .ToListAsync(cancellationToken);

        return ipos.Select(i =>
        {
            var latestGmp = i.GmpHistories.OrderBy(g => g.ObservedAt).LastOrDefault();
            var latestScore = i.Scores.OrderBy(sc => sc.CalculatedAt).LastOrDefault();
            return new IpoSearchDto
            {
                Id = i.Id,
                CompanyId = i.CompanyId,
                Name = i.Name,
                Symbol = i.Symbol,
                Sector = i.Company.Sector,
                IpoType = i.IpoType,
                Status = i.Status,
                LatestGmpPercentage = latestGmp?.GMPPercentage ?? (i.PriceBandHigh > 0 && latestGmp != null ? Math.Round((latestGmp.GMP / i.PriceBandHigh) * 100, 2) : 0),
                ListingGainScore = latestScore?.ListingGainScore
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<IpoSummaryDto>> GetUpcomingIposAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.Company).ThenInclude(c => c.Financials)
            .Include(i => i.GmpHistories)
            .Include(i => i.SubscriptionHistories)
            .Include(i => i.Risks)
            .Include(i => i.Scores)
            .OrderBy(i => i.OpenDate)
            .ToListAsync(cancellationToken);

        return list
            .Select(MapToSummaryDto)
            .Where(s => s.Status == IpoStatus.Upcoming)
            .ToList();
    }

    public async Task<IReadOnlyList<IpoSummaryDto>> GetOpenIposAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.Company).ThenInclude(c => c.Financials)
            .Include(i => i.GmpHistories)
            .Include(i => i.SubscriptionHistories)
            .Include(i => i.Risks)
            .Include(i => i.Scores)
            .OrderBy(i => i.CloseDate)
            .ToListAsync(cancellationToken);

        return list
            .Select(MapToSummaryDto)
            .Where(s => s.Status == IpoStatus.Open)
            .ToList();
    }

    public async Task<IReadOnlyList<IpoSummaryDto>> GetListedIposAsync(CancellationToken cancellationToken = default)
    {
        var list = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.Company).ThenInclude(c => c.Financials)
            .Include(i => i.GmpHistories)
            .Include(i => i.SubscriptionHistories)
            .Include(i => i.Risks)
            .Include(i => i.Scores)
            .Where(i => i.Status == IpoStatus.Listed)
            .OrderByDescending(i => i.ListingDate)
            .ToListAsync(cancellationToken);

        return list.Select(MapToSummaryDto).ToList();
    }

    public async Task<GmpHistoryDto?> GetGmpHistoryAsync(Guid ipoId, CancellationToken cancellationToken = default)
    {
        var ipo = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.GmpHistories)
            .FirstOrDefaultAsync(i => i.Id == ipoId, cancellationToken);

        return ipo == null ? null : _gmpAnalyticsService.AnalyzeGmpHistory(ipo);
    }

    public async Task<SubscriptionBreakdownDto?> GetSubscriptionAsync(Guid ipoId, CancellationToken cancellationToken = default)
    {
        var ipo = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.SubscriptionHistories)
            .FirstOrDefaultAsync(i => i.Id == ipoId, cancellationToken);

        if (ipo == null) return null;

        var history = ipo.SubscriptionHistories.OrderBy(s => s.DayNumber).ToList();
        var latest = history.LastOrDefault();

        var verdict = "No bidding data yet.";
        if (latest != null)
        {
            if (latest.QibSubscription > 25) verdict = $"Strong Institutional Backing (QIB: {latest.QibSubscription:F1}x)";
            else if (latest.RetailSubscription > 15) verdict = $"Retail Frenzy (Retail: {latest.RetailSubscription:F1}x)";
            else if (latest.TotalSubscription > 5) verdict = $"Solid Multi-Segment Demand ({latest.TotalSubscription:F1}x Total)";
            else if (latest.TotalSubscription >= 1) verdict = $"Adequately Subscribed ({latest.TotalSubscription:F1}x)";
            else verdict = $"Subdued Subscription Demand ({latest.TotalSubscription:F1}x)";
        }

        return new SubscriptionBreakdownDto
        {
            IpoId = ipo.Id,
            IpoName = ipo.Name,
            LatestTotalSubscription = latest?.TotalSubscription ?? 0,
            LatestQibSubscription = latest?.QibSubscription ?? 0,
            LatestNiiSubscription = latest?.NiiSubscription ?? 0,
            LatestRetailSubscription = latest?.RetailSubscription ?? 0,
            LatestEmployeeSubscription = latest?.EmployeeSubscription,
            LatestShareholderSubscription = latest?.ShareholderSubscription,
            DemandQualityVerdict = verdict,
            History = history.Select(h => new SubscriptionSnapshotDto
            {
                Id = h.Id,
                IpoId = h.IpoId,
                RetailSubscription = h.RetailSubscription,
                QibSubscription = h.QibSubscription,
                NiiSubscription = h.NiiSubscription,
                EmployeeSubscription = h.EmployeeSubscription,
                ShareholderSubscription = h.ShareholderSubscription,
                TotalSubscription = h.TotalSubscription,
                SnapshotDate = h.SnapshotDate,
                DayNumber = h.DayNumber,
                Source = h.Source
            }).ToList()
        };
    }

    public async Task<CompanyFinancialReportDto?> GetFinancialsAsync(Guid ipoId, CancellationToken cancellationToken = default)
    {
        var ipo = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.Company)
                .ThenInclude(c => c.Financials)
            .FirstOrDefaultAsync(i => i.Id == ipoId, cancellationToken);

        return ipo == null ? null : _financialEngine.AnalyzeFinancials(ipo.Company);
    }

    public async Task<ComprehensiveAnalysisDto?> GetAnalysisAsync(Guid ipoId, CancellationToken cancellationToken = default)
    {
        var ipo = await _context.IPOs
            .AsNoTracking()
            .Include(i => i.Company)
                .ThenInclude(c => c.Financials)
            .Include(i => i.GmpHistories)
            .Include(i => i.SubscriptionHistories)
            .Include(i => i.Objectives)
            .Include(i => i.Risks)
            .Include(i => i.Scores)
            .FirstOrDefaultAsync(i => i.Id == ipoId, cancellationToken);

        if (ipo == null) return null;

        var ind = await _context.IndustryMetrics
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Sector == ipo.Company.Sector, cancellationToken);

        var finReport = _financialEngine.AnalyzeFinancials(ipo.Company);
        var latestFin = ipo.Company.Financials.OrderBy(f => f.PeriodEnding).LastOrDefault();
        var val = _valuationEngine.EvaluateValuation(ipo, latestFin, ind);
        var gmp = _gmpAnalyticsService.AnalyzeGmpHistory(ipo);
        var subBreakdown = await GetSubscriptionAsync(ipoId, cancellationToken);
        var risks = _riskEngine.EvaluateRisks(ipo, ipo.Company.Financials.ToList(), ind, gmp);
        var fundUtil = _fundUtilizationService.AnalyzeFundUtilization(ipo);
        var score = _scoringEngine.CalculateScore(ipo, finReport, val, gmp, subBreakdown, ipo.Risks.ToList());

        var business = new BusinessAnalysisDto
        {
            CompanyId = ipo.CompanyId,
            WhatCompanyDoes = ipo.Company.Description,
            HowItMakesMoney = $"Generates operational income across {ipo.Company.Industry} operations.",
            MainProductsServices = new[] { $"{ipo.Company.Industry} Solutions", "Turnkey Implementations", "Annual Maintenance & Support" },
            CustomerTypes = new[] { "Institutional Enterprises", "Government / PSU Utilities", "Tier-1 Industrial OEMs" },
            CompetitivePosition = "Ranked among prominent domestic operators with established client contracts.",
            GrowthDrivers = new[] { "Secular industry market expansion", "Operational capacity additions via IPO fresh proceeds", "Deepening export wallet share" },
            KeyStrengths = new[] { "Robust balance sheet structure", "Integrated manufacturing / service capabilities", "Experienced executive management" }
        };

        return new ComprehensiveAnalysisDto
        {
            IpoId = ipo.Id,
            Valuation = val,
            FundUtilization = fundUtil,
            Business = business,
            Risks = risks,
            Score = score
        };
    }

    public async Task<ScoreBreakdownDto?> GetScoreAsync(Guid ipoId, CancellationToken cancellationToken = default)
    {
        var analysis = await GetAnalysisAsync(ipoId, cancellationToken);
        return analysis?.Score;
    }

    private IpoSummaryDto MapToSummaryDto(IPO ipo)
    {
        var latestGmp = ipo.GmpHistories.OrderBy(g => g.ObservedAt).LastOrDefault();
        var latestSub = ipo.SubscriptionHistories.OrderBy(s => s.DayNumber).LastOrDefault();
        var latestScore = ipo.Scores.OrderBy(sc => sc.CalculatedAt).LastOrDefault();
        var latestFin = ipo.Company?.Financials?.OrderBy(f => f.PeriodEnding).LastOrDefault();

        decimal? companyPe = null;
        if (latestFin?.EPS.HasValue == true && latestFin.EPS.Value > 0 && ipo.PriceBandHigh > 0)
        {
            companyPe = Math.Round(ipo.PriceBandHigh / latestFin.EPS.Value, 2);
        }

        var gmpPercent = latestGmp?.GMPPercentage ?? (ipo.PriceBandHigh > 0 && latestGmp != null ? Math.Round((latestGmp.GMP / ipo.PriceBandHigh) * 100, 2) : 0);

        int dynamicListingScore;
        RecommendationRating dynamicListingRec;
        if (latestScore != null)
        {
            dynamicListingScore = latestScore.ListingGainScore;
            dynamicListingRec = latestScore.ListingRecommendation;
        }
        else
        {
            if (gmpPercent >= 50) { dynamicListingScore = Math.Min(98, 88 + (int)(gmpPercent / 12)); dynamicListingRec = RecommendationRating.Strong; }
            else if (gmpPercent >= 25) { dynamicListingScore = 78 + (int)((gmpPercent - 25) / 3.0m); dynamicListingRec = RecommendationRating.Strong; }
            else if (gmpPercent >= 10) { dynamicListingScore = 65 + (int)((gmpPercent - 10) / 1.8m); dynamicListingRec = RecommendationRating.Positive; }
            else if (gmpPercent >= 2) { dynamicListingScore = 52 + (int)(gmpPercent * 2); dynamicListingRec = RecommendationRating.Neutral; }
            else { dynamicListingScore = Math.Max(28, 42 - (int)Math.Abs(gmpPercent)); dynamicListingRec = RecommendationRating.Weak; }
        }

        int dynamicLtScore;
        RecommendationRating dynamicLtRec;
        if (latestScore != null)
        {
            dynamicLtScore = latestScore.LongTermScore;
            dynamicLtRec = latestScore.LongTermRecommendation;
        }
        else
        {
            if (ipo.IpoType == IpoType.Mainboard && ipo.IssueSize > 800) { dynamicLtScore = 82; dynamicLtRec = RecommendationRating.Strong; }
            else if (ipo.IpoType == IpoType.Sme) { dynamicLtScore = 63; dynamicLtRec = RecommendationRating.Neutral; }
            else { dynamicLtScore = 68; dynamicLtRec = RecommendationRating.Positive; }
        }

        var todayUtc = DateTime.UtcNow.Date;
        var status = ipo.Status;
        if (status != IpoStatus.Listed)
        {
            if (ipo.CloseDate.HasValue && ipo.CloseDate.Value.Date <= todayUtc)
            {
                status = IpoStatus.Closed;
            }
            else if (ipo.OpenDate.HasValue && ipo.OpenDate.Value.Date > todayUtc)
            {
                status = IpoStatus.Upcoming;
            }
            else if (ipo.OpenDate.HasValue && ipo.OpenDate.Value.Date <= todayUtc && ipo.CloseDate.HasValue && ipo.CloseDate.Value.Date > todayUtc)
            {
                status = IpoStatus.Open;
            }
        }

        // Subscription: only show if open/closed/listed
        decimal? totalSub = status == IpoStatus.Upcoming ? null : latestSub?.TotalSubscription;
        decimal? qibSub = status == IpoStatus.Upcoming ? null : latestSub?.QibSubscription;
        decimal? retailSub = status == IpoStatus.Upcoming ? null : latestSub?.RetailSubscription;
        decimal? niiSub = status == IpoStatus.Upcoming ? null : latestSub?.NiiSubscription;

        // Estimated Profit / Loss based on current GMP
        decimal? estimatedProfitPerLot = null;
        if (latestGmp != null && ipo.LotSize > 0)
        {
            estimatedProfitPerLot = latestGmp.GMP * ipo.LotSize;
        }

        // Actual Listing Day Metrics (for Listed IPOs)
        decimal? listingPrice = ipo.ListingPrice;
        decimal? listingGainPercent = ipo.ListingGainPercent;
        decimal? actualGainAmount = null;
        decimal? actualGainPerLot = null;

        if (status == IpoStatus.Listed)
        {
            listingPrice ??= (latestGmp != null && latestGmp.EstimatedListingPrice > 0 ? latestGmp.EstimatedListingPrice : (latestGmp != null ? ipo.PriceBandHigh + latestGmp.GMP : ipo.PriceBandHigh));
            if (listingPrice.HasValue && ipo.PriceBandHigh > 0)
            {
                actualGainAmount = listingPrice.Value - ipo.PriceBandHigh;
                actualGainPerLot = ipo.LotSize > 0 ? actualGainAmount.Value * ipo.LotSize : null;
                listingGainPercent ??= Math.Round((actualGainAmount.Value / ipo.PriceBandHigh) * 100m, 2);
            }
        }

        return new IpoSummaryDto
        {
            Id = ipo.Id,
            CompanyId = ipo.CompanyId,
            Name = ipo.Name,
            Symbol = ipo.Symbol,
            Sector = ipo.Company?.Sector ?? string.Empty,
            Industry = ipo.Company?.Industry ?? string.Empty,
            IpoType = ipo.IpoType,
            Status = status,
            OpenDate = ipo.OpenDate,
            CloseDate = ipo.CloseDate,
            AllotmentDate = ipo.AllotmentDate,
            ListingDate = ipo.ListingDate,
            PriceBandLow = ipo.PriceBandLow,
            PriceBandHigh = ipo.PriceBandHigh,
            LotSize = ipo.LotSize,
            MinimumInvestment = ipo.MinimumInvestment > 0 ? ipo.MinimumInvestment : ipo.PriceBandHigh * ipo.LotSize,
            IssueSize = ipo.IssueSize,
            FreshIssueAmount = ipo.FreshIssueAmount,
            OFSAmount = ipo.OFSAmount,
            LatestGmp = latestGmp?.GMP,
            LatestGmpPercentage = gmpPercent,
            EstimatedListingPrice = latestGmp != null ? ipo.PriceBandHigh + latestGmp.GMP : ipo.PriceBandHigh,
            EstimatedProfitPerLot = estimatedProfitPerLot,
            ListingPrice = listingPrice,
            ListingGainPercent = listingGainPercent,
            ActualListingGainAmount = actualGainAmount,
            ActualListingGainPerLot = actualGainPerLot,
            Day1ClosePrice = ipo.Day1ClosePrice ?? listingPrice,
            GmpTrend = latestGmp != null ? GmpTrend.Increasing : GmpTrend.Stable,
            Gmp24hChange = 0,
            TotalSubscription = totalSub,
            QibSubscription = qibSub,
            RetailSubscription = retailSub,
            NiiSubscription = niiSub,
            ListingGainScore = dynamicListingScore,
            ListingRecommendation = dynamicListingRec,
            LongTermScore = dynamicLtScore,
            LongTermRecommendation = dynamicLtRec,
            ValuationClassification = ValuationClassification.Reasonable,
            HighRiskCount = ipo.Risks.Count(r => r.Severity >= RiskSeverity.High),
            CompanyPE = companyPe,
            IndustryPE = 35.0m,
            DataLastUpdatedAt = latestGmp?.RetrievedAt ?? ipo.UpdatedAt
        };
    }
}
