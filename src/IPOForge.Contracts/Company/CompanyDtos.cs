using IPOForge.Contracts.Financial;
using IPOForge.Contracts.Ipo;

namespace IPOForge.Contracts.Company;

public class CompanySummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string? CIN { get; set; }
    public string? Symbol { get; set; }
    public string Sector { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Website { get; set; }
    public int? FoundedYear { get; set; }
    public string? Headquarters { get; set; }
    public string? PromoterInformation { get; set; }
    public decimal? LatestRevenue { get; set; }
    public decimal? LatestPAT { get; set; }
    public decimal? LatestROE { get; set; }
}

public class CompanyDetailDto : CompanySummaryDto
{
    public string? ManagingDirector { get; set; }
    public decimal? PromoterHoldingPreIssue { get; set; }
    public decimal? PromoterHoldingPostIssue { get; set; }
    public IReadOnlyList<FinancialYearDto> Financials { get; set; } = Array.Empty<FinancialYearDto>();
    public FinancialGrowthDto Growth { get; set; } = new();
    public IReadOnlyList<IpoSummaryDto> AssociatedIpos { get; set; } = Array.Empty<IpoSummaryDto>();
}
