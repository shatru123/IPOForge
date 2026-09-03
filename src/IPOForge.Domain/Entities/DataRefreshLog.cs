using IPOForge.Domain.Common;

namespace IPOForge.Domain.Entities;

public class DataRefreshLog : BaseEntity
{
    public string TriggerType { get; set; } = "Scheduled"; // Scheduled, ManualAdmin, HttpTrigger
    public string Status { get; set; } = "Completed"; // InProgress, Completed, Failed
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int RecordsProcessed { get; set; } = 0;
    public string? ErrorMessage { get; set; }
    public string DetailsJson { get; set; } = "{}";
}
