using IPOForge.Domain.Enums;

namespace IPOForge.Contracts.Analysis;

public class ValuationAnalysisDto
{
    public Guid IpoId { get; set; }
    public decimal? CompanyPE { get; set; }
    public decimal? CompanyPB { get; set; }
    public decimal? CompanyEvEbitda { get; set; }
    public decimal? EPS { get; set; }
    public decimal? IndustryPE { get; set; }
    public decimal? IndustryPB { get; set; }
    public ValuationClassification Classification { get; set; } = ValuationClassification.Reasonable;
    public string SummaryText { get; set; } = string.Empty;
    public decimal? ValuationDiscountOrPremiumPercent { get; set; }
}

public class ObjectiveAllocationItemDto
{
    public Guid Id { get; set; }
    public ObjectiveCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AmountInCrores { get; set; }
    public decimal Percentage { get; set; }
}

public class FundUtilizationDto
{
    public Guid IpoId { get; set; }
    public decimal TotalIssueSize { get; set; }
    public decimal FreshIssueAmount { get; set; }
    public decimal FreshIssuePercent { get; set; }
    public decimal OfsAmount { get; set; }
    public decimal OfsPercent { get; set; }

    public decimal DebtRepaymentPercent { get; set; }
    public decimal ExpansionCapexPercent { get; set; }
    public decimal WorkingCapitalPercent { get; set; }
    public decimal GeneralCorporatePercent { get; set; }
    public decimal OtherUtilizationPercent { get; set; }

    public string OfsAssessment { get; set; } = string.Empty;
    public IReadOnlyList<ObjectiveAllocationItemDto> Objectives { get; set; } = Array.Empty<ObjectiveAllocationItemDto>();
}

public class BusinessAnalysisDto
{
    public Guid CompanyId { get; set; }
    public string WhatCompanyDoes { get; set; } = string.Empty;
    public string HowItMakesMoney { get; set; } = string.Empty;
    public IReadOnlyList<string> MainProductsServices { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> CustomerTypes { get; set; } = Array.Empty<string>();
    public string CompetitivePosition { get; set; } = string.Empty;
    public IReadOnlyList<string> GrowthDrivers { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> KeyStrengths { get; set; } = Array.Empty<string>();
}

public class RiskItemDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RiskSeverity Severity { get; set; }
    public string? TraceableMetric { get; set; }
}

public class RiskAnalysisDto
{
    public Guid IpoId { get; set; }
    public int CriticalRiskCount { get; set; }
    public int HighRiskCount { get; set; }
    public int MediumRiskCount { get; set; }
    public int LowRiskCount { get; set; }
    public string OverallRiskLevel { get; set; } = "Moderate"; // Low, Moderate, High, Extreme
    public IReadOnlyList<RiskItemDto> IdentifiedRisks { get; set; } = Array.Empty<RiskItemDto>();
}

public class ScorePillarDto
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public int MaxScore { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ScoreBreakdownDto
{
    public Guid IpoId { get; set; }
    public DateTime CalculatedAt { get; set; }

    // Listing Gain (100 Max)
    public int ListingGainScore { get; set; }
    public RecommendationRating ListingRecommendation { get; set; }
    public string ListingGainVerdict { get; set; } = string.Empty;
    public IReadOnlyList<ScorePillarDto> ListingGainPillars { get; set; } = Array.Empty<ScorePillarDto>();

    // Long Term (100 Max)
    public int LongTermScore { get; set; }
    public RecommendationRating LongTermRecommendation { get; set; }
    public string LongTermVerdict { get; set; } = string.Empty;
    public IReadOnlyList<ScorePillarDto> LongTermPillars { get; set; } = Array.Empty<ScorePillarDto>();

    public string UnifiedAnalyticalConclusion { get; set; } = string.Empty;
}

public class ComprehensiveAnalysisDto
{
    public Guid IpoId { get; set; }
    public ValuationAnalysisDto Valuation { get; set; } = new();
    public FundUtilizationDto FundUtilization { get; set; } = new();
    public BusinessAnalysisDto Business { get; set; } = new();
    public RiskAnalysisDto Risks { get; set; } = new();
    public ScoreBreakdownDto Score { get; set; } = new();
}
