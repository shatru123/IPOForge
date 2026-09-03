using System.Text.Json;
using IPOForge.Application.Interfaces;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IPOForge.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        IpoForgeDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IIpoScoringEngine scoringEngine,
        IFinancialAnalysisEngine financialEngine,
        IValuationEngine valuationEngine,
        IGmpAnalyticsService gmpService,
        IRiskEngine riskEngine,
        ILogger logger)
    {
        try
        {
            // Ensure schema is created
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Industry Benchmark Metrics if empty
            if (!await context.IndustryMetrics.AnyAsync())
            {
                var industryMetrics = new List<IndustryMetric>
                {
                    new() { Sector = "Technology", Industry = "Automotive Engineering & IT", MedianPE = 42.5m, MedianPB = 8.2m, MedianROE = 21.4m, MedianROCE = 26.8m, MedianDebtEquity = 0.15m, MedianNetMargin = 16.5m },
                    new() { Sector = "Energy & Utilities", Industry = "Solar & Renewable Equipment", MedianPE = 38.0m, MedianPB = 5.5m, MedianROE = 19.5m, MedianROCE = 22.0m, MedianDebtEquity = 0.65m, MedianNetMargin = 14.2m },
                    new() { Sector = "Financial Services", Industry = "Housing Finance & NBFC", MedianPE = 24.0m, MedianPB = 3.2m, MedianROE = 16.8m, MedianROCE = 14.5m, MedianDebtEquity = 3.80m, MedianNetMargin = 22.0m },
                    new() { Sector = "Consumer Discretionary", Industry = "Quick Commerce & Delivery", MedianPE = 65.0m, MedianPB = 6.8m, MedianROE = 8.5m, MedianROCE = 9.2m, MedianDebtEquity = 0.25m, MedianNetMargin = 3.5m },
                    new() { Sector = "Automobile", Industry = "Electric Two-Wheelers", MedianPE = 48.0m, MedianPB = 5.8m, MedianROE = 12.0m, MedianROCE = 14.0m, MedianDebtEquity = 0.45m, MedianNetMargin = 6.8m },
                    new() { Sector = "Consumer Staples", Industry = "D2C Beauty & Personal Care", MedianPE = 52.0m, MedianPB = 7.1m, MedianROE = 14.5m, MedianROCE = 16.2m, MedianDebtEquity = 0.18m, MedianNetMargin = 8.4m },
                    new() { Sector = "Capital Goods & Automation", Industry = "Industrial Automation & Robotics", MedianPE = 36.5m, MedianPB = 4.8m, MedianROE = 18.2m, MedianROCE = 21.0m, MedianDebtEquity = 0.35m, MedianNetMargin = 12.8m }
                };
                await context.IndustryMetrics.AddRangeAsync(industryMetrics);
                await context.SaveChangesAsync();
            }

            // 2. Seed Data Sources if empty
            if (!await context.DataSources.AnyAsync())
            {
                var dataSources = new List<DataSource>
                {
                    new() { Name = "BSE & NSE India Official Feeds", ProviderKey = "EXCHANGE_FEED", SourceType = DataSourceType.OfficialExchange, BaseUrl = "https://www.bseindia.com", IsActive = true, LastSyncAt = DateTime.UtcNow.AddMinutes(-12), HealthStatus = "Operational" },
                    new() { Name = "Link Intime & KFinTech Registrars", ProviderKey = "REGISTRAR_FEED", SourceType = DataSourceType.Registrar, BaseUrl = "https://linkintime.co.in", IsActive = true, LastSyncAt = DateTime.UtcNow.AddMinutes(-25), HealthStatus = "Operational" },
                    new() { Name = "Grey Market Intelligence Aggregator", ProviderKey = "GMP_FEED", SourceType = DataSourceType.MarketAggregator, BaseUrl = "https://ipowatch.in", IsActive = true, LastSyncAt = DateTime.UtcNow.AddMinutes(-5), HealthStatus = "Operational" }
                };
                await context.DataSources.AddRangeAsync(dataSources);
                await context.SaveChangesAsync();
            }

            // 3. Seed Companies, IPOs, Financials, GMP, Subscriptions, Objectives, Risks if empty
            if (!await context.Companies.AnyAsync())
            {
                logger.LogInformation("Seeding Indian IPO dataset with historical and active issues...");

                // 1. Tata Technologies Limited
                var tataTech = new Company
                {
                    Name = "Tata Technologies Limited",
                    LegalName = "Tata Technologies Limited",
                    CIN = "U72200PN1994PLC013313",
                    Symbol = "TATATECH",
                    Sector = "Technology",
                    Industry = "Automotive Engineering & IT",
                    Description = "A leading global engineering services company providing product development and digital solutions to global automotive OEMs and aerospace giants.",
                    Website = "https://www.tatatechnologies.com",
                    FoundedYear = 1994,
                    Headquarters = "Pune, Maharashtra",
                    PromoterInformation = "Tata Motors Limited (Promoter with 64.79% holding pre-issue)",
                    ManagingDirector = "Warren Harris",
                    PromoterHoldingPreIssue = 66.8m,
                    PromoterHoldingPostIssue = 55.4m
                };
                tataTech.Financials = new List<CompanyFinancial>
                {
                    new() { FiscalYear = "FY2022", PeriodEnding = new DateTime(2022, 3, 31), Revenue = 3529.6m, EBITDA = 645.7m, EBIT = 582.4m, PAT = 437.0m, EPS = 10.77m, OperatingCashFlow = 492.1m, FreeCashFlow = 380.5m, TotalAssets = 4210.0m, TotalDebt = 0m, NetWorth = 2280.0m, CurrentAssets = 2890.0m, CurrentLiabilities = 1100.0m, EbitdaMargin = 18.29m, PatMargin = 12.38m, ROE = 19.17m, DebtToEquity = 0m, CurrentRatio = 2.63m },
                    new() { FiscalYear = "FY2023", PeriodEnding = new DateTime(2023, 3, 31), Revenue = 4414.2m, EBITDA = 914.4m, EBIT = 835.0m, PAT = 624.0m, EPS = 15.38m, OperatingCashFlow = 712.5m, FreeCashFlow = 589.2m, TotalAssets = 4950.0m, TotalDebt = 0m, NetWorth = 2980.0m, CurrentAssets = 3450.0m, CurrentLiabilities = 1250.0m, EbitdaMargin = 20.71m, PatMargin = 14.14m, ROE = 20.94m, DebtToEquity = 0m, CurrentRatio = 2.76m },
                    new() { FiscalYear = "FY2024", PeriodEnding = new DateTime(2024, 3, 31), Revenue = 5117.2m, EBITDA = 1072.0m, EBIT = 988.0m, PAT = 745.0m, EPS = 18.36m, OperatingCashFlow = 890.0m, FreeCashFlow = 740.0m, TotalAssets = 5720.0m, TotalDebt = 0m, NetWorth = 3650.0m, CurrentAssets = 4120.0m, CurrentLiabilities = 1380.0m, EbitdaMargin = 20.95m, PatMargin = 14.56m, ROE = 20.41m, DebtToEquity = 0m, CurrentRatio = 2.99m }
                };

                var tataTechIpo = new IPO
                {
                    Name = "Tata Technologies IPO",
                    Symbol = "TATATECH",
                    IpoType = IpoType.Mainboard,
                    Status = IpoStatus.Listed,
                    OpenDate = new DateTime(2023, 11, 22),
                    CloseDate = new DateTime(2023, 11, 24),
                    AllotmentDate = new DateTime(2023, 11, 28),
                    ListingDate = new DateTime(2023, 11, 30),
                    PriceBandLow = 475m,
                    PriceBandHigh = 500m,
                    LotSize = 30,
                    MinimumInvestment = 15000m,
                    IssueSize = 3042.51m,
                    FreshIssueAmount = 0m,
                    OFSAmount = 3042.51m,
                    FaceValue = 2m,
                    Registrar = "Link Intime India Private Ltd",
                    LeadManagers = "JM Financial, Citigroup Global Markets, BofA Securities",
                    Exchange = "BSE, NSE",
                    ListingPrice = 1200.0m,
                    ListingGainPercent = 140.0m,
                    Day1ClosePrice = 1313.0m
                };
                tataTechIpo.GmpHistories = new List<IPOGmpHistory>
                {
                    new() { GMP = 340m, GMPPercentage = 68.0m, EstimatedListingPrice = 840m, Source = "Market Aggregator", ObservedAt = new DateTime(2023, 11, 18), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 380m, GMPPercentage = 76.0m, EstimatedListingPrice = 880m, Source = "Market Aggregator", ObservedAt = new DateTime(2023, 11, 20), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 415m, GMPPercentage = 83.0m, EstimatedListingPrice = 915m, Source = "Market Aggregator", ObservedAt = new DateTime(2023, 11, 23), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 450m, GMPPercentage = 90.0m, EstimatedListingPrice = 950m, Source = "Market Aggregator", ObservedAt = new DateTime(2023, 11, 27), RetrievedAt = DateTime.UtcNow }
                };
                tataTechIpo.SubscriptionHistories = new List<IPOSubscriptionHistory>
                {
                    new() { RetailSubscription = 5.4m, QibSubscription = 4.1m, NiiSubscription = 8.6m, TotalSubscription = 6.5m, DayNumber = 1, SnapshotDate = new DateTime(2023, 11, 22) },
                    new() { RetailSubscription = 11.2m, QibSubscription = 8.5m, NiiSubscription = 31.0m, TotalSubscription = 15.1m, DayNumber = 2, SnapshotDate = new DateTime(2023, 11, 23) },
                    new() { RetailSubscription = 16.5m, QibSubscription = 203.4m, NiiSubscription = 62.1m, TotalSubscription = 69.4m, DayNumber = 3, SnapshotDate = new DateTime(2023, 11, 24) }
                };
                tataTechIpo.Objectives = new List<IPOObjective>
                {
                    new() { Category = ObjectiveCategory.OfferForSale, Title = "Offer for Sale by Promoter & Investors", Description = "Provide exit liquidity to Tata Motors and Alpha TC Holdings.", AmountInCrores = 3042.51m, PercentageOfTotal = 100m }
                };
                tataTechIpo.Risks = new List<IPORisk>
                {
                    new() { Category = "CLIENT CONCENTRATION", Title = "Client Concentration in Auto Anchors", Description = "Top 5 clients, including Tata Motors and JLR, contribute over 60% of revenues.", Severity = RiskSeverity.Medium, TraceableMetric = "Top 5 Client Share = 62%" },
                    new() { Category = "ISSUE STRUCTURE", Title = "100% OFS Structure", Description = "No fresh capital infused into business operations.", Severity = RiskSeverity.Low, TraceableMetric = "OFS = 100%" }
                };
                tataTech.Ipos.Add(tataTechIpo);

                // 2. Waaree Energies Limited
                var waaree = new Company
                {
                    Name = "Waaree Energies Limited",
                    LegalName = "Waaree Energies Limited",
                    CIN = "U29248MH1990PLC058703",
                    Symbol = "WAAREE",
                    Sector = "Energy & Utilities",
                    Industry = "Solar & Renewable Equipment",
                    Description = "India's largest manufacturer of solar PV modules with an aggregate installed capacity of 12 GW, exporting globally to the US and Europe.",
                    Website = "https://www.waaree.com",
                    FoundedYear = 1990,
                    Headquarters = "Mumbai, Maharashtra",
                    PromoterInformation = "Hitesh Chimanlal Doshi & Family",
                    ManagingDirector = "Hitesh Doshi",
                    PromoterHoldingPreIssue = 71.8m,
                    PromoterHoldingPostIssue = 64.3m
                };
                waaree.Financials = new List<CompanyFinancial>
                {
                    new() { FiscalYear = "FY2022", PeriodEnding = new DateTime(2022, 3, 31), Revenue = 2854.0m, EBITDA = 198.0m, EBIT = 152.0m, PAT = 79.6m, EPS = 3.65m, OperatingCashFlow = 112.0m, FreeCashFlow = 45.0m, TotalAssets = 3450.0m, TotalDebt = 520.0m, NetWorth = 980.0m, CurrentAssets = 2100.0m, CurrentLiabilities = 1650.0m, EbitdaMargin = 6.94m, PatMargin = 2.79m, ROE = 8.12m, DebtToEquity = 0.53m, CurrentRatio = 1.27m },
                    new() { FiscalYear = "FY2023", PeriodEnding = new DateTime(2023, 3, 31), Revenue = 6750.0m, EBITDA = 946.0m, EBIT = 830.0m, PAT = 500.2m, EPS = 21.40m, OperatingCashFlow = 810.0m, FreeCashFlow = 420.0m, TotalAssets = 6800.0m, TotalDebt = 650.0m, NetWorth = 1950.0m, CurrentAssets = 4300.0m, CurrentLiabilities = 2900.0m, EbitdaMargin = 14.01m, PatMargin = 7.41m, ROE = 25.65m, DebtToEquity = 0.33m, CurrentRatio = 1.48m },
                    new() { FiscalYear = "FY2024", PeriodEnding = new DateTime(2024, 3, 31), Revenue = 11398.0m, EBITDA = 1845.0m, EBIT = 1650.0m, PAT = 1274.0m, EPS = 53.40m, OperatingCashFlow = 1620.0m, FreeCashFlow = 910.0m, TotalAssets = 11200.0m, TotalDebt = 480.0m, NetWorth = 4150.0m, CurrentAssets = 7100.0m, CurrentLiabilities = 4100.0m, EbitdaMargin = 16.19m, PatMargin = 11.18m, ROE = 30.70m, DebtToEquity = 0.12m, CurrentRatio = 1.73m }
                };

                var waareeIpo = new IPO
                {
                    Name = "Waaree Energies IPO",
                    Symbol = "WAAREE",
                    IpoType = IpoType.Mainboard,
                    Status = IpoStatus.Listed,
                    OpenDate = new DateTime(2024, 10, 21),
                    CloseDate = new DateTime(2024, 10, 23),
                    AllotmentDate = new DateTime(2024, 10, 25),
                    ListingDate = new DateTime(2024, 10, 28),
                    PriceBandLow = 1427m,
                    PriceBandHigh = 1503m,
                    LotSize = 9,
                    MinimumInvestment = 13527m,
                    IssueSize = 4321.44m,
                    FreshIssueAmount = 3600.0m,
                    OFSAmount = 721.44m,
                    FaceValue = 10m,
                    Registrar = "Link Intime India Private Ltd",
                    LeadManagers = "Axis Capital, Jefferies India, Nomura, SBI Capital",
                    Exchange = "BSE, NSE",
                    ListingPrice = 2550.0m,
                    ListingGainPercent = 69.66m,
                    Day1ClosePrice = 2345.0m
                };
                waareeIpo.GmpHistories = new List<IPOGmpHistory>
                {
                    new() { GMP = 1250m, GMPPercentage = 83.17m, EstimatedListingPrice = 2753m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 10, 18), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 1425m, GMPPercentage = 94.81m, EstimatedListingPrice = 2928m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 10, 21), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 1560m, GMPPercentage = 103.79m, EstimatedListingPrice = 3063m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 10, 23), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 1300m, GMPPercentage = 86.49m, EstimatedListingPrice = 2803m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 10, 26), RetrievedAt = DateTime.UtcNow }
                };
                waareeIpo.SubscriptionHistories = new List<IPOSubscriptionHistory>
                {
                    new() { RetailSubscription = 3.3m, QibSubscription = 1.8m, NiiSubscription = 8.1m, TotalSubscription = 3.5m, DayNumber = 1, SnapshotDate = new DateTime(2024, 10, 21) },
                    new() { RetailSubscription = 6.4m, QibSubscription = 3.9m, NiiSubscription = 24.5m, TotalSubscription = 9.2m, DayNumber = 2, SnapshotDate = new DateTime(2024, 10, 22) },
                    new() { RetailSubscription = 11.3m, QibSubscription = 215.0m, NiiSubscription = 65.2m, TotalSubscription = 79.4m, DayNumber = 3, SnapshotDate = new DateTime(2024, 10, 23) }
                };
                waareeIpo.Objectives = new List<IPOObjective>
                {
                    new() { Category = ObjectiveCategory.Expansion, Title = "Set up 6GW Ingot-Wafer, Solar Cell & Module Facility in Odisha", Description = "Capex investment in vertical integration manufacturing.", AmountInCrores = 2775.0m, PercentageOfTotal = 64.21m },
                    new() { Category = ObjectiveCategory.GeneralCorporate, Title = "General Corporate Purposes", Description = "Strategic expansion and working buffer.", AmountInCrores = 825.0m, PercentageOfTotal = 19.09m },
                    new() { Category = ObjectiveCategory.OfferForSale, Title = "Offer for Sale by Promoters", Description = "Secondary promoter share monetization.", AmountInCrores = 721.44m, PercentageOfTotal = 16.70m }
                };
                waareeIpo.Risks = new List<IPORisk>
                {
                    new() { Category = "GLOBAL TRADE POLICY", Title = "US Tariffs & Export Dependency", Description = "Over 68% of export revenues originate from the US market subject to trade policy shifts.", Severity = RiskSeverity.Medium, TraceableMetric = "US Export Share = 68%" },
                    new() { Category = "RAW MATERIAL CYCLICALITY", Title = "Polysilicon & Wafer Price Volatility", Description = "Fluctuating raw material input prices may compress EBITDA margins.", Severity = RiskSeverity.Medium, TraceableMetric = "Raw Material Cost = 72% of Sales" }
                };
                waaree.Ipos.Add(waareeIpo);

                // 3. Bajaj Housing Finance Limited
                var bajajHfl = new Company
                {
                    Name = "Bajaj Housing Finance Limited",
                    LegalName = "Bajaj Housing Finance Limited",
                    CIN = "U65922PN2008PLC132217",
                    Symbol = "BAJAJHFL",
                    Sector = "Financial Services",
                    Industry = "Housing Finance & NBFC",
                    Description = "A 100% subsidiary of Bajaj Finance, offering prime housing loans, developer finance, and loan against property with an AUM exceeding ₹97,000 Cr.",
                    Website = "https://www.bajajhousingfinance.in",
                    FoundedYear = 2008,
                    Headquarters = "Pune, Maharashtra",
                    PromoterInformation = "Bajaj Finance Limited & Bajaj Finserv Limited",
                    ManagingDirector = "Atul Jain",
                    PromoterHoldingPreIssue = 100.0m,
                    PromoterHoldingPostIssue = 88.75m
                };
                bajajHfl.Financials = new List<CompanyFinancial>
                {
                    new() { FiscalYear = "FY2022", PeriodEnding = new DateTime(2022, 3, 31), Revenue = 3767.0m, EBITDA = 1110.0m, EBIT = 1050.0m, PAT = 710.0m, EPS = 1.07m, OperatingCashFlow = -4500.0m, FreeCashFlow = -4550.0m, TotalAssets = 53000.0m, TotalDebt = 44000.0m, NetWorth = 6500.0m, CurrentAssets = 12000.0m, CurrentLiabilities = 10500.0m, EbitdaMargin = 29.47m, PatMargin = 18.85m, ROE = 11.5m, DebtToEquity = 6.77m, CurrentRatio = 1.14m },
                    new() { FiscalYear = "FY2023", PeriodEnding = new DateTime(2023, 3, 31), Revenue = 5665.0m, EBITDA = 1920.0m, EBIT = 1850.0m, PAT = 1258.0m, EPS = 1.89m, OperatingCashFlow = -6200.0m, FreeCashFlow = -6280.0m, TotalAssets = 71000.0m, TotalDebt = 59000.0m, NetWorth = 9200.0m, CurrentAssets = 15000.0m, CurrentLiabilities = 13200.0m, EbitdaMargin = 33.89m, PatMargin = 22.21m, ROE = 14.6m, DebtToEquity = 6.41m, CurrentRatio = 1.14m },
                    new() { FiscalYear = "FY2024", PeriodEnding = new DateTime(2024, 3, 31), Revenue = 7617.0m, EBITDA = 2650.0m, EBIT = 2540.0m, PAT = 1731.0m, EPS = 2.60m, OperatingCashFlow = -8400.0m, FreeCashFlow = -8500.0m, TotalAssets = 97000.0m, TotalDebt = 78000.0m, NetWorth = 12200.0m, CurrentAssets = 21000.0m, CurrentLiabilities = 18500.0m, EbitdaMargin = 34.79m, PatMargin = 22.73m, ROE = 15.2m, DebtToEquity = 6.39m, CurrentRatio = 1.14m }
                };

                var bajajHflIpo = new IPO
                {
                    Name = "Bajaj Housing Finance IPO",
                    Symbol = "BAJAJHFL",
                    IpoType = IpoType.Mainboard,
                    Status = IpoStatus.Listed,
                    OpenDate = new DateTime(2024, 9, 9),
                    CloseDate = new DateTime(2024, 9, 11),
                    AllotmentDate = new DateTime(2024, 9, 12),
                    ListingDate = new DateTime(2024, 9, 16),
                    PriceBandLow = 66m,
                    PriceBandHigh = 70m,
                    LotSize = 214,
                    MinimumInvestment = 14980m,
                    IssueSize = 6560.0m,
                    FreshIssueAmount = 3560.0m,
                    OFSAmount = 3000.0m,
                    FaceValue = 10m,
                    Registrar = "KFin Technologies Limited",
                    LeadManagers = "Kotak Mahindra Capital, BofA Securities, Axis Capital, Goldman Sachs, SBI Capital",
                    Exchange = "BSE, NSE",
                    ListingPrice = 150.0m,
                    ListingGainPercent = 114.29m,
                    Day1ClosePrice = 165.0m
                };
                bajajHflIpo.GmpHistories = new List<IPOGmpHistory>
                {
                    new() { GMP = 55m, GMPPercentage = 78.57m, EstimatedListingPrice = 125m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 9, 6), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 68m, GMPPercentage = 97.14m, EstimatedListingPrice = 138m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 9, 9), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 75m, GMPPercentage = 107.14m, EstimatedListingPrice = 145m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 9, 11), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 79m, GMPPercentage = 112.86m, EstimatedListingPrice = 149m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 9, 14), RetrievedAt = DateTime.UtcNow }
                };
                bajajHflIpo.SubscriptionHistories = new List<IPOSubscriptionHistory>
                {
                    new() { RetailSubscription = 1.5m, QibSubscription = 1.1m, NiiSubscription = 4.3m, TotalSubscription = 2.0m, DayNumber = 1, SnapshotDate = new DateTime(2024, 9, 9) },
                    new() { RetailSubscription = 3.8m, QibSubscription = 7.2m, NiiSubscription = 16.5m, TotalSubscription = 7.5m, DayNumber = 2, SnapshotDate = new DateTime(2024, 9, 10) },
                    new() { RetailSubscription = 7.4m, QibSubscription = 222.0m, NiiSubscription = 43.5m, TotalSubscription = 67.4m, DayNumber = 3, SnapshotDate = new DateTime(2024, 9, 11) }
                };
                bajajHflIpo.Objectives = new List<IPOObjective>
                {
                    new() { Category = ObjectiveCategory.Expansion, Title = "Augment Tier-I Capital Base", Description = "Meet future business capital requirements and onward lending.", AmountInCrores = 3560.0m, PercentageOfTotal = 54.27m },
                    new() { Category = ObjectiveCategory.OfferForSale, Title = "Offer for Sale by Bajaj Finance", Description = "Parent company equity monetization per RBI scale-based regulation.", AmountInCrores = 3000.0m, PercentageOfTotal = 45.73m }
                };
                bajajHflIpo.Risks = new List<IPORisk>
                {
                    new() { Category = "REGULATORY COMPLIANCE", Title = "RBI Scale-Based Regulatory Mandates", Description = "Strict regulatory capital adequacy and provisioning requirements.", Severity = RiskSeverity.Low, TraceableMetric = "Capital Adequacy Ratio = 21.3%" }
                };
                bajajHfl.Ipos.Add(bajajHflIpo);

                // 4. Swiggy Limited (Active / Recently Listed)
                var swiggy = new Company
                {
                    Name = "Swiggy Limited",
                    LegalName = "Swiggy Limited",
                    CIN = "U74110KA2013PLC096536",
                    Symbol = "SWIGGY",
                    Sector = "Consumer Discretionary",
                    Industry = "Quick Commerce & Delivery",
                    Description = "India's pioneering on-demand convenience platform operating across food delivery, Instamart quick commerce, Dineout, and Genie logistics.",
                    Website = "https://www.swiggy.com",
                    FoundedYear = 2013,
                    Headquarters = "Bengaluru, Karnataka",
                    PromoterInformation = "Professionally Managed Company (No identifiable promoter)",
                    ManagingDirector = "Sriharsha Majety",
                    PromoterHoldingPreIssue = 0m,
                    PromoterHoldingPostIssue = 0m
                };
                swiggy.Financials = new List<CompanyFinancial>
                {
                    new() { FiscalYear = "FY2022", PeriodEnding = new DateTime(2022, 3, 31), Revenue = 5705.0m, EBITDA = -3240.0m, EBIT = -3420.0m, PAT = -3628.0m, EPS = -18.2m, OperatingCashFlow = -2850.0m, FreeCashFlow = -3100.0m, TotalAssets = 10400.0m, TotalDebt = 210.0m, NetWorth = 6800.0m, CurrentAssets = 7800.0m, CurrentLiabilities = 3100.0m, EbitdaMargin = -56.79m, PatMargin = -63.59m, ROE = -53.3m, DebtToEquity = 0.03m, CurrentRatio = 2.52m },
                    new() { FiscalYear = "FY2023", PeriodEnding = new DateTime(2023, 3, 31), Revenue = 8265.0m, EBITDA = -3410.0m, EBIT = -3650.0m, PAT = -4179.0m, EPS = -20.1m, OperatingCashFlow = -2450.0m, FreeCashFlow = -2720.0m, TotalAssets = 11200.0m, TotalDebt = 180.0m, NetWorth = 5400.0m, CurrentAssets = 8200.0m, CurrentLiabilities = 3800.0m, EbitdaMargin = -41.26m, PatMargin = -50.56m, ROE = -77.3m, DebtToEquity = 0.03m, CurrentRatio = 2.16m },
                    new() { FiscalYear = "FY2024", PeriodEnding = new DateTime(2024, 3, 31), Revenue = 11247.0m, EBITDA = -1870.0m, EBIT = -2150.0m, PAT = -2350.0m, EPS = -10.8m, OperatingCashFlow = -1180.0m, FreeCashFlow = -1420.0m, TotalAssets = 12500.0m, TotalDebt = 150.0m, NetWorth = 7200.0m, CurrentAssets = 9400.0m, CurrentLiabilities = 4100.0m, EbitdaMargin = -16.63m, PatMargin = -20.89m, ROE = -32.6m, DebtToEquity = 0.02m, CurrentRatio = 2.29m }
                };

                var swiggyIpo = new IPO
                {
                    Name = "Swiggy IPO",
                    Symbol = "SWIGGY",
                    IpoType = IpoType.Mainboard,
                    Status = IpoStatus.Listed,
                    OpenDate = new DateTime(2024, 11, 6),
                    CloseDate = new DateTime(2024, 11, 8),
                    AllotmentDate = new DateTime(2024, 11, 11),
                    ListingDate = new DateTime(2024, 11, 13),
                    PriceBandLow = 371m,
                    PriceBandHigh = 390m,
                    LotSize = 38,
                    MinimumInvestment = 14820m,
                    IssueSize = 11327.43m,
                    FreshIssueAmount = 4499.0m,
                    OFSAmount = 6828.43m,
                    FaceValue = 1m,
                    Registrar = "Link Intime India Private Ltd",
                    LeadManagers = "Kotak Mahindra, Citigroup, Jefferies, Avendus, BofA Securities, JP Morgan",
                    Exchange = "BSE, NSE",
                    ListingPrice = 420.0m,
                    ListingGainPercent = 7.69m,
                    Day1ClosePrice = 456.0m
                };
                swiggyIpo.GmpHistories = new List<IPOGmpHistory>
                {
                    new() { GMP = 25m, GMPPercentage = 6.41m, EstimatedListingPrice = 415m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 11, 3), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 18m, GMPPercentage = 4.62m, EstimatedListingPrice = 408m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 11, 6), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 12m, GMPPercentage = 3.08m, EstimatedListingPrice = 402m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 11, 8), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 2m, GMPPercentage = 0.51m, EstimatedListingPrice = 392m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 11, 11), RetrievedAt = DateTime.UtcNow }
                };
                swiggyIpo.SubscriptionHistories = new List<IPOSubscriptionHistory>
                {
                    new() { RetailSubscription = 0.6m, QibSubscription = 0.1m, NiiSubscription = 0.2m, TotalSubscription = 0.35m, DayNumber = 1, SnapshotDate = new DateTime(2024, 11, 6) },
                    new() { RetailSubscription = 0.9m, QibSubscription = 0.3m, NiiSubscription = 0.5m, TotalSubscription = 0.65m, DayNumber = 2, SnapshotDate = new DateTime(2024, 11, 7) },
                    new() { RetailSubscription = 1.14m, QibSubscription = 6.02m, NiiSubscription = 1.24m, TotalSubscription = 3.59m, DayNumber = 3, SnapshotDate = new DateTime(2024, 11, 8) }
                };
                swiggyIpo.Objectives = new List<IPOObjective>
                {
                    new() { Category = ObjectiveCategory.Expansion, Title = "Expansion of Dark Store Network (Instamart)", Description = "Invest in dark store infrastructure and supply chain leases.", AmountInCrores = 1178.7m, PercentageOfTotal = 10.41m },
                    new() { Category = ObjectiveCategory.Expansion, Title = "Technology & Cloud Infrastructure", Description = "R&D in generative AI, real-time routing algorithms, and systems.", AmountInCrores = 703.4m, PercentageOfTotal = 6.21m },
                    new() { Category = ObjectiveCategory.GeneralCorporate, Title = "Brand Marketing & General Corporate", Description = "User acquisition and advertising campaigns.", AmountInCrores = 1115.3m, PercentageOfTotal = 9.85m },
                    new() { Category = ObjectiveCategory.OfferForSale, Title = "Offer for Sale by PE Investors (Prosus, Softbank)", Description = "Secondary liquidity for early fund backers.", AmountInCrores = 6828.43m, PercentageOfTotal = 60.28m }
                };
                swiggyIpo.Risks = new List<IPORisk>
                {
                    new() { Category = "PROFITABILITY RISK", Title = "History of Net Losses", Description = "Swiggy has recorded continuous net losses (₹2,350 Cr in FY24) and negative operating cash flows.", Severity = RiskSeverity.High, TraceableMetric = "FY24 PAT = -₹2,350 Cr" },
                    new() { Category = "INTENSE COMPETITION", Title = "Duopoly & Quick Commerce Price Wars", Description = "High competition from Zomato/Blinkit and Zepto in 10-minute delivery unit economics.", Severity = RiskSeverity.High, TraceableMetric = "EBITDA Margin = -16.6%" },
                    new() { Category = "ISSUE STRUCTURE", Title = "High OFS Proportion (>60%)", Description = "Majority of issue consists of investor exits rather than growth capital.", Severity = RiskSeverity.Medium, TraceableMetric = "OFS = 60.3%" }
                };
                swiggy.Ipos.Add(swiggyIpo);

                // 5. Ather Energy Limited (Open / Upcoming IPO)
                var ather = new Company
                {
                    Name = "Ather Energy Limited",
                    LegalName = "Ather Energy Limited",
                    CIN = "U34100KA2013PLC071536",
                    Symbol = "ATHER",
                    Sector = "Automobile",
                    Industry = "Electric Two-Wheelers",
                    Description = "Pure-play Indian EV manufacturer designing premium performance electric scooters (Ather 450X, Rizta) and building the nationwide Ather Grid charging network.",
                    Website = "https://www.atherenergy.com",
                    FoundedYear = 2013,
                    Headquarters = "Bengaluru, Karnataka",
                    PromoterInformation = "Tarun Mehta & Swapnil Jain (Supported by Hero MotoCorp as single largest shareholder)",
                    ManagingDirector = "Tarun Mehta",
                    PromoterHoldingPreIssue = 18.2m,
                    PromoterHoldingPostIssue = 14.5m
                };
                ather.Financials = new List<CompanyFinancial>
                {
                    new() { FiscalYear = "FY2022", PeriodEnding = new DateTime(2022, 3, 31), Revenue = 408.0m, EBITDA = -320.0m, EBIT = -340.0m, PAT = -344.0m, EPS = -12.5m, OperatingCashFlow = -310.0m, FreeCashFlow = -450.0m, TotalAssets = 1200.0m, TotalDebt = 140.0m, NetWorth = 450.0m, CurrentAssets = 680.0m, CurrentLiabilities = 410.0m, EbitdaMargin = -78.43m, PatMargin = -84.31m, ROE = -76.4m, DebtToEquity = 0.31m, CurrentRatio = 1.66m },
                    new() { FiscalYear = "FY2023", PeriodEnding = new DateTime(2023, 3, 31), Revenue = 1783.0m, EBITDA = -680.0m, EBIT = -740.0m, PAT = -864.0m, EPS = -28.0m, OperatingCashFlow = -580.0m, FreeCashFlow = -820.0m, TotalAssets = 2400.0m, TotalDebt = 220.0m, NetWorth = 920.0m, CurrentAssets = 1350.0m, CurrentLiabilities = 890.0m, EbitdaMargin = -38.14m, PatMargin = -48.46m, ROE = -93.9m, DebtToEquity = 0.24m, CurrentRatio = 1.52m },
                    new() { FiscalYear = "FY2024", PeriodEnding = new DateTime(2024, 3, 31), Revenue = 1789.0m, EBITDA = -710.0m, EBIT = -790.0m, PAT = -1059.0m, EPS = -31.4m, OperatingCashFlow = -620.0m, FreeCashFlow = -890.0m, TotalAssets = 2950.0m, TotalDebt = 310.0m, NetWorth = 780.0m, CurrentAssets = 1620.0m, CurrentLiabilities = 1120.0m, EbitdaMargin = -39.69m, PatMargin = -59.20m, ROE = -135.7m, DebtToEquity = 0.40m, CurrentRatio = 1.45m }
                };

                var atherIpo = new IPO
                {
                    Name = "Ather Energy IPO",
                    Symbol = "ATHER",
                    IpoType = IpoType.Mainboard,
                    Status = IpoStatus.Listed,
                    OpenDate = new DateTime(2024, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                    CloseDate = new DateTime(2024, 12, 12, 0, 0, 0, DateTimeKind.Utc),
                    AllotmentDate = new DateTime(2024, 12, 13, 0, 0, 0, DateTimeKind.Utc),
                    ListingDate = new DateTime(2024, 12, 17, 0, 0, 0, DateTimeKind.Utc),
                    ListingPrice = 410m,
                    ListingGainPercent = 25.00m,
                    PriceBandLow = 310m,
                    PriceBandHigh = 328m,
                    LotSize = 45,
                    MinimumInvestment = 14760m,
                    IssueSize = 4500.0m,
                    FreshIssueAmount = 3100.0m,
                    OFSAmount = 1400.0m,
                    FaceValue = 1m,
                    Registrar = "Link Intime India Private Ltd",
                    LeadManagers = "Axis Capital, HSBC, Nomura, JM Financial",
                    Exchange = "BSE, NSE"
                };
                atherIpo.GmpHistories = new List<IPOGmpHistory>
                {
                    new() { GMP = 65m, GMPPercentage = 19.82m, EstimatedListingPrice = 393m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 12, 9), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 82m, GMPPercentage = 25.00m, EstimatedListingPrice = 410m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 12, 12), RetrievedAt = DateTime.UtcNow }
                };
                atherIpo.SubscriptionHistories = new List<IPOSubscriptionHistory>
                {
                    new() { RetailSubscription = 1.25m, QibSubscription = 0.45m, NiiSubscription = 1.82m, TotalSubscription = 1.10m, DayNumber = 1, SnapshotDate = new DateTime(2024, 12, 12) }
                };
                atherIpo.Objectives = new List<IPOObjective>
                {
                    new() { Category = ObjectiveCategory.Expansion, Title = "Establishment of Electric Two-Wheeler Factory in Maharashtra (Plant 3)", Description = "Capex outlays to expand annual manufacturing capacity to 1 million units.", AmountInCrores = 927.2m, PercentageOfTotal = 20.6m },
                    new() { Category = ObjectiveCategory.Expansion, Title = "Investment in R&D and Battery Pack Architecture", Description = "Next-generation cell chemistries and software stack.", AmountInCrores = 750.0m, PercentageOfTotal = 16.67m },
                    new() { Category = ObjectiveCategory.WorkingCapital, Title = "Marketing and Fast Charging Infrastructure", Description = "Ather Grid expansion across Tier 2 and Tier 3 cities.", AmountInCrores = 400.0m, PercentageOfTotal = 8.89m },
                    new() { Category = ObjectiveCategory.OfferForSale, Title = "Offer for Sale by Existing Shareholders", Description = "Partial monetization by early venture funds.", AmountInCrores = 1400.0m, PercentageOfTotal = 31.11m }
                };
                atherIpo.Risks = new List<IPORisk>
                {
                    new() { Category = "PROFITABILITY RISK", Title = "Continued Operating Losses", Description = "Ather is in heavy investment mode with net losses of ₹1,059 Cr in FY24.", Severity = RiskSeverity.High, TraceableMetric = "FY24 Net Loss = ₹1,059 Cr" },
                    new() { Category = "POLICY DEPENDENCE", Title = "Reduction in EMPS / FAME Government Subsidies", Description = "Changes in electric mobility subsidies directly impact unit retail pricing and customer adoption speed.", Severity = RiskSeverity.Medium, TraceableMetric = "Subsidy Share = 15% of ASP" }
                };
                ather.Ipos.Add(atherIpo);

                // 6. NTPC Green Energy Limited (Listed IPO)
                var ntpcGreen = new Company
                {
                    Name = "NTPC Green Energy Limited",
                    LegalName = "NTPC Green Energy Limited",
                    CIN = "U40100DL2022GOI396349",
                    Symbol = "NTPCGREEN",
                    Sector = "Energy & Utilities",
                    Industry = "Solar & Renewable Equipment",
                    Description = "Wholly owned renewable energy subsidiary of Maharatna PSU NTPC Limited, focused on utility-scale solar and wind projects across India.",
                    Website = "https://www.ngel.in",
                    FoundedYear = 2022,
                    Headquarters = "New Delhi",
                    PromoterInformation = "NTPC Limited (100% Shareholding Pre-Issue)",
                    ManagingDirector = "Gurdeep Singh",
                    PromoterHoldingPreIssue = 100.0m,
                    PromoterHoldingPostIssue = 89.0m
                };
                ntpcGreen.Financials = new List<CompanyFinancial>
                {
                    new() { FiscalYear = "FY2023", PeriodEnding = new DateTime(2023, 3, 31), Revenue = 170.0m, EBITDA = 152.0m, EBIT = 138.0m, PAT = 171.0m, EPS = 0.22m, OperatingCashFlow = 145.0m, FreeCashFlow = -1800.0m, TotalAssets = 9500.0m, TotalDebt = 4800.0m, NetWorth = 4200.0m, CurrentAssets = 1200.0m, CurrentLiabilities = 1500.0m, EbitdaMargin = 89.41m, PatMargin = 100.59m, ROE = 4.07m, DebtToEquity = 1.14m, CurrentRatio = 0.80m },
                    new() { FiscalYear = "FY2024", PeriodEnding = new DateTime(2024, 3, 31), Revenue = 1962.6m, EBITDA = 1746.0m, EBIT = 1540.0m, PAT = 344.7m, EPS = 0.44m, OperatingCashFlow = 1520.0m, FreeCashFlow = -4200.0m, TotalAssets = 25000.0m, TotalDebt = 15200.0m, NetWorth = 7800.0m, CurrentAssets = 3400.0m, CurrentLiabilities = 4100.0m, EbitdaMargin = 88.96m, PatMargin = 17.56m, ROE = 4.42m, DebtToEquity = 1.95m, CurrentRatio = 0.83m }
                };

                var ntpcGreenIpo = new IPO
                {
                    Name = "NTPC Green Energy IPO",
                    Symbol = "NTPCGREEN",
                    IpoType = IpoType.Mainboard,
                    Status = IpoStatus.Listed,
                    OpenDate = new DateTime(2024, 11, 19, 0, 0, 0, DateTimeKind.Utc),
                    CloseDate = new DateTime(2024, 11, 22, 0, 0, 0, DateTimeKind.Utc),
                    AllotmentDate = new DateTime(2024, 11, 25, 0, 0, 0, DateTimeKind.Utc),
                    ListingDate = new DateTime(2024, 11, 27, 0, 0, 0, DateTimeKind.Utc),
                    ListingPrice = 111.5m,
                    ListingGainPercent = 3.24m,
                    PriceBandLow = 102m,
                    PriceBandHigh = 108m,
                    LotSize = 138,
                    MinimumInvestment = 14904m,
                    IssueSize = 10000.0m,
                    FreshIssueAmount = 10000.0m,
                    OFSAmount = 0m,
                    FaceValue = 10m,
                    Registrar = "KFin Technologies Limited",
                    LeadManagers = "IDBI Capital, HDFC Bank, IIFL Securities, Nuvama Wealth",
                    Exchange = "BSE, NSE"
                };
                ntpcGreenIpo.GmpHistories = new List<IPOGmpHistory>
                {
                    new() { GMP = 18m, GMPPercentage = 16.67m, EstimatedListingPrice = 126m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 11, 20), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 3.5m, GMPPercentage = 3.24m, EstimatedListingPrice = 111.5m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 11, 22), RetrievedAt = DateTime.UtcNow }
                };
                ntpcGreenIpo.SubscriptionHistories = new List<IPOSubscriptionHistory>
                {
                    new() { RetailSubscription = 1.33m, QibSubscription = 3.32m, NiiSubscription = 0.81m, TotalSubscription = 2.42m, DayNumber = 3, SnapshotDate = new DateTime(2024, 11, 22) }
                };
                ntpcGreenIpo.Objectives = new List<IPOObjective>
                {
                    new() { Category = ObjectiveCategory.DebtRepayment, Title = "Repayment/Prepayment of Certain Outstanding Borrowings (NTPC Renewable Energy Ltd)", Description = "De-leveraging balance sheet to enhance profit margins.", AmountInCrores = 7500.0m, PercentageOfTotal = 75.0m },
                    new() { Category = ObjectiveCategory.GeneralCorporate, Title = "General Corporate Purposes", Description = "Working capital and bidding security deposits.", AmountInCrores = 2500.0m, PercentageOfTotal = 25.0m }
                };
                ntpcGreenIpo.Risks = new List<IPORisk>
                {
                    new() { Category = "HIGH DEBT & SOLVENCY", Title = "Elevated Leverage Profile (D/E = 1.95x)", Description = "Capital intensive business model with high long-term borrowings.", Severity = RiskSeverity.High, TraceableMetric = "Debt/Equity = 1.95x" },
                    new() { Category = "DISCOM PAYMENT DELAYS", Title = "State Discom Counterparty Risk", Description = "Cash flow exposure to state electricity distribution companies' payment cycles.", Severity = RiskSeverity.Medium, TraceableMetric = "State Discom Receivables = 42%" }
                };
                ntpcGreen.Ipos.Add(ntpcGreenIpo);

                // 7. Apex Solar & Automation (SME Listed IPO)
                var apexSolar = new Company
                {
                    Name = "Apex Solar Automation Limited",
                    LegalName = "Apex Solar Automation Limited",
                    CIN = "U29309GJ2018PLC102914",
                    Symbol = "APEXSOLAR",
                    Sector = "Capital Goods & Automation",
                    Industry = "Industrial Automation & Robotics",
                    Description = "Specialized engineering player manufacturing automated solar panel cleaning robotic systems and solar tracking equipment.",
                    Website = "https://www.apexsolarauto.in",
                    FoundedYear = 2018,
                    Headquarters = "Ahmedabad, Gujarat",
                    PromoterInformation = "Rajesh Patel & Hiren Shah",
                    ManagingDirector = "Rajesh Patel",
                    PromoterHoldingPreIssue = 84.5m,
                    PromoterHoldingPostIssue = 62.0m
                };
                apexSolar.Financials = new List<CompanyFinancial>
                {
                    new() { FiscalYear = "FY2022", PeriodEnding = new DateTime(2022, 3, 31), Revenue = 18.5m, EBITDA = 2.8m, EBIT = 2.4m, PAT = 1.6m, EPS = 3.2m, OperatingCashFlow = 1.9m, FreeCashFlow = 0.8m, TotalAssets = 16.0m, TotalDebt = 3.2m, NetWorth = 7.5m, CurrentAssets = 11.0m, CurrentLiabilities = 5.2m, EbitdaMargin = 15.14m, PatMargin = 8.65m, ROE = 21.3m, DebtToEquity = 0.43m, CurrentRatio = 2.12m },
                    new() { FiscalYear = "FY2023", PeriodEnding = new DateTime(2023, 3, 31), Revenue = 34.2m, EBITDA = 6.1m, EBIT = 5.4m, PAT = 3.8m, EPS = 7.6m, OperatingCashFlow = 4.2m, FreeCashFlow = 2.1m, TotalAssets = 28.0m, TotalDebt = 4.1m, NetWorth = 12.0m, CurrentAssets = 19.5m, CurrentLiabilities = 8.5m, EbitdaMargin = 17.84m, PatMargin = 11.11m, ROE = 31.6m, DebtToEquity = 0.34m, CurrentRatio = 2.29m },
                    new() { FiscalYear = "FY2024", PeriodEnding = new DateTime(2024, 3, 31), Revenue = 58.0m, EBITDA = 11.5m, EBIT = 10.2m, PAT = 7.2m, EPS = 14.4m, OperatingCashFlow = 8.5m, FreeCashFlow = 4.2m, TotalAssets = 45.0m, TotalDebt = 3.8m, NetWorth = 21.0m, CurrentAssets = 32.0m, CurrentLiabilities = 12.0m, EbitdaMargin = 19.83m, PatMargin = 12.41m, ROE = 34.2m, DebtToEquity = 0.18m, CurrentRatio = 2.67m }
                };

                var apexSolarIpo = new IPO
                {
                    Name = "Apex Solar Automation SME IPO",
                    Symbol = "APEXSOLAR",
                    IpoType = IpoType.Sme,
                    Status = IpoStatus.Listed,
                    OpenDate = new DateTime(2024, 6, 24, 0, 0, 0, DateTimeKind.Utc),
                    CloseDate = new DateTime(2024, 6, 26, 0, 0, 0, DateTimeKind.Utc),
                    AllotmentDate = new DateTime(2024, 6, 27, 0, 0, 0, DateTimeKind.Utc),
                    ListingDate = new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                    ListingPrice = 184m,
                    ListingGainPercent = 50.82m,
                    PriceBandLow = 115m,
                    PriceBandHigh = 122m,
                    LotSize = 1000,
                    MinimumInvestment = 122000m,
                    IssueSize = 48.5m,
                    FreshIssueAmount = 48.5m,
                    OFSAmount = 0m,
                    FaceValue = 10m,
                    Registrar = "Bigshare Services Pvt Ltd",
                    LeadManagers = "GYR Capital Advisors",
                    Exchange = "NSE SME"
                };
                apexSolarIpo.GmpHistories = new List<IPOGmpHistory>
                {
                    new() { GMP = 55m, GMPPercentage = 45.08m, EstimatedListingPrice = 177m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 6, 24), RetrievedAt = DateTime.UtcNow },
                    new() { GMP = 62m, GMPPercentage = 50.82m, EstimatedListingPrice = 184m, Source = "Market Aggregator", ObservedAt = new DateTime(2024, 6, 26), RetrievedAt = DateTime.UtcNow }
                };
                apexSolarIpo.SubscriptionHistories = new List<IPOSubscriptionHistory>
                {
                    new() { RetailSubscription = 14.5m, QibSubscription = 3.2m, NiiSubscription = 28.0m, TotalSubscription = 18.2m, DayNumber = 3, SnapshotDate = new DateTime(2024, 6, 26) }
                };
                apexSolarIpo.Objectives = new List<IPOObjective>
                {
                    new() { Category = ObjectiveCategory.Expansion, Title = "New Robotic Assembly Unit in Sanand", Description = "Expansion of robotic arm production lines.", AmountInCrores = 28.0m, PercentageOfTotal = 57.73m },
                    new() { Category = ObjectiveCategory.WorkingCapital, Title = "Working Capital Requirements", Description = "Inventory financing for solar tracker raw steel.", AmountInCrores = 14.5m, PercentageOfTotal = 29.90m },
                    new() { Category = ObjectiveCategory.GeneralCorporate, Title = "General Corporate Purposes", Description = "Corporate overhead and branding.", AmountInCrores = 6.0m, PercentageOfTotal = 12.37m }
                };
                apexSolarIpo.Risks = new List<IPORisk>
                {
                    new() { Category = "SME PLATFORM RISK", Title = "SME Exchange Liquidity Constraints", Description = "High lot size (1,000 shares, ₹1.22 Lakh) limits retail post-listing volume.", Severity = RiskSeverity.Medium, TraceableMetric = "Lot Size = ₹1.22 Lakh" }
                };
                apexSolar.Ipos.Add(apexSolarIpo);

                var allCompanies = new List<Company> { tataTech, waaree, bajajHfl, swiggy, ather, ntpcGreen, apexSolar };
                await context.Companies.AddRangeAsync(allCompanies);
                await context.SaveChangesAsync();

                // Calculate Scores and Populate IPOScores for each seeded IPO
                var industryDict = await context.IndustryMetrics.ToListAsync();
                foreach (var comp in allCompanies)
                {
                    var ind = industryDict.FirstOrDefault(i => i.Sector == comp.Sector);
                    var finReport = financialEngine.AnalyzeFinancials(comp);
                    var latestFin = comp.Financials.OrderBy(f => f.PeriodEnding).LastOrDefault();

                    foreach (var ipo in comp.Ipos)
                    {
                        var valDto = valuationEngine.EvaluateValuation(ipo, latestFin, ind);
                        var gmpDto = gmpService.AnalyzeGmpHistory(ipo);
                        var subHistory = ipo.SubscriptionHistories.OrderBy(s => s.DayNumber).ToList();
                        var latestSub = subHistory.LastOrDefault();
                        var subBreakdown = new Contracts.Subscription.SubscriptionBreakdownDto
                        {
                            IpoId = ipo.Id,
                            IpoName = ipo.Name,
                            LatestTotalSubscription = latestSub?.TotalSubscription ?? 0,
                            LatestQibSubscription = latestSub?.QibSubscription ?? 0,
                            LatestNiiSubscription = latestSub?.NiiSubscription ?? 0,
                            LatestRetailSubscription = latestSub?.RetailSubscription ?? 0
                        };
                        var riskDto = riskEngine.EvaluateRisks(ipo, comp.Financials.ToList(), ind, gmpDto);

                        var scoreBreakdown = scoringEngine.CalculateScore(ipo, finReport, valDto, gmpDto, subBreakdown, ipo.Risks.ToList());

                        var scoreEntity = new IPOScore
                        {
                            IpoId = ipo.Id,
                            ListingGainScore = scoreBreakdown.ListingGainScore,
                            ListingRecommendation = scoreBreakdown.ListingRecommendation,
                            ListingGainVerdict = scoreBreakdown.ListingGainVerdict,
                            LongTermScore = scoreBreakdown.LongTermScore,
                            LongTermRecommendation = scoreBreakdown.LongTermRecommendation,
                            LongTermVerdict = scoreBreakdown.LongTermVerdict,
                            BreakdownJson = JsonSerializer.Serialize(scoreBreakdown),
                            CalculatedAt = DateTime.UtcNow
                        };

                        await context.IPOScores.AddAsync(scoreEntity);
                    }
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Database seeded successfully with {CompanyCount} companies and complete financial & market models.", allCompanies.Count);
            }

            // 4. Seed Roles (Idempotent)
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                var norm = role.ToUpperInvariant();
                if (!await context.Roles.AnyAsync(r => r.NormalizedName == norm || r.Name == role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role) { NormalizedName = norm });
                }
            }

            // 5. Seed Default Admin User (Idempotent)
            var adminEmail = "admin@ipoforge.com";
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpperInvariant(),
                    NormalizedUserName = adminEmail.ToUpperInvariant(),
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@IPOForge2025!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 6. Seed Default Demo User (Idempotent)
            var demoEmail = "investor@ipoforge.com";
            var demoUser = await context.Users.FirstOrDefaultAsync(u => u.Email == demoEmail);
            if (demoUser == null)
            {
                demoUser = new IdentityUser
                {
                    UserName = demoEmail,
                    Email = demoEmail,
                    NormalizedEmail = demoEmail.ToUpperInvariant(),
                    NormalizedUserName = demoEmail.ToUpperInvariant(),
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(demoUser, "Demo@IPOForge2025!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoUser, "User");
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the IPOForge database.");
            throw;
        }
    }
}
