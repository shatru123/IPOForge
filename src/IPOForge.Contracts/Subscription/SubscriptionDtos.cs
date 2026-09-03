namespace IPOForge.Contracts.Subscription;

public class SubscriptionSnapshotDto
{
    public Guid Id { get; set; }
    public Guid IpoId { get; set; }
    public decimal RetailSubscription { get; set; }
    public decimal QibSubscription { get; set; }
    public decimal NiiSubscription { get; set; }
    public decimal? EmployeeSubscription { get; set; }
    public decimal? ShareholderSubscription { get; set; }
    public decimal TotalSubscription { get; set; }
    public DateTime SnapshotDate { get; set; }
    public int DayNumber { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class SubscriptionBreakdownDto
{
    public Guid IpoId { get; set; }
    public string IpoName { get; set; } = string.Empty;
    public decimal LatestTotalSubscription { get; set; }
    public decimal LatestQibSubscription { get; set; }
    public decimal LatestNiiSubscription { get; set; }
    public decimal LatestRetailSubscription { get; set; }
    public decimal? LatestEmployeeSubscription { get; set; }
    public decimal? LatestShareholderSubscription { get; set; }
    public string DemandQualityVerdict { get; set; } = string.Empty; // e.g. "Strong Institutional Backing (QIB > 20x)"
    public IReadOnlyList<SubscriptionSnapshotDto> History { get; set; } = Array.Empty<SubscriptionSnapshotDto>();
}
