using IPOForge.Domain.Common;
using IPOForge.Domain.Enums;

namespace IPOForge.Domain.Entities;

public class DataSource : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public DataSourceType SourceType { get; set; } = DataSourceType.MarketAggregator;
    public string? BaseUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastSyncAt { get; set; }
    public string HealthStatus { get; set; } = "Operational"; // Operational, Degraded, Offline
    public int ErrorCount { get; set; } = 0;
}
