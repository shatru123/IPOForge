using IPOForge.Domain.Common;

namespace IPOForge.Domain.Entities;

public class Company : AuditableEntity
{
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
    public string? ManagingDirector { get; set; }
    public decimal? PromoterHoldingPreIssue { get; set; }
    public decimal? PromoterHoldingPostIssue { get; set; }

    public ICollection<CompanyFinancial> Financials { get; set; } = new List<CompanyFinancial>();
    public ICollection<IPO> Ipos { get; set; } = new List<IPO>();
}
