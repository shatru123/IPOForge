namespace IPOForge.Contracts.Financial;

public class FinancialYearDto
{
    public Guid Id { get; set; }
    public string FiscalYear { get; set; } = string.Empty;
    public DateTime PeriodEnding { get; set; }
    public bool IsAudited { get; set; }

    public decimal? Revenue { get; set; }
    public decimal? EBITDA { get; set; }
    public decimal? EBIT { get; set; }
    public decimal? PAT { get; set; }
    public decimal? EPS { get; set; }
    public decimal? OperatingCashFlow { get; set; }
    public decimal? FreeCashFlow { get; set; }
    public decimal? TotalAssets { get; set; }
    public decimal? TotalDebt { get; set; }
    public decimal? NetWorth { get; set; }
    public decimal? CurrentAssets { get; set; }
    public decimal? CurrentLiabilities { get; set; }

    public decimal? EbitdaMargin { get; set; }
    public decimal? PatMargin { get; set; }
    public decimal? ROE { get; set; }
    public decimal? ROCE { get; set; }
    public decimal? DebtToEquity { get; set; }
    public decimal? CurrentRatio { get; set; }
}

public class FinancialGrowthDto
{
    public decimal? RevenueGrowthYoY { get; set; }
    public decimal? RevenueCagr3Year { get; set; }
    public decimal? ProfitGrowthYoY { get; set; }
    public decimal? ProfitCagr3Year { get; set; }
    public decimal? EbitdaCagr3Year { get; set; }
    public decimal? EpsGrowthYoY { get; set; }
    public string CashFlowTrend { get; set; } = "Not available"; // Positive, Negative, Fluctuating
    public string ProfitabilityVerdict { get; set; } = string.Empty;
    public string SolvencyVerdict { get; set; } = string.Empty;
}

public class CompanyFinancialReportDto
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public IReadOnlyList<FinancialYearDto> Years { get; set; } = Array.Empty<FinancialYearDto>();
    public FinancialGrowthDto GrowthAnalysis { get; set; } = new();
}
