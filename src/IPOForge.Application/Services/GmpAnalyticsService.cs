using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Gmp;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;

namespace IPOForge.Application.Services;

public class GmpAnalyticsService : IGmpAnalyticsService
{
    public GmpHistoryDto AnalyzeGmpHistory(IPO ipo)
    {
        var result = new GmpHistoryDto
        {
            IpoId = ipo.Id,
            IpoName = ipo.Name,
            PriceBandHigh = ipo.PriceBandHigh,
            LastUpdatedAt = DateTime.UtcNow
        };

        var historyList = ipo.GmpHistories
            .OrderBy(g => g.ObservedAt)
            .ToList();

        if (!historyList.Any())
        {
            result.Trend = GmpTrend.Stable;
            result.EstimatedListingPrice = ipo.PriceBandHigh;
            return result;
        }

        var latest = historyList.Last();
        result.CurrentGmp = latest.GMP;
        result.CurrentGmpPercentage = ipo.PriceBandHigh > 0
            ? Math.Round((latest.GMP / ipo.PriceBandHigh) * 100, 2)
            : 0;
        result.EstimatedListingPrice = ipo.PriceBandHigh + latest.GMP;
        result.HighestGmp = historyList.Max(g => g.GMP);
        result.LowestGmp = historyList.Min(g => g.GMP);
        result.LastUpdatedAt = latest.RetrievedAt;

        // Calculate 24h, 3d, 7d changes
        var now = latest.ObservedAt;
        var oneDayAgo = historyList.Where(g => g.ObservedAt <= now.AddDays(-1)).OrderByDescending(g => g.ObservedAt).FirstOrDefault();
        var threeDaysAgo = historyList.Where(g => g.ObservedAt <= now.AddDays(-3)).OrderByDescending(g => g.ObservedAt).FirstOrDefault();
        var sevenDaysAgo = historyList.Where(g => g.ObservedAt <= now.AddDays(-7)).OrderByDescending(g => g.ObservedAt).FirstOrDefault();

        if (oneDayAgo != null)
        {
            result.Gmp24hChange = latest.GMP - oneDayAgo.GMP;
        }
        else if (historyList.Count > 1)
        {
            result.Gmp24hChange = latest.GMP - historyList[^2].GMP;
        }

        if (threeDaysAgo != null)
        {
            result.Gmp3dChange = latest.GMP - threeDaysAgo.GMP;
        }

        if (sevenDaysAgo != null)
        {
            result.Gmp7dChange = latest.GMP - sevenDaysAgo.GMP;
        }

        // Calculate Volatility (standard deviation of GMP percentages)
        if (historyList.Count > 1)
        {
            var gmpPercents = historyList.Select(g => (double)g.GMPPercentage).ToList();
            var avg = gmpPercents.Average();
            var sumOfSquares = gmpPercents.Sum(p => Math.Pow(p - avg, 2));
            result.GmpVolatility = Math.Round((decimal)Math.Sqrt(sumOfSquares / gmpPercents.Count), 2);
        }

        // Determine Trend
        result.Trend = DetermineTrend(result.Gmp24hChange, result.Gmp3dChange, result.CurrentGmpPercentage);

        result.Snapshots = historyList.Select(h => new GmpSnapshotDto
        {
            Id = h.Id,
            IpoId = h.IpoId,
            GMP = h.GMP,
            GMPPercentage = ipo.PriceBandHigh > 0 ? Math.Round((h.GMP / ipo.PriceBandHigh) * 100, 2) : 0,
            EstimatedListingPrice = ipo.PriceBandHigh + h.GMP,
            KostakRate = h.KostakRate,
            SubjectToSauda = h.SubjectToSauda,
            Source = h.Source,
            ObservedAt = h.ObservedAt,
            Confidence = h.Confidence
        }).ToList();

        return result;
    }

    private static GmpTrend DetermineTrend(decimal change24h, decimal change3d, decimal currentPercent)
    {
        if (change3d > 20 || (change24h > 15 && currentPercent > 30))
            return GmpTrend.StronglyIncreasing;
        if (change24h > 2 || change3d > 5)
            return GmpTrend.Increasing;
        if (change3d < -20 || (change24h < -15 && currentPercent < 15))
            return GmpTrend.StronglyDeclining;
        if (change24h < -2 || change3d < -5)
            return GmpTrend.Declining;
        return GmpTrend.Stable;
    }

    public IReadOnlyList<GmpMoverDto> GetTopMovers(IReadOnlyList<IPO> ipos, int count = 8)
    {
        var list = new List<GmpMoverDto>();

        // Focus on active/current & upcoming/closed issues with live GMP
        var targetIpos = ipos
            .Where(i => i.Status == IpoStatus.Open || i.Status == IpoStatus.Upcoming || i.Status == IpoStatus.Closed)
            .ToList();

        if (!targetIpos.Any())
        {
            targetIpos = ipos.ToList();
        }

        foreach (var ipo in targetIpos)
        {
            var analysis = AnalyzeGmpHistory(ipo);
            var gmpVal = analysis.CurrentGmp > 0 ? analysis.CurrentGmp : (ipo.GmpHistories.LastOrDefault()?.GMP ?? 0);
            var gmpPct = analysis.CurrentGmpPercentage > 0 ? analysis.CurrentGmpPercentage : (ipo.PriceBandHigh > 0 && gmpVal > 0 ? Math.Round((gmpVal / ipo.PriceBandHigh) * 100, 2) : 0);

            if (gmpVal <= 0 && gmpPct <= 0) continue;

            var lotSize = ipo.LotSize > 0 ? ipo.LotSize : (ipo.IpoType == IpoType.Sme ? 1200 : 30);
            var estProfitPerLot = gmpVal * lotSize;

            list.Add(new GmpMoverDto
            {
                IpoId = ipo.Id,
                IpoName = ipo.Name,
                Symbol = ipo.Symbol,
                IpoType = ipo.IpoType,
                Status = ipo.Status,
                OpenDate = ipo.OpenDate,
                CloseDate = ipo.CloseDate,
                ListingDate = ipo.ListingDate,
                PriceBandHigh = ipo.PriceBandHigh,
                LotSize = lotSize,
                EstimatedProfitPerLot = estProfitPerLot,
                CurrentGmp = gmpVal,
                CurrentGmpPercentage = gmpPct,
                ChangeAmount = analysis.Gmp24hChange,
                ChangePercent = analysis.PriceBandHigh > 0 ? Math.Round((analysis.Gmp24hChange / analysis.PriceBandHigh) * 100, 2) : 0,
                Trend = analysis.Trend
            });
        }

        return list.OrderByDescending(m => m.CurrentGmpPercentage).Take(count).ToList();
    }

    public GmpAccuracyAnalyticsDto GetAccuracyAnalytics(IReadOnlyList<IPO> listedIpos)
    {
        var accuracyItems = new List<GmpListingAccuracyItemDto>();

        foreach (var ipo in listedIpos.Where(i => i.Status == IpoStatus.Listed && i.ListingPrice.HasValue && i.PriceBandHigh > 0))
        {
            var finalGmp = ipo.GmpHistories.OrderBy(g => g.ObservedAt).LastOrDefault()?.GMP ?? 0;
            var issuePrice = ipo.PriceBandHigh;
            var estListing = issuePrice + finalGmp;
            var actListing = ipo.ListingPrice!.Value;

            var gmpPredGain = Math.Round((finalGmp / issuePrice) * 100, 2);
            var actGain = Math.Round(((actListing - issuePrice) / issuePrice) * 100, 2);
            var predError = Math.Round(((actListing - estListing) / issuePrice) * 100, 2);

            accuracyItems.Add(new GmpListingAccuracyItemDto
            {
                IpoId = ipo.Id,
                IpoName = ipo.Name,
                ListingDate = ipo.ListingDate,
                IssuePrice = issuePrice,
                FinalGmp = finalGmp,
                EstimatedListingPrice = estListing,
                ActualListingPrice = actListing,
                GmpPredictedGainPercent = gmpPredGain,
                ActualListingGainPercent = actGain,
                PredictionErrorPercent = predError
            });
        }

        if (!accuracyItems.Any())
        {
            return new GmpAccuracyAnalyticsDto();
        }

        var total = accuracyItems.Count;
        var avgError = Math.Round(accuracyItems.Average(x => Math.Abs(x.PredictionErrorPercent)), 2);

        var sortedErrors = accuracyItems.Select(x => Math.Abs(x.PredictionErrorPercent)).OrderBy(x => x).ToList();
        var medianError = sortedErrors[total / 2];

        var within5 = Math.Round((decimal)accuracyItems.Count(x => Math.Abs(x.PredictionErrorPercent) <= 5.0m) / total * 100, 1);
        var within10 = Math.Round((decimal)accuracyItems.Count(x => Math.Abs(x.PredictionErrorPercent) <= 10.0m) / total * 100, 1);
        var within20 = Math.Round((decimal)accuracyItems.Count(x => Math.Abs(x.PredictionErrorPercent) <= 20.0m) / total * 100, 1);

        return new GmpAccuracyAnalyticsDto
        {
            TotalListedIposAnalyzed = total,
            AveragePredictionErrorPercent = avgError,
            MedianPredictionErrorPercent = medianError,
            Within5PercentAccuracyRate = within5,
            Within10PercentAccuracyRate = within10,
            Within20PercentAccuracyRate = within20,
            HistoricalComparison = accuracyItems.OrderByDescending(x => x.ListingDate).ToList()
        };
    }
}
