using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Analysis;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;

namespace IPOForge.Application.Services;

public class ValuationEngine : IValuationEngine
{
    public ValuationAnalysisDto EvaluateValuation(IPO ipo, CompanyFinancial? latestFinancial, IndustryMetric? industryMetric)
    {
        var result = new ValuationAnalysisDto
        {
            IpoId = ipo.Id,
            Classification = ValuationClassification.Reasonable
        };

        if (latestFinancial == null)
        {
            result.SummaryText = "Financial statement data is not yet available to compute valuation multiples.";
            return result;
        }

        // Calculate Company P/E
        if (latestFinancial.EPS.HasValue && latestFinancial.EPS.Value > 0 && ipo.PriceBandHigh > 0)
        {
            result.EPS = latestFinancial.EPS.Value;
            result.CompanyPE = Math.Round(ipo.PriceBandHigh / latestFinancial.EPS.Value, 2);
        }

        // Calculate P/B
        if (latestFinancial.NetWorth.HasValue && latestFinancial.NetWorth.Value > 0 && ipo.PriceBandHigh > 0)
        {
            // Estimate Book value per share (assuming Face Value and Net Worth)
            // Or if PAT and ROE exist: BVPS = EPS / (ROE / 100)
            if (latestFinancial.ROE.HasValue && latestFinancial.ROE.Value > 0 && result.EPS.HasValue)
            {
                var bvps = result.EPS.Value / (latestFinancial.ROE.Value / 100.0m);
                result.CompanyPB = Math.Round(ipo.PriceBandHigh / bvps, 2);
            }
        }

        // EV/EBITDA
        if (latestFinancial.EBITDA.HasValue && latestFinancial.EBITDA.Value > 0 && ipo.IssueSize > 0)
        {
            var estimatedMarketCap = ipo.PriceBandHigh > 0 && result.EPS.HasValue && latestFinancial.PAT.HasValue && latestFinancial.PAT.Value > 0
                ? (ipo.PriceBandHigh / result.EPS.Value) * latestFinancial.PAT.Value
                : ipo.IssueSize * 3; // Approx proxy if full share count not given

            var enterpriseValue = estimatedMarketCap + (latestFinancial.TotalDebt ?? 0) - ((latestFinancial.CurrentAssets ?? 0) * 0.3m);
            if (enterpriseValue > 0)
            {
                result.CompanyEvEbitda = Math.Round(enterpriseValue / latestFinancial.EBITDA.Value, 2);
            }
        }

        // Industry Benchmarking
        if (industryMetric != null && industryMetric.MedianPE > 0)
        {
            result.IndustryPE = industryMetric.MedianPE;
            result.IndustryPB = industryMetric.MedianPB;

            if (result.CompanyPE.HasValue)
            {
                var ratio = result.CompanyPE.Value / industryMetric.MedianPE;
                var discountOrPremium = Math.Round(((result.CompanyPE.Value - industryMetric.MedianPE) / industryMetric.MedianPE) * 100, 2);
                result.ValuationDiscountOrPremiumPercent = discountOrPremium;

                if (ratio <= 0.85m)
                {
                    result.Classification = ValuationClassification.Attractive;
                    result.SummaryText = $"Attractively valued at {result.CompanyPE.Value}x P/E, a {Math.Abs(discountOrPremium)}% discount to the industry median of {industryMetric.MedianPE}x.";
                }
                else if (ratio <= 1.15m)
                {
                    result.Classification = ValuationClassification.Reasonable;
                    result.SummaryText = $"Fairly/Reasonably valued at {result.CompanyPE.Value}x P/E, closely aligned with the industry median of {industryMetric.MedianPE}x ({discountOrPremium:+0.0;-0.0}%).";
                }
                else if (ratio <= 1.50m)
                {
                    result.Classification = ValuationClassification.Premium;
                    result.SummaryText = $"Trading at a noticeable premium of {discountOrPremium}% ({result.CompanyPE.Value}x vs industry median {industryMetric.MedianPE}x). Requires strong margin execution.";
                }
                else
                {
                    result.Classification = ValuationClassification.VeryExpensive;
                    result.SummaryText = $"Aggressively priced at {result.CompanyPE.Value}x P/E ({discountOrPremium}% above industry peer median {industryMetric.MedianPE}x). Leaves little margin of safety for retail investors.";
                }
            }
            else
            {
                result.SummaryText = $"Industry median P/E is {industryMetric.MedianPE}x. Company P/E could not be calculated due to negative/missing EPS.";
            }
        }
        else
        {
            result.SummaryText = result.CompanyPE.HasValue
                ? $"Company P/E is {result.CompanyPE.Value}x. Sector-wide peer benchmark is unavailable."
                : "Valuation metrics are not fully available.";
        }

        return result;
    }
}
