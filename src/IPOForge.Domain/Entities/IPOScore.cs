using IPOForge.Domain.Common;
using IPOForge.Domain.Enums;

namespace IPOForge.Domain.Entities;

public class IPOScore : AuditableEntity
{
    public Guid IpoId { get; set; }
    public IPO IPO { get; set; } = null!;

    // Listing Gain Score (0 - 100)
    public int ListingGainScore { get; set; }
    public RecommendationRating ListingRecommendation { get; set; }
    public string ListingGainVerdict { get; set; } = string.Empty;

    // Long-Term Score (0 - 100)
    public int LongTermScore { get; set; }
    public RecommendationRating LongTermRecommendation { get; set; }
    public string LongTermVerdict { get; set; } = string.Empty;

    // Complete breakdown stored as JSON for instant recall
    public string BreakdownJson { get; set; } = "{}";

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
