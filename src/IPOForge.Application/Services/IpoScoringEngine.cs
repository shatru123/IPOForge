using System.Text.Json;
using IPOForge.Application.Interfaces;
using IPOForge.Contracts.Analysis;
using IPOForge.Contracts.Financial;
using IPOForge.Contracts.Gmp;
using IPOForge.Contracts.Subscription;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;

namespace IPOForge.Application.Services;

public class IpoScoringEngine : IIpoScoringEngine
{
    public ScoreBreakdownDto CalculateScore(
        IPO ipo,
        CompanyFinancialReportDto? financials,
        ValuationAnalysisDto? valuation,
        GmpHistoryDto? gmpHistory,
        SubscriptionBreakdownDto? subscription,
        IReadOnlyList<IPORisk>? risks)
    {
        var result = new ScoreBreakdownDto
        {
            IpoId = ipo.Id,
            CalculatedAt = DateTime.UtcNow
        };

        var listingPillars = new List<ScorePillarDto>();
        var longTermPillars = new List<ScorePillarDto>();

        // ==========================================
        // 1. LISTING GAIN SCORE CALCULATION (100 Pts)
        // ==========================================

        // Pillar 1: GMP % (20 pts)
        var gmpPercent = gmpHistory?.CurrentGmpPercentage ?? (ipo.PriceBandHigh > 0 && gmpHistory?.CurrentGmp > 0 ? (gmpHistory.CurrentGmp / ipo.PriceBandHigh) * 100 : 0);
        int gmpScore;
        string gmpReason;
        if (gmpPercent >= 50) { gmpScore = 20; gmpReason = $"Exceptional Grey Market Premium of {gmpPercent:F1}% indicates intense retail and HNI listing enthusiasm."; }
        else if (gmpPercent >= 35) { gmpScore = 16; gmpReason = $"Strong GMP of {gmpPercent:F1}% suggests healthy listing day upside."; }
        else if (gmpPercent >= 20) { gmpScore = 12; gmpReason = $"Moderate GMP of {gmpPercent:F1}% offers a reasonable buffer."; }
        else if (gmpPercent >= 10) { gmpScore = 8; gmpReason = $"Mild GMP of {gmpPercent:F1}% indicates modest listing expectations."; }
        else if (gmpPercent > 0) { gmpScore = 4; gmpReason = $"Subdued GMP of {gmpPercent:F1}% leaves little room for listing gains."; }
        else { gmpScore = 1; gmpReason = "No noticeable grey market premium or discount observed."; }
        listingPillars.Add(new ScorePillarDto { Name = "Grey Market Premium (GMP)", Score = gmpScore, MaxScore = 20, Reason = gmpReason });

        // Pillar 2: GMP Trend (10 pts)
        var trend = gmpHistory?.Trend ?? GmpTrend.Stable;
        int trendScore;
        string trendReason;
        switch (trend)
        {
            case GmpTrend.StronglyIncreasing:
                trendScore = 10;
                trendReason = "GMP is in a strong uptrend over recent sessions, signaling accelerating demand.";
                break;
            case GmpTrend.Increasing:
                trendScore = 8;
                trendReason = "GMP is steadily expanding as issue close date approaches.";
                break;
            case GmpTrend.Stable:
                trendScore = 6;
                trendReason = "GMP has remained steady across recent market sessions.";
                break;
            case GmpTrend.Declining:
                trendScore = 2;
                trendReason = "GMP has softened over recent sessions, indicating cooling demand.";
                break;
            case GmpTrend.StronglyDeclining:
            default:
                trendScore = 0;
                trendReason = "GMP has fallen sharply, suggesting potential listing day pressure.";
                break;
        }
        listingPillars.Add(new ScorePillarDto { Name = "GMP Momentum & Trend", Score = trendScore, MaxScore = 10, Reason = trendReason });

        // Pillar 3: Total Subscription (20 pts)
        var totalSub = subscription?.LatestTotalSubscription ?? 0;
        int subScore;
        string subReason;
        if (totalSub >= 50) { subScore = 20; subReason = $"Massive demand with {totalSub:F1}x total subscription, creating acute allotment scarcity."; }
        else if (totalSub >= 25) { subScore = 16; subReason = $"Very strong subscription demand ({totalSub:F1}x)."; }
        else if (totalSub >= 10) { subScore = 12; subReason = $"Healthy oversubscription ({totalSub:F1}x)."; }
        else if (totalSub >= 3) { subScore = 8; subReason = $"Moderate subscription ({totalSub:F1}x)."; }
        else if (totalSub >= 1) { subScore = 4; subReason = $"Fully subscribed ({totalSub:F1}x), but lacks explosive retail/HNI frenzy."; }
        else { subScore = 2; subReason = totalSub > 0 ? $"Undersubscribed or early bidding ({totalSub:F1}x)." : "Subscription bidding not yet opened."; }
        listingPillars.Add(new ScorePillarDto { Name = "Overall Subscription Demand", Score = subScore, MaxScore = 20, Reason = subReason });

        // Pillar 4: Demand Quality / QIB Backing (10 pts)
        var qibSub = subscription?.LatestQibSubscription ?? 0;
        int qibScore;
        string qibReason;
        if (qibSub >= 30) { qibScore = 10; qibReason = $"Heavy institutional commitment with {qibSub:F1}x QIB subscription."; }
        else if (qibSub >= 15) { qibScore = 8; qibReason = $"Strong institutional interest ({qibSub:F1}x QIB)."; }
        else if (qibSub >= 5) { qibScore = 6; qibReason = $"Moderate institutional participation ({qibSub:F1}x QIB)."; }
        else if (qibSub >= 1) { qibScore = 3; qibReason = $"Base institutional book covered ({qibSub:F1}x QIB)."; }
        else { qibScore = 2; qibReason = "QIB book bidding pending or subdued."; }
        listingPillars.Add(new ScorePillarDto { Name = "Institutional Demand Quality (QIB)", Score = qibScore, MaxScore = 10, Reason = qibReason });

        // Pillar 5: Valuation Impact on Listing (10 pts)
        var valClass = valuation?.Classification ?? ValuationClassification.Reasonable;
        int valScore;
        string valReason;
        switch (valClass)
        {
            case ValuationClassification.Attractive:
                valScore = 10;
                valReason = "Discounted pricing leaves meaningful room on the table for listing gains.";
                break;
            case ValuationClassification.Reasonable:
                valScore = 8;
                valReason = "Fairly valued issue price provides adequate safety cushion for first-day trades.";
                break;
            case ValuationClassification.Premium:
                valScore = 5;
                valReason = "Premium pricing requires sustained market momentum to maintain listing premium.";
                break;
            case ValuationClassification.VeryExpensive:
            default:
                valScore = 2;
                valReason = "Stretched valuations increase the risk of profit-booking immediately upon listing.";
                break;
        }
        listingPillars.Add(new ScorePillarDto { Name = "Valuation Cushion", Score = valScore, MaxScore = 10, Reason = valReason });

        // Pillar 6: Fundamental Business Momentum (10 pts)
        var revGrowth = financials?.GrowthAnalysis?.RevenueGrowthYoY ?? 0;
        var patGrowth = financials?.GrowthAnalysis?.ProfitGrowthYoY ?? 0;
        int bizScore;
        string bizReason;
        if (patGrowth >= 25 && revGrowth >= 20) { bizScore = 10; bizReason = $"Robust short-term financial acceleration (YoY Rev: +{revGrowth:F0}%, PAT: +{patGrowth:F0}%)."; }
        else if (patGrowth > 10 && revGrowth > 10) { bizScore = 7; bizReason = "Stable top-line and bottom-line momentum supports market sentiment."; }
        else if (patGrowth > 0) { bizScore = 4; bizReason = "Modest profitability growth."; }
        else { bizScore = 2; bizReason = "Unprofitable or contracting earnings may cap first-day enthusiasm."; }
        listingPillars.Add(new ScorePillarDto { Name = "Business Momentum", Score = bizScore, MaxScore = 10, Reason = bizReason });

        // Pillar 7: Risk Penalties (10 pts)
        var highRisks = risks?.Count(r => r.Severity >= RiskSeverity.High) ?? 0;
        int riskPillarScore = Math.Max(1, 10 - (highRisks * 3));
        string riskPillarReason = highRisks == 0
            ? "No critical or elevated risk flags identified in issue structure or balance sheet."
            : $"{highRisks} elevated risk factor(s) detected (e.g. leverage, customer concentration, or OFS proportion).";
        listingPillars.Add(new ScorePillarDto { Name = "Risk & Issue Vulnerability", Score = riskPillarScore, MaxScore = 10, Reason = riskPillarReason });

        // Pillar 8: Market Context & Platform Tier (10 pts)
        int marketScore;
        string marketReason;
        if (ipo.IpoType == IpoType.Mainboard)
        {
            marketScore = totalSub > 5 ? 10 : 8;
            marketReason = "Mainboard listing on NSE/BSE ensures deep market liquidity and broad retail accessibility.";
        }
        else
        {
            marketScore = totalSub > 15 ? 8 : 5;
            marketReason = "SME platform issue with higher lot sizing (₹1L+) and specialized trading liquidity.";
        }
        listingPillars.Add(new ScorePillarDto { Name = "Market Context & Liquidity", Score = marketScore, MaxScore = 10, Reason = marketReason });

        var totalListingScore = listingPillars.Sum(p => p.Score);
        result.ListingGainScore = totalListingScore;
        result.ListingRecommendation = MapScoreToRating(totalListingScore);
        result.ListingGainPillars = listingPillars;
        result.ListingGainVerdict = GenerateListingVerdict(totalListingScore, result.ListingRecommendation, gmpPercent, totalSub);

        // ==========================================
        // 2. LONG-TERM SCORE CALCULATION (100 Pts)
        // ==========================================

        // Pillar 1: Revenue Growth / 3Y CAGR (10 pts)
        var revCagr = financials?.GrowthAnalysis?.RevenueCagr3Year ?? financials?.GrowthAnalysis?.RevenueGrowthYoY ?? 0;
        int revScore;
        string revReason;
        if (revCagr >= 25) { revScore = 10; revReason = $"Impressive 3-year revenue CAGR of {revCagr:F1}%."; }
        else if (revCagr >= 15) { revScore = 8; revReason = $"Healthy revenue growth of {revCagr:F1}% CAGR."; }
        else if (revCagr >= 8) { revScore = 5; revReason = $"Moderate revenue CAGR of {revCagr:F1}%."; }
        else if (revCagr > 0) { revScore = 3; revReason = $"Slow revenue expansion ({revCagr:F1}%)."; }
        else { revScore = 1; revReason = "Negative top-line growth or limited historical track record."; }
        longTermPillars.Add(new ScorePillarDto { Name = "Revenue Growth & CAGR", Score = revScore, MaxScore = 10, Reason = revReason });

        // Pillar 2: Profit Growth / 3Y CAGR (10 pts)
        var patCagr = financials?.GrowthAnalysis?.ProfitCagr3Year ?? financials?.GrowthAnalysis?.ProfitGrowthYoY ?? 0;
        int patScore;
        string patReason;
        if (patCagr >= 30) { patScore = 10; patReason = $"Compounding PAT growth at {patCagr:F1}% CAGR indicates strong operating leverage."; }
        else if (patCagr >= 15) { patScore = 8; patReason = $"Solid profit expansion of {patCagr:F1}% CAGR."; }
        else if (patCagr >= 5) { patScore = 5; patReason = $"Modest bottom-line CAGR of {patCagr:F1}%."; }
        else if (patCagr > 0) { patScore = 2; patReason = $"Minimal profit growth ({patCagr:F1}%)."; }
        else { patScore = 0; patReason = "Earnings contraction or operating losses over the evaluation period."; }
        longTermPillars.Add(new ScorePillarDto { Name = "Profit Compounding (PAT CAGR)", Score = patScore, MaxScore = 10, Reason = patReason });

        // Pillar 3: Operating / EBITDA Margins (10 pts)
        var latestFin = financials?.Years?.LastOrDefault();
        var ebitdaMargin = latestFin?.EbitdaMargin ?? 0;
        int marginScore;
        string marginReason;
        if (ebitdaMargin >= 25) { marginScore = 10; marginReason = $"High pricing power with {ebitdaMargin:F1}% EBITDA margin."; }
        else if (ebitdaMargin >= 16) { marginScore = 8; marginReason = $"Strong margin profile of {ebitdaMargin:F1}%."; }
        else if (ebitdaMargin >= 9) { marginScore = 5; marginReason = $"Standard operational margin of {ebitdaMargin:F1}%."; }
        else if (ebitdaMargin > 0) { marginScore = 2; marginReason = $"Thin margin cushion of {ebitdaMargin:F1}%."; }
        else { marginScore = 0; marginReason = "Negative or undisclosed operating margins."; }
        longTermPillars.Add(new ScorePillarDto { Name = "Operating & EBITDA Margins", Score = marginScore, MaxScore = 10, Reason = marginReason });

        // Pillar 4: ROE & ROCE Return Ratios (10 pts)
        var roe = latestFin?.ROE ?? 0;
        int roeScore;
        string roeReason;
        if (roe >= 22) { roeScore = 10; roeReason = $"Superior capital efficiency with {roe:F1}% Return on Equity."; }
        else if (roe >= 15) { roeScore = 8; roeReason = $"Healthy Return on Equity of {roe:F1}%."; }
        else if (roe >= 10) { roeScore = 5; roeReason = $"Acceptable Return on Equity of {roe:F1}%."; }
        else if (roe > 0) { roeScore = 2; roeReason = $"Sub-par Return on Equity of {roe:F1}%."; }
        else { roeScore = 0; roeReason = "Negative or undisclosed ROE metric."; }
        longTermPillars.Add(new ScorePillarDto { Name = "Capital Efficiency (ROE / ROCE)", Score = roeScore, MaxScore = 10, Reason = roeReason });

        // Pillar 5: Balance Sheet & Debt/Equity (10 pts)
        var d2e = latestFin?.DebtToEquity ?? 0;
        int debtScore;
        string debtReason;
        if (d2e <= 0.2m) { debtScore = 10; debtReason = $"Pristine balance sheet with minimal debt (D/E: {d2e:F2}x)."; }
        else if (d2e <= 0.7m) { debtScore = 8; debtReason = $"Conservative leverage profile (D/E: {d2e:F2}x)."; }
        else if (d2e <= 1.5m) { debtScore = 5; debtReason = $"Moderate debt levels (D/E: {d2e:F2}x)."; }
        else if (d2e <= 2.5m) { debtScore = 2; debtReason = $"Elevated debt burden (D/E: {d2e:F2}x)."; }
        else { debtScore = 0; debtReason = $"High financial risk (D/E: {d2e:F2}x)."; }
        longTermPillars.Add(new ScorePillarDto { Name = "Solvency & Debt-to-Equity", Score = debtScore, MaxScore = 10, Reason = debtReason });

        // Pillar 6: Cash Flow Generation (10 pts)
        var ocf = latestFin?.OperatingCashFlow ?? 0;
        var fcf = latestFin?.FreeCashFlow ?? 0;
        int cfScore;
        string cfReason;
        if (ocf > 0 && fcf > 0) { cfScore = 10; cfReason = $"Robust cash conversion with positive Operating (₹{ocf:F1} Cr) and Free Cash Flow (₹{fcf:F1} Cr)."; }
        else if (ocf > 0) { cfScore = 7; cfReason = $"Positive operational cash generation (₹{ocf:F1} Cr), reinvesting in expansion."; }
        else if (ocf == 0) { cfScore = 4; cfReason = "Cash flow data is neutral or partially disclosed."; }
        else { cfScore = 0; cfReason = $"Negative operating cash flow (₹{ocf:F1} Cr) signals high working capital strain."; }
        longTermPillars.Add(new ScorePillarDto { Name = "Cash Flow Quality (OCF & FCF)", Score = cfScore, MaxScore = 10, Reason = cfReason });

        // Pillar 7: Long-Term Valuation Multiple (15 pts)
        int ltValScore;
        string ltValReason;
        switch (valClass)
        {
            case ValuationClassification.Attractive:
                ltValScore = 15;
                ltValReason = "Discounted valuation multiples relative to industry peers provide solid margin of safety for multi-year holding.";
                break;
            case ValuationClassification.Reasonable:
                ltValScore = 12;
                ltValReason = "Valuation multiples are in line with industry peers, allowing returns to track underlying business growth.";
                break;
            case ValuationClassification.Premium:
                ltValScore = 7;
                ltValReason = "Demanding valuation limits valuation expansion; returns will depend heavily on sustained earnings execution.";
                break;
            case ValuationClassification.VeryExpensive:
            default:
                ltValScore = 2;
                ltValReason = "Elevated valuation multiples introduce significant de-rating risk if quarterly earnings falter.";
                break;
        }
        longTermPillars.Add(new ScorePillarDto { Name = "Valuation & Margin of Safety", Score = ltValScore, MaxScore = 15, Reason = ltValReason });

        // Pillar 8: Business Moat & Position (10 pts)
        int moatScore = 8; // Default robust benchmark
        string moatReason = "Established market footprint with competitive barriers in primary service/product offerings.";
        if (financials?.GrowthAnalysis?.ProfitabilityVerdict?.Contains("Exceptional") == true)
        {
            moatScore = 10;
            moatReason = "Demonstrated industry leadership and strong economic moat reflected in market share.";
        }
        longTermPillars.Add(new ScorePillarDto { Name = "Competitive Moat & Industry Position", Score = moatScore, MaxScore = 10, Reason = moatReason });

        // Pillar 9: Promoter & Corporate Governance (5 pts)
        var preHolding = ipo.Company?.PromoterHoldingPreIssue ?? 75;
        int promScore;
        string promReason;
        if (preHolding >= 65) { promScore = 5; promReason = $"High promoter commitment ({preHolding:F1}% pre-issue holding)."; }
        else if (preHolding >= 50) { promScore = 4; promReason = $"Adequate promoter skin-in-the-game ({preHolding:F1}%)."; }
        else { promScore = 2; promReason = $"Lower promoter stake ({preHolding:F1}%)."; }
        longTermPillars.Add(new ScorePillarDto { Name = "Promoter Holding & Governance", Score = promScore, MaxScore = 5, Reason = promReason });

        // Pillar 10: Industry Secular Tailwinds (5 pts)
        var sector = ipo.Company?.Sector?.ToLowerInvariant() ?? "";
        int indScore = 4;
        string indReason = "Stable sector with predictable domestic demand cycles.";
        if (sector.Contains("tech") || sector.Contains("energy") || sector.Contains("solar") || sector.Contains("electronic") || sector.Contains("health"))
        {
            indScore = 5;
            indReason = "Operating in a high-growth sector with strong government incentives and secular tailwinds.";
        }
        longTermPillars.Add(new ScorePillarDto { Name = "Industry Outlook & Tailwinds", Score = indScore, MaxScore = 5, Reason = indReason });

        // Pillar 11: Fund Utilization Strategy (5 pts)
        int fundScore;
        string fundReason;
        var freshPct = ipo.IssueSize > 0 ? (ipo.FreshIssueAmount / ipo.IssueSize) * 100 : 50;
        if (freshPct >= 70)
        {
            fundScore = 5;
            fundReason = $"{freshPct:F0}% Fresh Issue will directly fund capacity expansion, debt reduction, or technology upgrades.";
        }
        else if (freshPct >= 40)
        {
            fundScore = 3;
            fundReason = $"Balanced issue structure ({freshPct:F0}% Fresh Issue, {100 - freshPct:F0}% OFS).";
        }
        else
        {
            fundScore = 1;
            fundReason = $"OFS-heavy structure ({100 - freshPct:F0}% OFS); limited fresh equity infused into core operations.";
        }
        longTermPillars.Add(new ScorePillarDto { Name = "Use of IPO Proceeds", Score = fundScore, MaxScore = 5, Reason = fundReason });

        var totalLongTermScore = longTermPillars.Sum(p => p.Score);
        result.LongTermScore = totalLongTermScore;
        result.LongTermRecommendation = MapScoreToRating(totalLongTermScore);
        result.LongTermPillars = longTermPillars;
        result.LongTermVerdict = GenerateLongTermVerdict(totalLongTermScore, result.LongTermRecommendation, revCagr, d2e);

        // Generate Unified Analytical Verdict
        result.UnifiedAnalyticalConclusion = GenerateUnifiedConclusion(result.ListingRecommendation, result.LongTermRecommendation, ipo.Name);

        return result;
    }

    private static RecommendationRating MapScoreToRating(int score) => score switch
    {
        >= 80 => RecommendationRating.Strong,
        >= 65 => RecommendationRating.Positive,
        >= 50 => RecommendationRating.Neutral,
        >= 35 => RecommendationRating.Weak,
        _ => RecommendationRating.Avoid
    };

    private static string GenerateListingVerdict(int score, RecommendationRating rating, decimal gmpPercent, decimal totalSub)
    {
        return rating switch
        {
            RecommendationRating.Strong => $"Strong candidate for listing gains (Score: {score}/100). High GMP ({gmpPercent:F1}%) and robust subscription ({totalSub:F1}x) suggest significant day-one premium potential.",
            RecommendationRating.Positive => $"Positive listing prospects (Score: {score}/100). Favorable momentum and healthy bid demand provide a supportive listing backdrop.",
            RecommendationRating.Neutral => $"Neutral listing view (Score: {score}/100). Moderate demand indicators suggest modest listing premium with limited margin of safety.",
            RecommendationRating.Weak => $"Weak listing indicators (Score: {score}/100). Tepid subscription and subdued grey market premium suggest cautious short-term expectations.",
            _ => $"High risk for listing trades (Score: {score}/100). Absence of grey market premium and poor bid interest signal vulnerability to listing day discounts."
        };
    }

    private static string GenerateLongTermVerdict(int score, RecommendationRating rating, decimal revCagr, decimal d2e)
    {
        return rating switch
        {
            RecommendationRating.Strong => $"Compelling long-term fundamentals (Score: {score}/100). Strong compound growth ({revCagr:F1}% CAGR), healthy return ratios, and clean balance sheet (D/E: {d2e:F2}x).",
            RecommendationRating.Positive => $"Constructive long-term profile (Score: {score}/100). Healthy financial trajectory and defensible business model support multi-year compounding.",
            RecommendationRating.Neutral => $"Balanced multi-year outlook (Score: {score}/100). Solid business core balanced by premium valuation or specific operational/debt considerations.",
            RecommendationRating.Weak => $"Long-term headwinds (Score: {score}/100). Below-average return ratios or elevated balance sheet leverage require careful monitoring.",
            _ => $"Unfavorable for long-term investment (Score: {score}/100). Inconsistent cash flows, contracting earnings, or severe debt burden present significant downside risks."
        };
    }

    private static string GenerateUnifiedConclusion(RecommendationRating listing, RecommendationRating longTerm, string ipoName)
    {
        if (listing == RecommendationRating.Strong && longTerm == RecommendationRating.Strong)
            return $"{ipoName} demonstrates exceptional strength across both listing momentum and multi-year fundamental quality.";
        if (listing >= RecommendationRating.Positive && longTerm <= RecommendationRating.Neutral)
            return $"Potentially attractive for listing gains backed by strong grey market demand, but long-term investors should carefully consider valuation multiples and fundamental risk factors.";
        if (listing <= RecommendationRating.Neutral && longTerm >= RecommendationRating.Positive)
            return $"Subdued initial listing excitement may offer a favorable entry point for patient, long-term fundamental investors.";
        if (listing <= RecommendationRating.Weak && longTerm <= RecommendationRating.Weak)
            return $"Both short-term demand metrics and multi-year financial health indicate heightened risk; caution is advised.";
        return $"Mixed indicators between short-term demand and fundamental compounding parameters. Align bidding strategy with individual risk tolerance and investment horizons.";
    }
}
