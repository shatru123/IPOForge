using FluentAssertions;
using IPOForge.Application.Services;
using IPOForge.Contracts.Gmp;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;
using Xunit;

namespace IPOForge.UnitTests;

public class RiskEngineTests
{
    private readonly RiskEngine _engine = new();

    [Fact]
    public void EvaluateRisks_Flags_High_Debt_And_Heavy_OFS()
    {
        var ipo = new IPO
        {
            Id = Guid.NewGuid(),
            Name = "Leveraged OFS IPO",
            PriceBandHigh = 100.0m,
            IssueSize = 1000.0m,
            FreshIssueAmount = 100.0m,
            OFSAmount = 900.0m, // 90% OFS
            Risks = new List<IPORisk>()
        };

        var financials = new List<CompanyFinancial>
        {
            new()
            {
                FiscalYear = "FY2024",
                PeriodEnding = new DateTime(2024, 3, 31),
                Revenue = 500.0m,
                PAT = 20.0m,
                TotalDebt = 350.0m,
                NetWorth = 100.0m, // Debt/Equity = 3.5x
                OperatingCashFlow = -50.0m // Negative operating cash flow
            }
        };

        var gmp = new GmpHistoryDto
        {
            Trend = GmpTrend.StronglyDeclining,
            Gmp24hChange = -20.0m,
            Gmp3dChange = -35.0m
        };

        var result = _engine.EvaluateRisks(ipo, financials, null, gmp);

        result.OverallRiskLevel.Should().Be("High");
        result.HighRiskCount.Should().BeGreaterThan(0);
        result.IdentifiedRisks.Should().Contain(r => r.Category.Contains("DEBT"));
        result.IdentifiedRisks.Should().Contain(r => r.Category.Contains("CASH FLOW"));
        result.IdentifiedRisks.Should().Contain(r => r.Category.Contains("STRUCTURE"));
    }
}
