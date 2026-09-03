using IPOForge.Domain.Common;

namespace IPOForge.Domain.Entities;

public class IPOSubscriptionHistory : AuditableEntity
{
    public Guid IpoId { get; set; }
    public IPO IPO { get; set; } = null!;

    public decimal RetailSubscription { get; set; } // e.g. 3.45x
    public decimal QibSubscription { get; set; } // Qualified Institutional Buyers
    public decimal NiiSubscription { get; set; } // Non-Institutional / HNI
    public decimal? EmployeeSubscription { get; set; }
    public decimal? ShareholderSubscription { get; set; }
    public decimal TotalSubscription { get; set; } // e.g. 12.8x

    public DateTime SnapshotDate { get; set; }
    public int DayNumber { get; set; } = 1; // Day 1, Day 2, Day 3
    public string Source { get; set; } = "Exchange Live Data";
}
