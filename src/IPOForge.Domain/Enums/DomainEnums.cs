namespace IPOForge.Domain.Enums;

public enum IpoType
{
    Mainboard = 1,
    Sme = 2
}

public enum IpoStatus
{
    Upcoming = 1,
    Open = 2,
    Closed = 3,
    AllotmentPending = 4,
    Allotted = 5,
    Listed = 6
}

public enum GmpTrend
{
    StronglyIncreasing = 1,
    Increasing = 2,
    Stable = 3,
    Declining = 4,
    StronglyDeclining = 5
}

public enum RecommendationRating
{
    Strong = 1,     // 80 - 100
    Positive = 2,   // 65 - 79
    Neutral = 3,    // 50 - 64
    Weak = 4,       // 35 - 49
    Avoid = 5       // 0 - 34
}

public enum ValuationClassification
{
    Attractive = 1,
    Reasonable = 2,
    Premium = 3,
    VeryExpensive = 4
}

public enum RiskSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ObjectiveCategory
{
    DebtRepayment = 1,
    Expansion = 2,
    WorkingCapital = 3,
    GeneralCorporate = 4,
    OfferForSale = 5,
    Other = 6
}

public enum DataSourceType
{
    OfficialExchange = 1,
    Registrar = 2,
    FinancialReport = 3,
    MarketAggregator = 4,
    InternalEstimate = 5
}
