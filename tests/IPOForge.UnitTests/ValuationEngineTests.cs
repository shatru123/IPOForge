using FluentAssertions;
using IPOForge.Application.Services;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;
using Xunit;

namespace IPOForge.UnitTests;

public class ValuationEngineTests
{
    private readonly ValuationEngine _engine = new();

    [Fact]
    public void EvaluateValuation_Correctly_Calculates_PE_And_Assigns_Attractive_Classification()
    {
        var ipo = new IPO
        {
            Id = Guid.NewGuid(),
            PriceBandHigh = 300.0m
        };

        var latestFinancial = new CompanyFinancial
        {
            EPS = 15.0m, // P/E = 300 / 15 = 20.0x
            PAT = 100.0m,
            EBITDA = 150.0m
        };

        var industry = new IndustryMetric
        {
            Sector = "Technology",
            Industry = "Software",
            MedianPE = 40.0m // 20.0x vs 40.0x is 50% discount -> Attractive
        };

        var result = _engine.EvaluateValuation(ipo, latestFinancial, industry);

        result.CompanyPE.Should().Be(20.0m);
        result.IndustryPE.Should().Be(40.0m);
        result.Classification.Should().Be(ValuationClassification.Attractive);
        result.ValuationDiscountOrPremiumPercent.Should().Be(-50.0m);
    }

    [Fact]
    public void EvaluateValuation_Assigns_VeryExpensive_When_PE_Exceeds_1_5x_Industry()
    {
        var ipo = new IPO
        {
            Id = Guid.NewGuid(),
            PriceBandHigh = 800.0m
        };

        var latestFinancial = new CompanyFinancial
        {
            EPS = 10.0m // P/E = 80.0x
        };

        var industry = new IndustryMetric
        {
            Sector = "Retail",
            Industry = "Consumer",
            MedianPE = 35.0m // 80.0x vs 35.0x -> Very Expensive
        };

        var result = _engine.EvaluateValuation(ipo, latestFinancial, industry);

        result.CompanyPE.Should().Be(80.0m);
        result.Classification.Should().Be(ValuationClassification.VeryExpensive);
    }
}
