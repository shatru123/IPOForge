using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Financial;
using IPOForge.Domain.Entities;

namespace IPOForge.Application.Services;

public class FinancialAnalysisEngine : IFinancialAnalysisEngine
{
    public CompanyFinancialReportDto AnalyzeFinancials(Company company)
    {
        var result = new CompanyFinancialReportDto
        {
            CompanyId = company.Id,
            CompanyName = company.Name,
            Sector = company.Sector,
            Industry = company.Industry
        };

        var financials = company.Financials
            .OrderBy(f => f.PeriodEnding)
            .ToList();

        if (!financials.Any())
        {
            return result;
        }

        var yearDtos = new List<FinancialYearDto>();

        foreach (var fin in financials)
        {
            var ebitdaMargin = fin.Revenue > 0 && fin.EBITDA.HasValue
                ? Math.Round((fin.EBITDA.Value / fin.Revenue.Value) * 100, 2)
                : (decimal?)null;

            var patMargin = fin.Revenue > 0 && fin.PAT.HasValue
                ? Math.Round((fin.PAT.Value / fin.Revenue.Value) * 100, 2)
                : (decimal?)null;

            var roe = fin.NetWorth > 0 && fin.PAT.HasValue
                ? Math.Round((fin.PAT.Value / fin.NetWorth.Value) * 100, 2)
                : (decimal?)null;

            var capitalEmployed = (fin.TotalAssets ?? 0) - (fin.CurrentLiabilities ?? 0);
            var roce = capitalEmployed > 0 && fin.EBIT.HasValue
                ? Math.Round((fin.EBIT.Value / capitalEmployed) * 100, 2)
                : (decimal?)null;

            var d2e = fin.NetWorth > 0 && fin.TotalDebt.HasValue
                ? Math.Round(fin.TotalDebt.Value / fin.NetWorth.Value, 2)
                : (decimal?)null;

            var currentRatio = fin.CurrentLiabilities > 0 && fin.CurrentAssets.HasValue
                ? Math.Round(fin.CurrentAssets.Value / fin.CurrentLiabilities.Value, 2)
                : (decimal?)null;

            yearDtos.Add(new FinancialYearDto
            {
                Id = fin.Id,
                FiscalYear = fin.FiscalYear,
                PeriodEnding = fin.PeriodEnding,
                IsAudited = fin.IsAudited,
                Revenue = fin.Revenue,
                EBITDA = fin.EBITDA,
                EBIT = fin.EBIT,
                PAT = fin.PAT,
                EPS = fin.EPS,
                OperatingCashFlow = fin.OperatingCashFlow,
                FreeCashFlow = fin.FreeCashFlow,
                TotalAssets = fin.TotalAssets,
                TotalDebt = fin.TotalDebt,
                NetWorth = fin.NetWorth,
                CurrentAssets = fin.CurrentAssets,
                CurrentLiabilities = fin.CurrentLiabilities,
                EbitdaMargin = ebitdaMargin,
                PatMargin = patMargin,
                ROE = roe,
                ROCE = roce,
                DebtToEquity = d2e,
                CurrentRatio = currentRatio
            });
        }

        result.Years = yearDtos;

        // Calculate Growth and Trends
        var growth = new FinancialGrowthDto();
        if (yearDtos.Count >= 2)
        {
            var latest = yearDtos.Last();
            var prev = yearDtos[^2];

            if (latest.Revenue.HasValue && prev.Revenue.HasValue && prev.Revenue.Value > 0)
            {
                growth.RevenueGrowthYoY = Math.Round(((latest.Revenue.Value - prev.Revenue.Value) / prev.Revenue.Value) * 100, 2);
            }

            if (latest.PAT.HasValue && prev.PAT.HasValue && prev.PAT.Value > 0)
            {
                growth.ProfitGrowthYoY = Math.Round(((latest.PAT.Value - prev.PAT.Value) / prev.PAT.Value) * 100, 2);
            }

            if (latest.EPS.HasValue && prev.EPS.HasValue && prev.EPS.Value > 0)
            {
                growth.EpsGrowthYoY = Math.Round(((latest.EPS.Value - prev.EPS.Value) / prev.EPS.Value) * 100, 2);
            }
        }

        if (yearDtos.Count >= 3)
        {
            var latest = yearDtos.Last();
            var oldest = yearDtos.First();
            var years = yearDtos.Count - 1;

            if (latest.Revenue.HasValue && oldest.Revenue.HasValue && oldest.Revenue.Value > 0 && latest.Revenue.Value > 0)
            {
                var cagr = Math.Pow((double)(latest.Revenue.Value / oldest.Revenue.Value), 1.0 / years) - 1.0;
                growth.RevenueCagr3Year = Math.Round((decimal)cagr * 100, 2);
            }

            if (latest.PAT.HasValue && oldest.PAT.HasValue && oldest.PAT.Value > 0 && latest.PAT.Value > 0)
            {
                var cagr = Math.Pow((double)(latest.PAT.Value / oldest.PAT.Value), 1.0 / years) - 1.0;
                growth.ProfitCagr3Year = Math.Round((decimal)cagr * 100, 2);
            }

            if (latest.EBITDA.HasValue && oldest.EBITDA.HasValue && oldest.EBITDA.Value > 0 && latest.EBITDA.Value > 0)
            {
                var cagr = Math.Pow((double)(latest.EBITDA.Value / oldest.EBITDA.Value), 1.0 / years) - 1.0;
                growth.EbitdaCagr3Year = Math.Round((decimal)cagr * 100, 2);
            }
        }

        // Cash flow evaluation
        var ocfList = yearDtos.Where(y => y.OperatingCashFlow.HasValue).Select(y => y.OperatingCashFlow!.Value).ToList();
        if (ocfList.Count >= 2)
        {
            if (ocfList.All(v => v > 0))
                growth.CashFlowTrend = "Consistently Positive";
            else if (ocfList.Last() > 0 && ocfList.First() <= 0)
                growth.CashFlowTrend = "Turned Positive";
            else if (ocfList.Last() < 0)
                growth.CashFlowTrend = "Negative Operating Cash Flow";
            else
                growth.CashFlowTrend = "Fluctuating";
        }

        // Profitability verdict
        if (growth.ProfitCagr3Year > 25)
            growth.ProfitabilityVerdict = "Exceptional Growth (>25% 3-Year CAGR)";
        else if (growth.ProfitCagr3Year > 15)
            growth.ProfitabilityVerdict = "Strong Steady Growth (15-25% CAGR)";
        else if (growth.ProfitCagr3Year > 0)
            growth.ProfitabilityVerdict = "Moderate Growth (<15% CAGR)";
        else if (growth.ProfitCagr3Year.HasValue)
            growth.ProfitabilityVerdict = "Declining Profitability";
        else
            growth.ProfitabilityVerdict = "Insufficient multi-year history";

        // Solvency verdict
        var latestDebt = yearDtos.LastOrDefault()?.DebtToEquity;
        if (latestDebt.HasValue)
        {
            if (latestDebt.Value < 0.2m)
                growth.SolvencyVerdict = "Virtually Debt-Free (D/E < 0.2x)";
            else if (latestDebt.Value < 0.8m)
                growth.SolvencyVerdict = "Comfortable Leverage (D/E < 0.8x)";
            else if (latestDebt.Value < 1.5m)
                growth.SolvencyVerdict = "Moderate Debt (D/E < 1.5x)";
            else
                growth.SolvencyVerdict = "Elevated Leverage (D/E > 1.5x)";
        }

        result.GrowthAnalysis = growth;
        return result;
    }
}
