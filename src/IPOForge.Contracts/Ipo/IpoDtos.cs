using IPOForge.Domain.Enums;

namespace IPOForge.Contracts.Ipo;

public class IpoSummaryDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public IpoType IpoType { get; set; }
    public IpoStatus Status { get; set; }

    public DateTime? OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public DateTime? AllotmentDate { get; set; }
    public DateTime? ListingDate { get; set; }

    public decimal PriceBandLow { get; set; }
    public decimal PriceBandHigh { get; set; }
    public int LotSize { get; set; }
    public decimal MinimumInvestment { get; set; }
    public decimal IssueSize { get; set; } // ₹ Cr
    public decimal FreshIssueAmount { get; set; }
    public decimal OFSAmount { get; set; }

    // Latest GMP snapshot
    public decimal? LatestGmp { get; set; }
    public decimal? LatestGmpPercentage { get; set; }
    public decimal? EstimatedListingPrice { get; set; }
    public GmpTrend? GmpTrend { get; set; }
    public decimal? Gmp24hChange { get; set; }

    // Latest Subscription snapshot
    public decimal? TotalSubscription { get; set; }
    public decimal? QibSubscription { get; set; }
    public decimal? RetailSubscription { get; set; }
    public decimal? NiiSubscription { get; set; }

    // Scores & Recommendations
    public int? ListingGainScore { get; set; }
    public RecommendationRating? ListingRecommendation { get; set; }
    public int? LongTermScore { get; set; }
    public RecommendationRating? LongTermRecommendation { get; set; }

    // Valuation & Risk highlights
    public ValuationClassification? ValuationClassification { get; set; }
    public int HighRiskCount { get; set; }
    public decimal? CompanyPE { get; set; }
    public decimal? IndustryPE { get; set; }

    public DateTime? DataLastUpdatedAt { get; set; }
}

public class IpoDetailDto : IpoSummaryDto
{
    public string LegalName { get; set; } = string.Empty;
    public string? CIN { get; set; }
    public string CompanyDescription { get; set; } = string.Empty;
    public string? Website { get; set; }
    public int? FoundedYear { get; set; }
    public string? Headquarters { get; set; }
    public string? PromoterInformation { get; set; }
    public string? ManagingDirector { get; set; }
    public decimal? PromoterHoldingPreIssue { get; set; }
    public decimal? PromoterHoldingPostIssue { get; set; }

    public decimal FaceValue { get; set; }
    public string Registrar { get; set; } = string.Empty;
    public string LeadManagers { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;

    public decimal? ActualListingPrice { get; set; }
    public decimal? ActualListingGainPercent { get; set; }
    public decimal? Day1ClosePrice { get; set; }

    public decimal FreshIssuePercentage => IssueSize > 0 ? Math.Round((FreshIssueAmount / IssueSize) * 100, 2) : 0;
    public decimal OfsPercentage => IssueSize > 0 ? Math.Round((OFSAmount / IssueSize) * 100, 2) : 0;
}

public class IpoFilterRequest
{
    public string? Search { get; set; }
    public IpoType? IpoType { get; set; }
    public IpoStatus? Status { get; set; }
    public string? Sector { get; set; }
    public decimal? MinGmpPercent { get; set; }
    public decimal? MaxGmpPercent { get; set; }
    public decimal? MinSubscription { get; set; }
    public int? MinListingScore { get; set; }
    public int? MinLongTermScore { get; set; }
    public RecommendationRating? Recommendation { get; set; }
    public ValuationClassification? Valuation { get; set; }
    public string? SortBy { get; set; } = "OpenDate"; // OpenDate, GMP, GMPPercent, Subscription, ListingScore, LongTermScore, IssueSize
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class IpoSearchDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public IpoType IpoType { get; set; }
    public IpoStatus Status { get; set; }
    public decimal? LatestGmpPercentage { get; set; }
    public int? ListingGainScore { get; set; }
}
