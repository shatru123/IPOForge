import {
  AdminSyncStatus,
  ApiResponse,
  AuthResponse,
  CompanyDetail,
  CompanySummary,
  DashboardSummary,
  GmpAccuracyAnalytics,
  GmpHistory,
  GmpMover,
  IpoDetail,
  IpoFilterRequest,
  IpoSearchDto,
  IpoSummary,
  LoginRequest,
  PagedResult,
  RegisterRequest,
  ScoreBreakdown,
  SubscriptionBreakdown,
  CompanyFinancialReport,
  WatchlistItem,
} from '../types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

async function request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('ipoforge_token');
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  const json: ApiResponse<T> = await response.json();

  if (!response.ok || !json.success) {
    throw new Error(json.error?.message || 'An unexpected error occurred.');
  }

  return json.data as T;
}

export const api = {
  // Dashboard
  getDashboardSummary: () => request<DashboardSummary>('/api/dashboard'),

  // IPOs
  getIpos: (filters: IpoFilterRequest = {}) => {
    const params = new URLSearchParams();
    if (filters.search) params.append('Search', filters.search);
    if (filters.status) params.append('Status', filters.status);
    if (filters.ipoType) params.append('IpoType', filters.ipoType);
    if (filters.sector) params.append('Sector', filters.sector);
    if (filters.minListingGainScore) params.append('MinListingScore', filters.minListingGainScore.toString());
    if (filters.minLongTermScore) params.append('MinLongTermScore', filters.minLongTermScore.toString());
    if (filters.minGmpPercentage) params.append('MinGmpPercent', filters.minGmpPercentage.toString());
    if (filters.recommendation) params.append('Recommendation', filters.recommendation);
    if (filters.sortBy) params.append('SortBy', filters.sortBy);
    if (filters.sortDescending !== undefined) params.append('SortDescending', filters.sortDescending.toString());
    if (filters.pageNumber) params.append('PageNumber', filters.pageNumber.toString());
    if (filters.pageSize) params.append('PageSize', filters.pageSize.toString());

    return request<PagedResult<IpoSummary>>(`/api/ipos?${params.toString()}`);
  },

  // Deep detail aggregator
  getIpoById: async (id: string): Promise<IpoDetail> => {
    const baseIpo = await request<IpoDetail>(`/api/ipos/${id}`);

    // Fetch related analytics in parallel
    const [gmpRes, subRes, finRes, analysisRes, scoreRes] = await Promise.allSettled([
      request<GmpHistory>(`/api/ipos/${id}/gmp`),
      request<SubscriptionBreakdown>(`/api/ipos/${id}/subscriptions`),
      request<CompanyFinancialReport>(`/api/ipos/${id}/financials`),
      request<any>(`/api/ipos/${id}/analysis`),
      request<ScoreBreakdown>(`/api/ipos/${id}/score`),
    ]);

    const gmpHistory = gmpRes.status === 'fulfilled' ? gmpRes.value : undefined;
    const subscription = subRes.status === 'fulfilled' ? subRes.value : undefined;
    const financialReport = finRes.status === 'fulfilled' ? finRes.value : undefined;
    const analysis = analysisRes.status === 'fulfilled' ? analysisRes.value : undefined;
    const scores = scoreRes.status === 'fulfilled' ? scoreRes.value : analysis?.score;

    return {
      ...baseIpo,
      company: {
        id: baseIpo.companyId,
        name: baseIpo.name,
        legalName: baseIpo.legalName || baseIpo.name,
        cin: baseIpo.cin,
        symbol: baseIpo.symbol,
        sector: baseIpo.sector,
        industry: baseIpo.industry,
        description: baseIpo.companyDescription || '',
        website: baseIpo.website,
        foundedYear: baseIpo.foundedYear,
        headquarters: baseIpo.headquarters,
        promoterInformation: baseIpo.promoterInformation,
        managingDirector: baseIpo.managingDirector,
        promoterHoldingPreIssue: baseIpo.promoterHoldingPreIssue,
        promoterHoldingPostIssue: baseIpo.promoterHoldingPostIssue,
      },
      gmpHistory,
      subscription: subscription ? {
        ...subscription,
        days: subscription.history || subscription.days || [],
      } : undefined,
      financialReport,
      scores,
      valuationAnalysis: analysis?.valuation,
      fundUtilization: analysis?.fundUtilization,
      businessAnalysis: analysis?.business,
      riskAnalysis: analysis?.risks,
    };
  },

  searchIpos: (query: string) => request<IpoSearchDto[]>(`/api/ipos/search?q=${encodeURIComponent(query)}`),
  getUpcomingIpos: () => request<IpoSummary[]>('/api/ipos/upcoming'),
  getOpenIpos: () => request<IpoSummary[]>('/api/ipos/open'),
  getListedIpos: () => request<IpoSummary[]>('/api/ipos/listed'),

  // GMP
  getTopGmpGainers: () => request<GmpMover[]>('/api/gmp/top-movers'),
  getGmpAccuracy: () => request<GmpAccuracyAnalytics>('/api/gmp/accuracy'),

  // Companies
  getCompanies: (search?: string) => {
    const params = search ? `?search=${encodeURIComponent(search)}` : '';
    return request<PagedResult<CompanySummary>>(`/api/companies${params}`);
  },
  getCompanyById: (id: string) => request<CompanyDetail>(`/api/companies/${id}`),

  // Watchlist
  getWatchlist: () => request<WatchlistItem[]>('/api/watchlist'),
  addToWatchlist: (ipoId: string, targetListingPrice?: number, notes?: string) =>
    request<WatchlistItem>('/api/watchlist', {
      method: 'POST',
      body: JSON.stringify({ ipoId, targetListingPrice, notes }),
    }),
  removeFromWatchlist: (ipoId: string) =>
    request<boolean>(`/api/watchlist/${ipoId}`, {
      method: 'DELETE',
    }),

  // Auth
  login: (data: LoginRequest) =>
    request<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  register: (data: RegisterRequest) =>
    request<AuthResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  getMe: () => request<AuthResponse['user']>('/api/auth/me'),

  // Admin
  triggerDataRefresh: (fullSync = true) =>
    request<{ message: string; refreshedAt: string }>(`/api/admin/data-refresh?fullSync=${fullSync}`, {
      method: 'POST',
    }),
  getAdminStatus: () => request<AdminSyncStatus>('/api/admin/status'),
};
