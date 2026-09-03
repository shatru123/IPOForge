using IPOForge.Contracts.Ipo;

namespace IPOForge.Contracts.Watchlist;

public class WatchlistItemDto
{
    public Guid Id { get; set; }
    public Guid IpoId { get; set; }
    public IpoSummaryDto Ipo { get; set; } = new();
    public string? Notes { get; set; }
    public bool AlertOnGmpChange { get; set; }
    public bool AlertOnSubscriptionCross { get; set; }
    public decimal? TargetGmpPercent { get; set; }
    public DateTime AddedAt { get; set; }
}

public class AddToWatchlistRequest
{
    public Guid IpoId { get; set; }
    public string? Notes { get; set; }
    public bool AlertOnGmpChange { get; set; } = true;
    public bool AlertOnSubscriptionCross { get; set; } = true;
    public decimal? TargetGmpPercent { get; set; }
}

public class UpdateWatchlistRequest
{
    public string? Notes { get; set; }
    public bool AlertOnGmpChange { get; set; }
    public bool AlertOnSubscriptionCross { get; set; }
    public decimal? TargetGmpPercent { get; set; }
}
