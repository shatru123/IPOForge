using IPOForge.Domain.Common;

namespace IPOForge.Domain.Entities;

public class NotificationPreference : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public bool EmailNotificationsEnabled { get; set; } = true;
    public decimal GmpAlertThresholdPercent { get; set; } = 10.0m;
    public decimal SubscriptionCrossThresholdX { get; set; } = 10.0m;
    public bool NotifyOnListingDate { get; set; } = true;
    public bool NotifyOnAllotmentDate { get; set; } = true;
}
