using IPOForge.Contracts.Admin;
using IPOForge.Contracts.Analysis;
using IPOForge.Contracts.Auth;
using IPOForge.Contracts.Company;
using IPOForge.Contracts.Common;
using IPOForge.Contracts.Dashboard;
using IPOForge.Contracts.Financial;
using IPOForge.Contracts.Gmp;
using IPOForge.Contracts.Ipo;
using IPOForge.Contracts.Subscription;
using IPOForge.Contracts.Watchlist;
using IPOForge.Domain.Entities;
using IPOForge.Domain.Enums;

namespace IPOForge.Application.Interfaces;

public interface IIpoScoringEngine
{
    ScoreBreakdownDto CalculateScore(IPO ipo, CompanyFinancialReportDto? financials, ValuationAnalysisDto? valuation, GmpHistoryDto? gmpHistory, SubscriptionBreakdownDto? subscription, IReadOnlyList<IPORisk>? risks);
}

public interface IValuationEngine
{
    ValuationAnalysisDto EvaluateValuation(IPO ipo, CompanyFinancial? latestFinancial, IndustryMetric? industryMetric);
}

public interface IRiskEngine
{
    RiskAnalysisDto EvaluateRisks(IPO ipo, IReadOnlyList<CompanyFinancial> financials, IndustryMetric? industryMetric, GmpHistoryDto? gmpHistory);
}

public interface IFinancialAnalysisEngine
{
    CompanyFinancialReportDto AnalyzeFinancials(Company company);
}

public interface IGmpAnalyticsService
{
    GmpHistoryDto AnalyzeGmpHistory(IPO ipo);
    IReadOnlyList<GmpMoverDto> GetTopMovers(IReadOnlyList<IPO> ipos, int count = 5);
    GmpAccuracyAnalyticsDto GetAccuracyAnalytics(IReadOnlyList<IPO> listedIpos);
}

public interface IFundUtilizationService
{
    FundUtilizationDto AnalyzeFundUtilization(IPO ipo);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public interface IAiAnalysisProvider
{
    Task<string> SummarizeBusinessModelAsync(string companyName, string sector, string description, CancellationToken cancellationToken = default);
    Task<string> SummarizeKeyRisksAsync(string companyName, IReadOnlyList<string> rawRiskStatements, CancellationToken cancellationToken = default);
}

public interface IIpoService
{
    Task<PagedResult<IpoSummaryDto>> GetIposAsync(IpoFilterRequest filter, CancellationToken cancellationToken = default);
    Task<IpoDetailDto?> GetIpoByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IpoSearchDto>> SearchIposAsync(string query, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IpoSummaryDto>> GetUpcomingIposAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IpoSummaryDto>> GetOpenIposAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IpoSummaryDto>> GetListedIposAsync(CancellationToken cancellationToken = default);
    Task<GmpHistoryDto?> GetGmpHistoryAsync(Guid ipoId, CancellationToken cancellationToken = default);
    Task<SubscriptionBreakdownDto?> GetSubscriptionAsync(Guid ipoId, CancellationToken cancellationToken = default);
    Task<CompanyFinancialReportDto?> GetFinancialsAsync(Guid ipoId, CancellationToken cancellationToken = default);
    Task<ComprehensiveAnalysisDto?> GetAnalysisAsync(Guid ipoId, CancellationToken cancellationToken = default);
    Task<ScoreBreakdownDto?> GetScoreAsync(Guid ipoId, CancellationToken cancellationToken = default);
}

public interface ICompanyService
{
    Task<PagedResult<CompanySummaryDto>> GetCompaniesAsync(int page = 1, int pageSize = 20, string? search = null, CancellationToken cancellationToken = default);
    Task<CompanyDetailDto?> GetCompanyByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}

public interface IWatchlistService
{
    Task<IReadOnlyList<WatchlistItemDto>> GetUserWatchlistAsync(string userId, CancellationToken cancellationToken = default);
    Task<WatchlistItemDto> AddToWatchlistAsync(string userId, AddToWatchlistRequest request, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromWatchlistAsync(string userId, Guid ipoId, CancellationToken cancellationToken = default);
    Task<bool> UpdateWatchlistItemAsync(string userId, Guid ipoId, UpdateWatchlistRequest request, CancellationToken cancellationToken = default);
}

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserProfileDto?> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
}

public interface IGmpDataProvider
{
    Task<IReadOnlyCollection<IPOGmpHistory>> GetLatestGmpAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<IPOGmpHistory>> GetGmpHistoryAsync(string ipoSymbol, CancellationToken cancellationToken = default);
}

public interface ISubscriptionDataProvider
{
    Task<IReadOnlyCollection<IPOSubscriptionHistory>> GetLiveSubscriptionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<IPOSubscriptionHistory>> GetSubscriptionHistoryAsync(string ipoSymbol, CancellationToken cancellationToken = default);
}

public interface IIpoDataProvider
{
    Task<IReadOnlyCollection<IPO>> GetUpcomingAndOpenIposAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<IPO>> GetListedIposAsync(CancellationToken cancellationToken = default);
}

public interface IDataRefreshService
{
    Task<DataRefreshStatusDto> RefreshMarketDataAsync(DataRefreshRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DataRefreshLog>> GetRefreshLogsAsync(int limit = 50, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DataSource>> GetDataSourcesAsync(CancellationToken cancellationToken = default);
}
