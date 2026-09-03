using IPOForge.Contracts.Gmp;
using IPOForge.Contracts.Ipo;

namespace IPOForge.Contracts.Dashboard;

public class DashboardSummaryDto
{
    // High-Level Market Statistics
    public int TotalIposThisYear { get; set; }
    public int MainboardIposCount { get; set; }
    public int SmeIposCount { get; set; }
    public decimal AverageGmpPercent { get; set; }
    public decimal AverageSubscriptionX { get; set; }
    public decimal AverageListingGainPercent { get; set; }

    // Tabular / List Groupings
    public IReadOnlyList<IpoSummaryDto> OpenIpos { get; set; } = Array.Empty<IpoSummaryDto>();
    public IReadOnlyList<IpoSummaryDto> UpcomingIpos { get; set; } = Array.Empty<IpoSummaryDto>();
    public IReadOnlyList<IpoSummaryDto> ClosingTodayIpos { get; set; } = Array.Empty<IpoSummaryDto>();
    public IReadOnlyList<IpoSummaryDto> RecentlyListedIpos { get; set; } = Array.Empty<IpoSummaryDto>();

    // Dynamic Movers & Analytical Highlights
    public IReadOnlyList<GmpMoverDto> TopGmpGainers { get; set; } = Array.Empty<GmpMoverDto>();
    public IReadOnlyList<GmpMoverDto> TopGmpLosers { get; set; } = Array.Empty<GmpMoverDto>();
    public IReadOnlyList<IpoSummaryDto> HighPotentialListingIpos { get; set; } = Array.Empty<IpoSummaryDto>();
    public IReadOnlyList<IpoSummaryDto> HighPotentialLongTermIpos { get; set; } = Array.Empty<IpoSummaryDto>();
    public IReadOnlyList<IpoSummaryDto> HighRiskIpos { get; set; } = Array.Empty<IpoSummaryDto>();
}
