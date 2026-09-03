using IPOForge.Domain.Common;
using IPOForge.Domain.Enums;

namespace IPOForge.Domain.Entities;

public class IPORisk : AuditableEntity
{
    public Guid IpoId { get; set; }
    public IPO IPO { get; set; } = null!;

    public string Category { get; set; } = string.Empty; // e.g. "HIGH DEBT", "VALUATION RISK", "CUSTOMER CONCENTRATION"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RiskSeverity Severity { get; set; } = RiskSeverity.Medium;
    public string? TraceableMetric { get; set; } // e.g. "Debt/Equity = 2.45x (High)"
    public string? MitigationDetails { get; set; }
}
