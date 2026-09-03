using IPOForge.Domain.Enums;

namespace IPOForge.Contracts.Admin;

public class DataRefreshRequest
{
    public bool RefreshGmpOnly { get; set; } = false;
    public bool ForceFullSync { get; set; } = false;
}

public class DataRefreshStatusDto
{
    public Guid LogId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int RecordsProcessed { get; set; }
    public string? ErrorMessage { get; set; }
    public string Details { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class DataRefreshLogDto
{
    public Guid Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int RecordsUpdated { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public DateTime ExecutedAt { get; set; }
}

public class DataSourceStatusDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public DataSourceType SourceType { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public int ErrorCount { get; set; }
}

public class AdminSyncStatusDto
{
    public DateTime? LastSyncTime { get; set; }
    public IReadOnlyList<DataSourceStatusDto> DataSources { get; set; } = Array.Empty<DataSourceStatusDto>();
    public IReadOnlyList<DataRefreshLogDto> RecentLogs { get; set; } = Array.Empty<DataRefreshLogDto>();
}
