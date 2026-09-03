using IPOForge.Domain.Common;

namespace IPOForge.Domain.Entities;

public class Watchlist : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid IpoId { get; set; }
    public IPO IPO { get; set; } = null!;

    public string? Notes { get; set; }
    public bool AlertOnGmpChange { get; set; } = true;
    public bool AlertOnSubscriptionCross { get; set; } = true;
    public decimal? TargetGmpPercent { get; set; }
}
