using FluentAssertions;
using IPOForge.Application.Services;
using IPOForge.Domain.Entities;
using Xunit;

namespace IPOForge.UnitTests;

public class FinancialCalculationTests
{
    private readonly FinancialAnalysisEngine _engine = new();

    [Fact]
    public void AnalyzeFinancials_Calculates_Margins_And_MultiYear_CAGR()
    {
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = "SolarTech Systems",
            Sector = "Energy",
            Industry = "Solar",
            Financials = new List<CompanyFinancial>
            {
                new()
                {
                    FiscalYear = "FY2022",
                    PeriodEnding = new DateTime(2022, 3, 31),
                    Revenue = 100.0m,
                    EBITDA = 20.0m,
                    EBIT = 18.0m,
                    PAT = 10.0m,
                    EPS = 5.0m,
                    TotalAssets = 150.0m,
                    TotalDebt = 30.0m,
                    NetWorth = 80.0m,
                    CurrentAssets = 60.0m,
                    CurrentLiabilities = 30.0m,
                    OperatingCashFlow = 15.0m
                },
                new()
                {
                    FiscalYear = "FY2023",
                    PeriodEnding = new DateTime(2023, 3, 31),
                    Revenue = 150.0m,
                    EBITDA = 35.0m,
                    EBIT = 32.0m,
                    PAT = 20.0m,
                    EPS = 10.0m,
                    TotalAssets = 220.0m,
                    TotalDebt = 40.0m,
                    NetWorth = 120.0m,
                    CurrentAssets = 90.0m,
                    CurrentLiabilities = 45.0m,
                    OperatingCashFlow = 25.0m
                },
                new()
                {
                    FiscalYear = "FY2024",
                    PeriodEnding = new DateTime(2024, 3, 31),
                    Revenue = 225.0m,
                    EBITDA = 55.0m,
                    EBIT = 50.0m,
                    PAT = 35.0m,
                    EPS = 17.5m,
                    TotalAssets = 320.0m,
                    TotalDebt = 35.0m,
                    NetWorth = 180.0m,
                    CurrentAssets = 140.0m,
                    CurrentLiabilities = 55.0m,
                    OperatingCashFlow = 40.0m
                }
            }
        };

        var result = _engine.AnalyzeFinancials(company);

        result.Years.Should().HaveCount(3);

        var latestYear = result.Years.Last();
        latestYear.EbitdaMargin.Should().Be(24.44m); // 55 / 225 * 100
        latestYear.PatMargin.Should().Be(15.56m);    // 35 / 225 * 100
        latestYear.DebtToEquity.Should().Be(0.19m);  // 35 / 180
        latestYear.CurrentRatio.Should().Be(2.55m);  // 140 / 55

        // CAGR 2 years from 100 to 225 => sqrt(2.25) - 1 = 1.5 - 1 = 50%
        result.GrowthAnalysis.RevenueCagr3Year.Should().Be(50.00m);
        result.GrowthAnalysis.CashFlowTrend.Should().Be("Consistently Positive");
    }
}
