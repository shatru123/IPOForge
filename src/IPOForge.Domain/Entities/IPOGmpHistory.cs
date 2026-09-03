using IPOForge.Domain.Common;

namespace IPOForge.Domain.Entities;

public class IPOGmpHistory : AuditableEntity
{
    public Guid IpoId { get; set; }
    public IPO IPO { get; set; } = null!;

    public decimal GMP { get; set; } // ₹ GMP premium per share
    public decimal GMPPercentage { get; set; } // (GMP / PriceBandHigh) * 100
    public decimal EstimatedListingPrice { get; set; } // PriceBandHigh + GMP
    public decimal? KostakRate { get; set; } // ₹ per application
    public decimal? SubjectToSauda { get; set; } // ₹ per lot

    public string Source { get; set; } = "Market Aggregator";
    public DateTime ObservedAt { get; set; }
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
    public decimal Confidence { get; set; } = 1.0m; // 0.0 - 1.0
}
