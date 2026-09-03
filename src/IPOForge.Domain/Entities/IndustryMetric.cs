using IPOForge.Domain.Common;

namespace IPOForge.Domain.Entities;

public class IndustryMetric : AuditableEntity
{
    public string Sector { get; set; } = string.Empty;
    public string Industry { get; set; } = string.Empty;

    public decimal MedianPE { get; set; }
    public decimal MedianPB { get; set; }
    public decimal? MedianROE { get; set; }
    public decimal? MedianROCE { get; set; }
    public decimal? MedianDebtEquity { get; set; }
    public decimal? MedianNetMargin { get; set; }
    public int ObservedYear { get; set; } = 2024;
}
