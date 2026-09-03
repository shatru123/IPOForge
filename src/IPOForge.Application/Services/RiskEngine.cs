using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Analysis;
using IPOForge.Contracts.Gmp;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;

namespace IPOForge.Application.Services;

public class RiskEngine : IRiskEngine
{
    public RiskAnalysisDto EvaluateRisks(IPO ipo, IReadOnlyList<CompanyFinancial> financials, IndustryMetric? industryMetric, GmpHistoryDto? gmpHistory)
    {
        var riskItems = new List<RiskItemDto>();
        var sortedFinancials = financials.OrderBy(f => f.PeriodEnding).ToList();
        var latestFin = sortedFinancials.LastOrDefault();
        var prevFin = sortedFinancials.Count >= 2 ? sortedFinancials[^2] : null;

        // 1. Debt & Solvency Risk
        if (latestFin?.TotalDebt.HasValue == true && latestFin.NetWorth.HasValue == true && latestFin.NetWorth.Value > 0)
        {
            var de = latestFin.TotalDebt.Value / latestFin.NetWorth.Value;
            if (de > 2.0m)
            {
                riskItems.Add(new RiskItemDto
                {
                    Id = Guid.NewGuid(),
                    Category = "HIGH DEBT & SOLVENCY",
                    Title = "Elevated Leverage (Debt/Equity > 2.0x)",
                    Description = $"The company operates with a high Debt-to-Equity ratio of {de:F2}x, increasing interest payment burdens and vulnerabilities during economic downturns.",
                    Severity = RiskSeverity.High,
                    TraceableMetric = $"Debt/Equity = {de:F2}x"
                });
            }
            else if (de > 1.2m)
            {
                riskItems.Add(new RiskItemDto
                {
                    Id = Guid.NewGuid(),
                    Category = "MODERATE DEBT",
                    Title = "Noticeable Debt Burden",
                    Description = $"Debt-to-Equity ratio stands at {de:F2}x. Investors should check if IPO fresh issue proceeds are allocated to debt repayment.",
                    Severity = RiskSeverity.Medium,
                    TraceableMetric = $"Debt/Equity = {de:F2}x"
                });
            }
        }

        // 2. Valuation Risk
        if (latestFin?.EPS.HasValue == true && latestFin.EPS.Value > 0 && ipo.PriceBandHigh > 0 && industryMetric != null && industryMetric.MedianPE > 0)
        {
            var companyPe = ipo.PriceBandHigh / latestFin.EPS.Value;
            if (companyPe > industryMetric.MedianPE * 1.5m)
            {
                riskItems.Add(new RiskItemDto
                {
                    Id = Guid.NewGuid(),
                    Category = "VALUATION RISK",
                    Title = "Rich Valuation vs Industry Peers",
                    Description = $"IPO is asking a P/E multiple of {companyPe:F1}x compared to the industry median of {industryMetric.MedianPE:F1}x (+{((companyPe - industryMetric.MedianPE) / industryMetric.MedianPE) * 100:F0}%). Leaves limited safety margin.",
                    Severity = RiskSeverity.High,
                    TraceableMetric = $"IPO P/E: {companyPe:F1}x vs Industry: {industryMetric.MedianPE:F1}x"
                });
            }
        }

        // 3. Negative Cash Flow Risk
        if (latestFin?.OperatingCashFlow.HasValue == true && latestFin.OperatingCashFlow.Value < 0)
        {
            riskItems.Add(new RiskItemDto
            {
                Id = Guid.NewGuid(),
                Category = "CASH FLOW RISK",
                Title = "Negative Operating Cash Flow",
                Description = $"Operating cash flow in the latest fiscal year was negative (₹{Math.Abs(latestFin.OperatingCashFlow.Value):F1} Cr), indicating that revenue is tied up in working capital or operational outlays.",
                Severity = RiskSeverity.High,
                TraceableMetric = $"Operating Cash Flow = ₹{latestFin.OperatingCashFlow.Value:F1} Cr"
            });
        }

        // 4. Margin Contraction Risk
        if (latestFin?.PAT.HasValue == true && latestFin.Revenue.HasValue == true && latestFin.Revenue.Value > 0 &&
            prevFin?.PAT.HasValue == true && prevFin.Revenue.HasValue == true && prevFin.Revenue.Value > 0)
        {
            var latestMargin = (latestFin.PAT.Value / latestFin.Revenue.Value) * 100;
            var prevMargin = (prevFin.PAT.Value / prevFin.Revenue.Value) * 100;
            if (latestMargin < prevMargin - 3.0m)
            {
                riskItems.Add(new RiskItemDto
                {
                    Id = Guid.NewGuid(),
                    Category = "PROFITABILITY RISK",
                    Title = "Net Margin Contraction",
                    Description = $"Net Profit Margin compressed from {prevMargin:F1}% in previous fiscal year to {latestMargin:F1}% in the latest year.",
                    Severity = RiskSeverity.Medium,
                    TraceableMetric = $"Margin drop: {prevMargin:F1}% -> {latestMargin:F1}%"
                });
            }
        }

        // 5. Heavy Offer For Sale (OFS) Risk
        if (ipo.IssueSize > 0 && ipo.OFSAmount > 0)
        {
            var ofsPercent = (ipo.OFSAmount / ipo.IssueSize) * 100;
            if (ofsPercent >= 75.0m)
            {
                riskItems.Add(new RiskItemDto
                {
                    Id = Guid.NewGuid(),
                    Category = "ISSUE STRUCTURE",
                    Title = "High OFS Concentration (>75%)",
                    Description = $"{ofsPercent:F1}% of IPO proceeds (₹{ipo.OFSAmount:F1} Cr) are being sold by existing shareholders/promoters, and will not flow into company growth/operations.",
                    Severity = RiskSeverity.Medium,
                    TraceableMetric = $"OFS Share = {ofsPercent:F1}%"
                });
            }
        }

        // 6. GMP Trend Risk
        if (gmpHistory != null && (gmpHistory.Trend == GmpTrend.Declining || gmpHistory.Trend == GmpTrend.StronglyDeclining))
        {
            riskItems.Add(new RiskItemDto
            {
                Id = Guid.NewGuid(),
                Category = "GMP MOMENTUM RISK",
                Title = "Declining Grey Market Premium",
                Description = $"Grey Market Premium has weakened recently ({gmpHistory.Gmp24hChange:+0;-0} in 24h, {gmpHistory.Gmp3dChange:+0;-0} in 3d), signaling cooling sentiment among grey market participants.",
                Severity = RiskSeverity.Medium,
                TraceableMetric = $"Trend: {gmpHistory.Trend}, 3d change: {gmpHistory.Gmp3dChange} pts"
            });
        }

        // 7. Liquidity / SME Risk
        if (ipo.IpoType == IpoType.Sme)
        {
            riskItems.Add(new RiskItemDto
            {
                Id = Guid.NewGuid(),
                Category = "SME PLATFORM RISK",
                Title = "SME Exchange Liquidity & High Lot Sizing",
                Description = "SME issues carry higher lot size constraints (usually >₹1 Lakh minimum investment) and typically feature lower post-listing liquidity compared to Mainboard equities.",
                Severity = RiskSeverity.Low,
                TraceableMetric = $"Lot Size: {ipo.LotSize} shares (₹{ipo.MinimumInvestment:N0})"
            });
        }

        // Include any explicit DB risks configured on the IPO
        foreach (var dbRisk in ipo.Risks)
        {
            if (!riskItems.Any(r => r.Title.Equals(dbRisk.Title, StringComparison.OrdinalIgnoreCase)))
            {
                riskItems.Add(new RiskItemDto
                {
                    Id = dbRisk.Id,
                    Category = dbRisk.Category,
                    Title = dbRisk.Title,
                    Description = dbRisk.Description,
                    Severity = dbRisk.Severity,
                    TraceableMetric = dbRisk.TraceableMetric
                });
            }
        }

        var criticalCount = riskItems.Count(r => r.Severity == RiskSeverity.Critical);
        var highCount = riskItems.Count(r => r.Severity == RiskSeverity.High);
        var medCount = riskItems.Count(r => r.Severity == RiskSeverity.Medium);
        var lowCount = riskItems.Count(r => r.Severity == RiskSeverity.Low);

        var overall = "Low";
        if (criticalCount > 0 || highCount >= 2) overall = "High";
        else if (highCount == 1 || medCount >= 2) overall = "Moderate";
        else if (criticalCount == 0 && highCount == 0 && medCount <= 1) overall = "Low";

        return new RiskAnalysisDto
        {
            IpoId = ipo.Id,
            CriticalRiskCount = criticalCount,
            HighRiskCount = highCount,
            MediumRiskCount = medCount,
            LowRiskCount = lowCount,
            OverallRiskLevel = overall,
            IdentifiedRisks = riskItems.OrderByDescending(r => r.Severity).ToList()
        };
    }
}
