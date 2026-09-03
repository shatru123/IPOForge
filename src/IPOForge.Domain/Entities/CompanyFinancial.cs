using IPOForge.Domain.Common;

namespace IPOForge.Domain.Entities;

public class CompanyFinancial : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public string FiscalYear { get; set; } = string.Empty; // e.g. "FY2023", "FY2024"
    public DateTime PeriodEnding { get; set; }
    public bool IsAudited { get; set; } = true;

    // Financial Metrics in ₹ Crores
    public decimal? Revenue { get; set; }
    public decimal? EBITDA { get; set; }
    public decimal? EBIT { get; set; }
    public decimal? PAT { get; set; } // Profit After Tax
    public decimal? EPS { get; set; } // Earnings Per Share (₹)
    public decimal? OperatingCashFlow { get; set; }
    public decimal? FreeCashFlow { get; set; }
    public decimal? TotalAssets { get; set; }
    public decimal? TotalDebt { get; set; }
    public decimal? NetWorth { get; set; }
    public decimal? CurrentAssets { get; set; }
    public decimal? CurrentLiabilities { get; set; }

    // Calculated fields (stored for high-performance retrieval)
    public decimal? EbitdaMargin { get; set; }
    public decimal? PatMargin { get; set; }
    public decimal? ROE { get; set; } // Return on Equity %
    public decimal? ROCE { get; set; } // Return on Capital Employed %
    public decimal? DebtToEquity { get; set; }
    public decimal? CurrentRatio { get; set; }
}
