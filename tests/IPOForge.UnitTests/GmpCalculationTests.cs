using FluentAssertions;
using IPOForge.Application.Services;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;
using Xunit;

namespace IPOForge.UnitTests;

public class GmpCalculationTests
{
    private readonly GmpAnalyticsService _service = new();

    [Fact]
    public void AnalyzeGmpHistory_Calculates_Exact_Percentage_And_Estimated_Listing_Price()
    {
        // Example from spec: Issue Price = ₹429, GMP = ₹250 => GMP % ≈ 58.28%, Estimated Listing = ₹679
        var ipo = new IPO
        {
            Id = Guid.NewGuid(),
            Name = "Test IPO",
            PriceBandHigh = 429.0m,
            GmpHistories = new List<IPOGmpHistory>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    GMP = 250.0m,
                    ObservedAt = DateTime.UtcNow,
                    RetrievedAt = DateTime.UtcNow
                }
            }
        };

        var result = _service.AnalyzeGmpHistory(ipo);

        result.CurrentGmp.Should().Be(250.0m);
        result.EstimatedListingPrice.Should().Be(679.0m);
        result.CurrentGmpPercentage.Should().Be(58.28m);
    }

    [Fact]
    public void AnalyzeGmpHistory_Calculates_24h_And_3d_Deltas_And_Determines_Trend()
    {
        var now = DateTime.UtcNow;
        var ipo = new IPO
        {
            Id = Guid.NewGuid(),
            Name = "Trending Solar IPO",
            PriceBandHigh = 100.0m,
            GmpHistories = new List<IPOGmpHistory>
            {
                new() { GMP = 20.0m, ObservedAt = now.AddDays(-4) },
                new() { GMP = 30.0m, ObservedAt = now.AddDays(-3) },
                new() { GMP = 55.0m, ObservedAt = now.AddDays(-1) },
                new() { GMP = 70.0m, ObservedAt = now }
            }
        };

        var result = _service.AnalyzeGmpHistory(ipo);

        result.CurrentGmp.Should().Be(70.0m);
        result.HighestGmp.Should().Be(70.0m);
        result.LowestGmp.Should().Be(20.0m);
        result.Gmp24hChange.Should().Be(15.0m);
        result.Gmp3dChange.Should().Be(40.0m);
        result.Trend.Should().Be(GmpTrend.StronglyIncreasing);
    }

    [Fact]
    public void GetAccuracyAnalytics_Computes_Prediction_Errors_Correctly()
    {
        var listedIpos = new List<IPO>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Tata Tech",
                Status = IpoStatus.Listed,
                PriceBandHigh = 500.0m,
                ListingPrice = 1200.0m, // 140% actual gain
                ListingDate = DateTime.UtcNow.AddDays(-10),
                GmpHistories = new List<IPOGmpHistory>
                {
                    new() { GMP = 450.0m, ObservedAt = DateTime.UtcNow.AddDays(-11) } // 90% predicted gain
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Bajaj HFL",
                Status = IpoStatus.Listed,
                PriceBandHigh = 70.0m,
                ListingPrice = 150.0m, // 114.3% actual gain
                ListingDate = DateTime.UtcNow.AddDays(-20),
                GmpHistories = new List<IPOGmpHistory>
                {
                    new() { GMP = 75.0m, ObservedAt = DateTime.UtcNow.AddDays(-21) } // 107.1% predicted gain
                }
            }
        };

        var analytics = _service.GetAccuracyAnalytics(listedIpos);

        analytics.TotalListedIposAnalyzed.Should().Be(2);
        analytics.HistoricalComparison.Should().HaveCount(2);
        analytics.Within20PercentAccuracyRate.Should().BeGreaterThan(0);
    }
}
