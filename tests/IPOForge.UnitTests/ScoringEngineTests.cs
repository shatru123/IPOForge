using FluentAssertions;
using IPOForge.Application.Services;
using IPOForge.Contracts.Analysis;
using IPOForge.Contracts.Financial;
using IPOForge.Contracts.Gmp;
using IPOForge.Contracts.Subscription;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;
using Xunit;

namespace IPOForge.UnitTests;

public class ScoringEngineTests
{
    private readonly IpoScoringEngine _engine = new();

    [Fact]
    public void CalculateScore_Returns_High_Scores_And_Pillar_Breakdown_For_High_Quality_IPO()
    {
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = "Waaree Solar",
            Sector = "Energy",
            PromoterHoldingPreIssue = 75.0m
        };

        var ipo = new IPO
        {
            Id = Guid.NewGuid(),
            Name = "Waaree Solar IPO",
            Company = company,
            IpoType = IpoType.Mainboard,
            PriceBandHigh = 1500.0m,
            IssueSize = 4000.0m,
            FreshIssueAmount = 3500.0m,
            OFSAmount = 500.0m
        };

        var financials = new CompanyFinancialReportDto
        {
            GrowthAnalysis = new FinancialGrowthDto
            {
                RevenueCagr3Year = 65.0m,
                ProfitCagr3Year = 80.0m,
                RevenueGrowthYoY = 70.0m,
                ProfitGrowthYoY = 90.0m,
                ProfitabilityVerdict = "Exceptional Growth (>25% 3-Year CAGR)"
            },
            Years = new List<FinancialYearDto>
            {
                new()
                {
                    FiscalYear = "FY2024",
                    EbitdaMargin = 22.0m,
                    ROE = 28.0m,
                    DebtToEquity = 0.15m,
                    OperatingCashFlow = 1200.0m,
                    FreeCashFlow = 800.0m
                }
            }
        };

        var valuation = new ValuationAnalysisDto
        {
            Classification = ValuationClassification.Attractive,
            CompanyPE = 28.0m,
            IndustryPE = 40.0m
        };

        var gmp = new GmpHistoryDto
        {
            CurrentGmp = 1200.0m,
            CurrentGmpPercentage = 80.0m,
            Trend = GmpTrend.StronglyIncreasing
        };

        var subscription = new SubscriptionBreakdownDto
        {
            LatestTotalSubscription = 75.0m,
            LatestQibSubscription = 150.0m,
            LatestRetailSubscription = 12.0m,
            LatestNiiSubscription = 60.0m
        };

        var score = _engine.CalculateScore(ipo, financials, valuation, gmp, subscription, Array.Empty<IPORisk>());

        score.ListingGainScore.Should().BeInRange(80, 100);
        score.ListingRecommendation.Should().Be(RecommendationRating.Strong);
        score.ListingGainPillars.Should().HaveCount(8);

        score.LongTermScore.Should().BeInRange(80, 100);
        score.LongTermRecommendation.Should().Be(RecommendationRating.Strong);
        score.LongTermPillars.Should().HaveCount(11);

        score.ListingGainVerdict.Should().NotBeNullOrWhiteSpace();
        score.LongTermVerdict.Should().NotBeNullOrWhiteSpace();
        score.UnifiedAnalyticalConclusion.Should().NotBeNullOrWhiteSpace();
    }
}
