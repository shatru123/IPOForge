export type IpoType = 'Mainboard' | 'Sme';
export type IpoStatus = 'Upcoming' | 'Open' | 'Closed' | 'AllotmentOut' | 'Listed';
export type GmpTrend = 'StronglyIncreasing' | 'Increasing' | 'Stable' | 'Declining' | 'StronglyDeclining';
export type RecommendationRating = 'Strong' | 'Positive' | 'Neutral' | 'Weak' | 'Avoid';
export type ValuationClassification = 'Attractive' | 'Reasonable' | 'Premium' | 'VeryExpensive';
export type RiskSeverity = 'Low' | 'Medium' | 'High' | 'Critical';
export type ObjectiveCategory = 'Expansion' | 'DebtRepayment' | 'WorkingCapital' | 'GeneralCorporate' | 'OfferForSale' | 'Other';

export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  error?: {
    code: string;
    message: string;
    details?: any;
  };
  traceId?: string;
  timestamp: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface IpoSummary {
  id: string;
  name: string;
  symbol?: string;
  companyId: string;
  companyName?: string;
  sector: string;
  industry: string;
  ipoType: IpoType;
  status: IpoStatus;
  openDate?: string;
  closeDate?: string;
  allotmentDate?: string;
  listingDate?: string;
  priceBandLow?: number;
  priceBandHigh?: number;
  issuePrice?: number;
  lotSize?: number;
  minimumInvestment?: number;
  issueSize?: number;
  freshIssueAmount?: number;
  ofsAmount?: number;
  
  // GMP fields (supporting both latestGmp and currentGmp)
  latestGmp?: number;
  currentGmp?: number;
  latestGmpPercentage?: number;
  currentGmpPercentage?: number;
  estimatedListingPrice?: number;
  gmpTrend?: GmpTrend;
  gmp24hChange?: number;

  // Subscription
  totalSubscription?: number;
  qibSubscription?: number;
  niiSubscription?: number;
  retailSubscription?: number;

  // Scores
  listingGainScore: number;
  listingRecommendation: RecommendationRating;
  longTermScore: number;
  longTermRecommendation: RecommendationRating;

  // Valuation & Risk
  valuationClassification?: ValuationClassification;
  highRiskCount?: number;
  companyPE?: number;
  industryPE?: number;

  // Listing Day outcomes
  actualListingPrice?: number;
  listingPrice?: number;
  actualListingGainPercent?: number;
  listingGainPercent?: number;
}

export interface IpoDetail extends IpoSummary {
  legalName?: string;
  cin?: string;
  companyDescription?: string;
  website?: string;
  foundedYear?: number;
  headquarters?: string;
  promoterInformation?: string;
  managingDirector?: string;
  promoterHoldingPreIssue?: number;
  promoterHoldingPostIssue?: number;
  faceValue?: number;
  registrar?: string;
  leadManagers?: string;
  exchange?: string;
  freshIssuePercentage?: number;
  ofsPercentage?: number;
  day1ClosePrice?: number;

  company?: CompanySummary;
  scores?: ScoreBreakdown;
  financialReport?: CompanyFinancialReport;
  valuationAnalysis?: ValuationAnalysis;
  gmpHistory?: GmpHistory;
  subscription?: SubscriptionBreakdown;
  fundUtilization?: FundUtilization;
  businessAnalysis?: BusinessAnalysis;
  riskAnalysis?: RiskAnalysis;
}

export interface IpoFilterRequest {
  search?: string;
  status?: IpoStatus;
  ipoType?: IpoType;
  sector?: string;
  minListingGainScore?: number;
  minLongTermScore?: number;
  minGmpPercentage?: number;
  recommendation?: RecommendationRating;
  sortBy?: string;
  sortDescending?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface IpoSearchDto {
  id: string;
  name: string;
  symbol?: string;
  companyName?: string;
  sector: string;
  ipoType: IpoType;
  status: IpoStatus;
  latestGmpPercentage?: number;
  currentGmp?: number;
  currentGmpPercentage?: number;
  listingGainScore: number;
}

export interface GmpSnapshot {
  id: string;
  gmp: number;
  gmpPercentage: number;
  estimatedListingPrice: number;
  source: string;
  observedAt: string;
}

export interface GmpHistory {
  ipoId: string;
  ipoName: string;
  currentGmp: number;
  currentGmpPercentage: number;
  estimatedListingPrice: number;
  trend: GmpTrend;
  highestGmp: number;
  lowestGmp: number;
  gmp24hChange?: number;
  gmp3dChange?: number;
  gmp7dChange?: number;
  gmpVolatility?: number;
  snapshots: GmpSnapshot[];
}

export interface GmpMover {
  ipoId: string;
  ipoName: string;
  symbol?: string;
  ipoType: IpoType;
  status: IpoStatus;
  currentGmp: number;
  currentGmpPercentage: number;
  changeAmount?: number;
  changePercent?: number;
  trend: GmpTrend;
}

export interface GmpAccuracyItem {
  ipoId: string;
  ipoName: string;
  issuePrice: number;
  finalGmp?: number;
  estimatedListingPrice?: number;
  actualListingPrice: number;
  gmpPredictedGainPercent?: number;
  actualListingGainPercent: number;
  predictionErrorPercent?: number;
  listingDate?: string;
}

export interface GmpAccuracyAnalytics {
  totalListedIposAnalyzed: number;
  averagePredictionErrorPercent: number;
  within10PercentAccuracyRate: number;
  within20PercentAccuracyRate: number;
  within5PercentAccuracyRate?: number;
  historicalComparison: GmpAccuracyItem[];
}

export interface SubscriptionSnapshot {
  id: string;
  dayNumber: number;
  retailSubscription: number;
  qibSubscription: number;
  niiSubscription: number;
  totalSubscription: number;
  snapshotDate: string;
}

export interface SubscriptionBreakdown {
  ipoId: string;
  ipoName: string;
  latestTotalSubscription: number;
  latestQibSubscription: number;
  latestNiiSubscription: number;
  latestRetailSubscription: number;
  demandQualityVerdict?: string;
  history: SubscriptionSnapshot[];
  days?: SubscriptionSnapshot[];
}

export interface FinancialYear {
  fiscalYear: string;
  periodEnding?: string;
  revenue: number;
  ebitda: number;
  ebit?: number;
  pat: number;
  eps?: number;
  operatingCashFlow: number;
  freeCashFlow?: number;
  totalAssets?: number;
  totalDebt?: number;
  netWorth?: number;
  ebitdaMargin: number;
  patMargin: number;
  roe: number;
  roce?: number;
  debtToEquity: number;
  currentRatio?: number;
}

export interface FinancialGrowth {
  revenueGrowthYoY?: number;
  profitGrowthYoY?: number;
  revenueCagr3Year?: number;
  profitCagr3Year?: number;
  ebitdaCagr3Year?: number;
  isConsistentlyProfitable?: boolean;
  marginTrend?: string;
  cashFlowTrend?: string;
  profitabilityVerdict?: string;
  solvencyVerdict?: string;
}

export interface CompanyFinancialReport {
  companyId: string;
  companyName: string;
  years: FinancialYear[];
  growthAnalysis: FinancialGrowth;
}

export interface ValuationAnalysis {
  ipoId: string;
  companyPE?: number;
  industryPE?: number;
  companyPB?: number;
  industryPB?: number;
  companyEvEbitda?: number;
  classification: ValuationClassification;
  valuationDiscountOrPremiumPercent?: number;
  summaryText?: string;
  summary?: string;
}

export interface ObjectiveItem {
  id: string;
  category: ObjectiveCategory;
  title: string;
  description: string;
  amountInCrores: number;
  percentage: number;
  percentageOfTotal?: number;
}

export interface FundUtilization {
  ipoId: string;
  totalIssueSize: number;
  freshIssueAmount: number;
  freshIssuePercent: number;
  ofsAmount: number;
  ofsPercent: number;
  ofsAssessment?: string;
  objectives: ObjectiveItem[];
}

export interface BusinessAnalysis {
  companyId?: string;
  whatCompanyDoes: string;
  howItMakesMoney: string;
  mainProductsServices: string[];
  customerTypes: string[];
  competitivePosition: string;
  growthDrivers: string[];
  keyStrengths: string[];
}

export interface RiskItem {
  id: string;
  category: string;
  title: string;
  description: string;
  severity: RiskSeverity;
  traceableMetric?: string;
}

export interface RiskAnalysis {
  ipoId: string;
  overallRiskLevel: string;
  criticalRiskCount?: number;
  highRiskCount: number;
  mediumRiskCount: number;
  lowRiskCount: number;
  identifiedRisks: RiskItem[];
}

export interface ScorePillar {
  name: string;
  score: number;
  maxScore: number;
  reason: string;
}

export interface ScoreBreakdown {
  ipoId: string;
  calculatedAt?: string;
  listingGainScore: number;
  listingRecommendation: RecommendationRating;
  listingGainVerdict: string;
  listingGainPillars: ScorePillar[];
  longTermScore: number;
  longTermRecommendation: RecommendationRating;
  longTermVerdict: string;
  longTermPillars: ScorePillar[];
  unifiedAnalyticalConclusion: string;
}

export interface DashboardSummary {
  totalIposCount: number;
  activeOpenIposCount: number;
  upcomingIposCount: number;
  recentlyListedCount: number;
  averageGmpThisMonth: number;
  averageSubscriptionMultiple: number;
  openIpos: IpoSummary[];
  upcomingIpos: IpoSummary[];
  recentlyListedIpos: IpoSummary[];
  topGmpGainers: GmpMover[];
  highPotentialListingIpos?: IpoSummary[];
  highPotentialLongTermIpos?: IpoSummary[];
  highRiskIpos?: IpoSummary[];
}

export interface CompanySummary {
  id: string;
  name: string;
  legalName: string;
  cin?: string;
  symbol?: string;
  sector: string;
  industry: string;
  description: string;
  website?: string;
  foundedYear?: number;
  headquarters?: string;
  promoterInformation?: string;
  managingDirector?: string;
  promoterHoldingPreIssue?: number;
  promoterHoldingPostIssue?: number;
}

export interface CompanyDetail extends CompanySummary {
  financials: FinancialYear[];
  ipos: IpoSummary[];
}

export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  role: string;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: UserProfile;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface WatchlistItem {
  id: string;
  ipoId: string;
  ipoName: string;
  symbol?: string;
  sector: string;
  ipoType: IpoType;
  status: IpoStatus;
  currentGmp?: number;
  latestGmp?: number;
  currentGmpPercentage?: number;
  latestGmpPercentage?: number;
  listingGainScore: number;
  longTermScore: number;
  targetListingPrice?: number;
  notes?: string;
  addedAt: string;
}

export interface DataSourceStatus {
  id: string;
  name: string;
  providerKey: string;
  sourceType: string;
  baseUrl: string;
  isActive: boolean;
  lastSyncAt?: string;
  healthStatus: string;
}

export interface DataRefreshLogDto {
  id: string;
  source: string;
  status: string;
  recordsUpdated: number;
  errorMessage?: string;
  durationMs: number;
  executedAt: string;
}

export interface AdminSyncStatus {
  lastSyncTime?: string;
  dataSources: DataSourceStatus[];
  recentLogs: DataRefreshLogDto[];
}
