using IPOForge.Domain.Enums;

namespace IPOForge.Contracts.Gmp;

public class GmpSnapshotDto
{
    public Guid Id { get; set; }
    public Guid IpoId { get; set; }
    public decimal GMP { get; set; }
    public decimal GMPPercentage { get; set; }
    public decimal EstimatedListingPrice { get; set; }
    public decimal? KostakRate { get; set; }
    public decimal? SubjectToSauda { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime ObservedAt { get; set; }
    public decimal Confidence { get; set; }
}

public class GmpHistoryDto
{
    public Guid IpoId { get; set; }
    public string IpoName { get; set; } = string.Empty;
    public decimal PriceBandHigh { get; set; }
    public decimal CurrentGmp { get; set; }
    public decimal CurrentGmpPercentage { get; set; }
    public decimal EstimatedListingPrice { get; set; }
    public decimal HighestGmp { get; set; }
    public decimal LowestGmp { get; set; }
    public decimal Gmp24hChange { get; set; }
    public decimal Gmp3dChange { get; set; }
    public decimal Gmp7dChange { get; set; }
    public decimal GmpVolatility { get; set; }
    public GmpTrend Trend { get; set; }
    public string Disclaimer { get; set; } = "GMP (Grey Market Premium) is an unofficial, unregulated market indicator and does not guarantee listing gains. Sources are market aggregator reports.";
    public DateTime LastUpdatedAt { get; set; }
    public IReadOnlyList<GmpSnapshotDto> Snapshots { get; set; } = Array.Empty<GmpSnapshotDto>();
}

public class GmpMoverDto
{
    public Guid IpoId { get; set; }
    public string IpoName { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public IpoType IpoType { get; set; }
    public IpoStatus Status { get; set; }
    public decimal CurrentGmp { get; set; }
    public decimal CurrentGmpPercentage { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal ChangePercent { get; set; }
    public GmpTrend Trend { get; set; }
}

public class GmpAccuracyAnalyticsDto
{
    public int TotalListedIposAnalyzed { get; set; }
    public decimal AveragePredictionErrorPercent { get; set; }
    public decimal MedianPredictionErrorPercent { get; set; }
    public decimal Within5PercentAccuracyRate { get; set; } // % of IPOs where GMP predicted listing price within ±5%
    public decimal Within10PercentAccuracyRate { get; set; }
    public decimal Within20PercentAccuracyRate { get; set; }
    public IReadOnlyList<GmpListingAccuracyItemDto> HistoricalComparison { get; set; } = Array.Empty<GmpListingAccuracyItemDto>();
}

public class GmpListingAccuracyItemDto
{
    public Guid IpoId { get; set; }
    public string IpoName { get; set; } = string.Empty;
    public DateTime? ListingDate { get; set; }
    public decimal IssuePrice { get; set; }
    public decimal FinalGmp { get; set; }
    public decimal EstimatedListingPrice { get; set; }
    public decimal ActualListingPrice { get; set; }
    public decimal GmpPredictedGainPercent { get; set; }
    public decimal ActualListingGainPercent { get; set; }
    public decimal PredictionErrorPercent { get; set; }
    public bool IsAccurateWithin10Percent => Math.Abs(PredictionErrorPercent) <= 10.0m;
}
