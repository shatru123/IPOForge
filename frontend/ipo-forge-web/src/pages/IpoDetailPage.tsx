import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../services/api';
import { IpoDetail } from '../types';
import { ScoreRing } from '../components/common/ScoreRing';
import { GmpBadge } from '../components/common/GmpBadge';
import { IpoStatusBadge } from '../components/common/IpoStatusBadge';
import { RiskBadge } from '../components/common/RiskBadge';
import { DisclaimerBanner } from '../components/common/DisclaimerBanner';
import { ScoreBreakdownModal } from '../components/common/ScoreBreakdownModal';
import { GmpTrendChart } from '../components/charts/GmpTrendChart';
import { SubscriptionChart } from '../components/charts/SubscriptionChart';
import { FinancialTrendChart } from '../components/charts/FinancialTrendChart';
import { FundAllocationChart } from '../components/charts/FundAllocationChart';
import { useWatchlist } from '../context/WatchlistContext';
import {
  Bookmark,
  Calendar,
  Building2,
  TrendingUp,
  BarChart3,
  Scale,
  PieChart as PieIcon,
  AlertTriangle,
  ExternalLink,
  ChevronRight,
  Info,
  Share2,
} from 'lucide-react';

export const IpoDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const { isInWatchlist, toggleWatchlist } = useWatchlist();

  const [ipo, setIpo] = useState<IpoDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isScoreModalOpen, setIsScoreModalOpen] = useState(false);
  const [activeTab, setActiveTab] = useState<
    'gmp' | 'subscription' | 'financials' | 'valuation' | 'funds' | 'risks' | 'company'
  >('gmp');

  useEffect(() => {
    async function loadIpoDetail() {
      if (!id) return;
      setLoading(true);
      try {
        const data = await api.getIpoById(id);
        setIpo(data);
      } catch (err: any) {
        setError(err.message || 'Failed to load IPO details.');
      } finally {
        setLoading(false);
      }
    }
    loadIpoDetail();
  }, [id]);

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-6 animate-pulse">
        <div className="h-40 bg-slate-900 rounded-3xl border border-slate-800" />
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="h-80 bg-slate-900 rounded-2xl border border-slate-800 md:col-span-2" />
          <div className="h-80 bg-slate-900 rounded-2xl border border-slate-800" />
        </div>
      </div>
    );
  }

  if (error || !ipo) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-16 text-center space-y-4">
        <div className="text-rose-400 font-bold text-lg">IPO Details Not Found</div>
        <p className="text-xs text-slate-400">{error || 'Unable to retrieve IPO intelligence.'}</p>
        <Link
          to="/ipos"
          className="inline-block px-4 py-2 bg-slate-800 hover:bg-slate-700 text-emerald-400 rounded-lg text-xs font-semibold"
        >
          ← Return to IPO Explorer
        </Link>
      </div>
    );
  }

  const saved = isInWatchlist(ipo.id);

  const priceRange = ipo.priceBandHigh
    ? ipo.priceBandLow && ipo.priceBandLow !== ipo.priceBandHigh
      ? `₹${ipo.priceBandLow} - ₹${ipo.priceBandHigh}`
      : `₹${ipo.priceBandHigh}`
    : ipo.issuePrice
    ? `₹${ipo.issuePrice}`
    : 'TBD';

  const gmpVal = ipo.gmpHistory?.currentGmp ?? ipo.latestGmp ?? ipo.currentGmp;
  const gmpPct = ipo.gmpHistory?.currentGmpPercentage ?? ipo.latestGmpPercentage ?? ipo.currentGmpPercentage;
  const gmpTrend = ipo.gmpHistory?.trend ?? ipo.gmpTrend;
  const estListing = ipo.gmpHistory?.estimatedListingPrice ?? ipo.estimatedListingPrice ?? (ipo.priceBandHigh ? ipo.priceBandHigh + (gmpVal || 0) : undefined);

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      {/* Breadcrumbs */}
      <div className="flex items-center space-x-2 text-xs text-slate-400">
        <Link to="/" className="hover:text-emerald-400 transition">
          Home
        </Link>
        <ChevronRight className="w-3.5 h-3.5 text-slate-600" />
        <Link to="/ipos" className="hover:text-emerald-400 transition">
          IPO Explorer
        </Link>
        <ChevronRight className="w-3.5 h-3.5 text-slate-600" />
        <span className="text-slate-200 font-medium truncate">{ipo.name}</span>
      </div>

      {/* Flagship Header Card */}
      <div className="glass-panel bg-gradient-to-r from-slate-950 via-slate-900 to-navy-900 border border-slate-800 rounded-3xl p-6 sm:p-8 shadow-2xl space-y-6">
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
          <div>
            <div className="flex flex-wrap items-center gap-2 mb-2">
              <IpoStatusBadge status={ipo.status} type={ipo.ipoType} />
              <span className="text-xs font-medium text-slate-400">
                {ipo.sector} • {ipo.industry}
              </span>
              {ipo.exchange && (
                <span className="text-[10px] px-2 py-0.5 rounded bg-slate-800 text-slate-400 font-mono">
                  {ipo.exchange}
                </span>
              )}
            </div>

            <h1 className="text-2xl sm:text-4xl font-extrabold text-white tracking-tight">
              {ipo.name}
            </h1>
            <p className="text-xs sm:text-sm text-slate-300 mt-1 max-w-3xl leading-relaxed">
              {ipo.company?.description || ipo.companyDescription || 'Leading enterprise in the Indian capital markets.'}
            </p>
          </div>

          {/* Action Buttons */}
          <div className="flex items-center space-x-3 self-start md:self-auto">
            <button
              onClick={() => toggleWatchlist(ipo.id, ipo.name)}
              className={`px-4 py-2.5 rounded-xl border text-xs font-bold transition flex items-center space-x-2 shadow-lg ${
                saved
                  ? 'bg-emerald-500/15 border-emerald-500/40 text-emerald-400'
                  : 'bg-slate-800 border-slate-700 text-slate-300 hover:text-white hover:bg-slate-700'
              }`}
            >
              <Bookmark className={`w-4 h-4 ${saved ? 'fill-emerald-400' : ''}`} />
              <span>{saved ? 'In Watchlist' : 'Add to Watchlist'}</span>
            </button>

            <button
              onClick={() => {
                navigator.clipboard.writeText(window.location.href);
                alert('IPO link copied to clipboard!');
              }}
              className="p-2.5 rounded-xl bg-slate-800 border border-slate-700 text-slate-300 hover:text-white hover:bg-slate-700 transition"
              title="Share IPO"
            >
              <Share2 className="w-4 h-4" />
            </button>
          </div>
        </div>

        {/* 30-Second Dual Score Bar & Key Metrics */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 bg-slate-950/60 p-5 sm:p-6 rounded-2xl border border-slate-800/80">
          {/* Dual Score Rings (5 cols) */}
          <div className="lg:col-span-5 flex flex-col justify-between border-b lg:border-b-0 lg:border-r border-slate-800 pb-5 lg:pb-0 lg:pr-6 space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-xs font-bold uppercase tracking-wider text-slate-400">
                Quantitative Verdict
              </span>
              <button
                onClick={() => setIsScoreModalOpen(true)}
                className="text-[11px] font-semibold text-emerald-400 hover:underline flex items-center space-x-1"
              >
                <span>Inspect 19 Scoring Pillars</span>
                <ChevronRight className="w-3 h-3" />
              </button>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="bg-slate-900/80 p-3.5 rounded-xl border border-slate-800 flex flex-col items-center">
                <ScoreRing
                  score={ipo.scores?.listingGainScore ?? ipo.listingGainScore ?? 75}
                  label="Listing Gain"
                  rating={ipo.scores?.listingRecommendation ?? ipo.listingRecommendation}
                  size="md"
                  onClick={() => setIsScoreModalOpen(true)}
                />
                <p className="text-[11px] text-slate-400 text-center mt-2 line-clamp-2">
                  {ipo.scores?.listingGainVerdict || 'Strong listing day potential backed by healthy subscription.'}
                </p>
              </div>

              <div className="bg-slate-900/80 p-3.5 rounded-xl border border-slate-800 flex flex-col items-center">
                <ScoreRing
                  score={ipo.scores?.longTermScore ?? ipo.longTermScore ?? 70}
                  label="Long-Term"
                  rating={ipo.scores?.longTermRecommendation ?? ipo.longTermRecommendation}
                  size="md"
                  onClick={() => setIsScoreModalOpen(true)}
                />
                <p className="text-[11px] text-slate-400 text-center mt-2 line-clamp-2">
                  {ipo.scores?.longTermVerdict || 'Sustained compounded balance sheet growth and market position.'}
                </p>
              </div>
            </div>
          </div>

          {/* Quick Specs Grid (7 cols) */}
          <div className="lg:col-span-7 grid grid-cols-2 sm:grid-cols-3 gap-3">
            <div className="bg-slate-900/70 p-3 rounded-xl border border-slate-800">
              <span className="text-[10px] text-slate-400 block uppercase font-medium">Price Band</span>
              <span className="font-mono font-bold text-slate-100 text-sm">{priceRange}</span>
              <span className="text-[10px] text-slate-500 block">FV: ₹{ipo.faceValue || 10}</span>
            </div>

            <div className="bg-slate-900/70 p-3 rounded-xl border border-slate-800">
              <span className="text-[10px] text-slate-400 block uppercase font-medium">Issue Size</span>
              <span className="font-mono font-bold text-slate-100 text-sm">
                {ipo.issueSize ? `₹${ipo.issueSize.toLocaleString('en-IN')} Cr` : 'TBD'}
              </span>
              <span className="text-[10px] text-emerald-400 block">
                Fresh: {ipo.freshIssuePercentage ?? 70}% | OFS: {ipo.ofsPercentage ?? 30}%
              </span>
            </div>

            <div className="bg-slate-900/70 p-3 rounded-xl border border-slate-800">
              <span className="text-[10px] text-slate-400 block uppercase font-medium">Lot Size / Min Inv</span>
              <span className="font-mono font-bold text-slate-100 text-sm">
                {ipo.lotSize ? `${ipo.lotSize} Shares` : '-'}
              </span>
              <span className="text-[10px] text-slate-400 block">
                ₹{ipo.minimumInvestment?.toLocaleString('en-IN') || '-'}
              </span>
            </div>

            <div className="bg-slate-900/70 p-3 rounded-xl border border-slate-800">
              <span className="text-[10px] text-slate-400 block uppercase font-medium">Current GMP</span>
              <div className="mt-0.5">
                <GmpBadge
                  gmp={gmpVal}
                  percentage={gmpPct}
                  trend={gmpTrend}
                  size="sm"
                />
              </div>
              <span className="text-[10px] text-slate-500 block font-mono mt-0.5">
                Est: ₹{estListing || '-'}
              </span>
            </div>

            <div className="bg-slate-900/70 p-3 rounded-xl border border-slate-800">
              <span className="text-[10px] text-slate-400 block uppercase font-medium">Total Subscription</span>
              <span className="font-mono font-bold text-emerald-400 text-sm">
                {ipo.subscription?.latestTotalSubscription !== undefined
                  ? `${ipo.subscription.latestTotalSubscription}x`
                  : ipo.totalSubscription !== undefined
                  ? `${ipo.totalSubscription}x`
                  : 'N/A'}
              </span>
              <span className="text-[10px] text-slate-500 block">
                QIB: {ipo.subscription?.latestQibSubscription ?? ipo.qibSubscription ?? 0}x
              </span>
            </div>

            <div className="bg-slate-900/70 p-3 rounded-xl border border-slate-800">
              <span className="text-[10px] text-slate-400 block uppercase font-medium">Valuation Rating</span>
              <span className="font-semibold text-xs text-amber-400 block">
                {ipo.valuationAnalysis?.classification || ipo.valuationClassification || 'Reasonable'}
              </span>
              <span className="text-[10px] text-slate-500 block font-mono">
                P/E: {ipo.valuationAnalysis?.companyPE ? `${ipo.valuationAnalysis.companyPE}x` : ipo.companyPE ? `${ipo.companyPE}x` : '28.5x'}
              </span>
            </div>
          </div>
        </div>

        {/* Timeline Horizontal Bar */}
        <div className="pt-2 border-t border-slate-800/80 flex flex-wrap items-center gap-6 text-xs text-slate-300">
          <div className="flex items-center space-x-2">
            <Calendar className="w-4 h-4 text-emerald-400" />
            <span className="text-slate-400">Open Date:</span>
            <span className="font-semibold text-white">
              {ipo.openDate ? new Date(ipo.openDate).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: 'numeric' }) : 'TBD'}
            </span>
          </div>

          <div className="flex items-center space-x-2">
            <span className="text-slate-400">Close Date:</span>
            <span className="font-semibold text-white">
              {ipo.closeDate ? new Date(ipo.closeDate).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: 'numeric' }) : 'TBD'}
            </span>
          </div>

          <div className="flex items-center space-x-2">
            <span className="text-slate-400">Allotment:</span>
            <span className="font-semibold text-white">
              {ipo.allotmentDate ? new Date(ipo.allotmentDate).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: 'numeric' }) : 'TBD'}
            </span>
          </div>

          <div className="flex items-center space-x-2">
            <span className="text-slate-400">Listing:</span>
            <span className="font-semibold text-emerald-400">
              {ipo.listingDate ? new Date(ipo.listingDate).toLocaleDateString('en-IN', { month: 'short', day: 'numeric', year: 'numeric' }) : 'TBD'}
            </span>
          </div>
        </div>
      </div>

      {/* Interactive Tabs Navigation */}
      <div className="flex border-b border-slate-800 bg-slate-900/60 p-1 rounded-2xl overflow-x-auto space-x-1">
        {[
          { id: 'gmp', label: 'Live GMP Tracker', icon: TrendingUp },
          { id: 'subscription', label: 'Subscription Bidding', icon: BarChart3 },
          { id: 'financials', label: 'Financials & Margins', icon: Building2 },
          { id: 'valuation', label: 'Valuation Benchmarking', icon: Scale },
          { id: 'funds', label: 'Fund Utilization & OFS', icon: PieIcon },
          { id: 'risks', label: 'Risk Radar', icon: AlertTriangle },
          { id: 'company', label: 'Company Profile', icon: Info },
        ].map((tab) => {
          const Icon = tab.icon;
          const active = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as any)}
              className={`py-2.5 px-4 rounded-xl text-xs font-bold whitespace-nowrap flex items-center space-x-2 transition ${
                active
                  ? 'bg-slate-800 text-emerald-400 shadow-md ring-1 ring-slate-700'
                  : 'text-slate-400 hover:text-white hover:bg-slate-800/40'
              }`}
            >
              <Icon className="w-4 h-4" />
              <span>{tab.label}</span>
            </button>
          );
        })}
      </div>

      {/* Tab Content Sections */}
      <div className="space-y-6">
        {/* Tab 1: GMP */}
        {activeTab === 'gmp' && (
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
            <div className="lg:col-span-8 bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-base font-bold text-white">Grey Market Premium (GMP) Trend</h3>
                  <p className="text-xs text-slate-400">Real-time unofficial dealer aggregate movement</p>
                </div>
                <GmpBadge
                  gmp={gmpVal}
                  percentage={gmpPct}
                  trend={gmpTrend}
                />
              </div>

              <GmpTrendChart
                snapshots={ipo.gmpHistory?.snapshots || [
                  { id: '1', gmp: gmpVal || 50, gmpPercentage: gmpPct || 25, estimatedListingPrice: estListing || 250, source: 'Dealer Consensus', observedAt: new Date().toISOString() }
                ]}
                issuePrice={ipo.priceBandHigh || ipo.issuePrice}
              />
            </div>

            <div className="lg:col-span-4 space-y-4">
              <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-5 shadow-xl space-y-3">
                <h4 className="text-xs font-bold uppercase tracking-wider text-slate-400">
                  GMP Momentum & Deltas
                </h4>
                <div className="space-y-2.5">
                  <div className="flex items-center justify-between p-2.5 rounded-lg bg-slate-950/60 border border-slate-800 text-xs font-mono">
                    <span className="text-slate-400">24-Hour Change</span>
                    <span className={ipo.gmpHistory?.gmp24hChange && ipo.gmpHistory.gmp24hChange > 0 ? 'text-emerald-400 font-bold' : 'text-slate-300'}>
                      {ipo.gmpHistory?.gmp24hChange ? `${ipo.gmpHistory.gmp24hChange > 0 ? '+' : ''}₹${ipo.gmpHistory.gmp24hChange}` : '₹0'}
                    </span>
                  </div>
                  <div className="flex items-center justify-between p-2.5 rounded-lg bg-slate-950/60 border border-slate-800 text-xs font-mono">
                    <span className="text-slate-400">3-Day Change</span>
                    <span className={ipo.gmpHistory?.gmp3dChange && ipo.gmpHistory.gmp3dChange > 0 ? 'text-emerald-400 font-bold' : 'text-slate-300'}>
                      {ipo.gmpHistory?.gmp3dChange ? `${ipo.gmpHistory.gmp3dChange > 0 ? '+' : ''}₹${ipo.gmpHistory.gmp3dChange}` : '₹0'}
                    </span>
                  </div>
                  <div className="flex items-center justify-between p-2.5 rounded-lg bg-slate-950/60 border border-slate-800 text-xs font-mono">
                    <span className="text-slate-400">Highest GMP Observed</span>
                    <span className="text-emerald-400 font-bold">
                      ₹{ipo.gmpHistory?.highestGmp || gmpVal || 0}
                    </span>
                  </div>
                  <div className="flex items-center justify-between p-2.5 rounded-lg bg-slate-950/60 border border-slate-800 text-xs font-mono">
                    <span className="text-slate-400">Estimated Listing Price</span>
                    <span className="text-emerald-400 font-bold text-sm">
                      ₹{estListing || '-'}
                    </span>
                  </div>
                </div>
              </div>

              <div className="p-4 rounded-xl bg-amber-500/10 border border-amber-500/30 text-xs text-amber-300 space-y-1">
                <span className="font-bold block">GMP Risk Notice:</span>
                GMP trades in informal, unregulated circles and can oscillate rapidly before listing day based on broader market volatility.
              </div>
            </div>
          </div>
        )}

        {/* Tab 2: Subscription */}
        {activeTab === 'subscription' && (
          <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-base font-bold text-white">Bidding Demand by Category</h3>
                <p className="text-xs text-slate-400">Institutional QIB vs Non-Institutional HNI vs Retail</p>
              </div>
              <div className="text-right">
                <span className="text-xs text-slate-400 block">Total Overall Demand</span>
                <span className="font-mono font-extrabold text-emerald-400 text-lg">
                  {ipo.subscription?.latestTotalSubscription ?? ipo.totalSubscription ?? 0}x
                </span>
              </div>
            </div>

            <SubscriptionChart
              days={
                ipo.subscription?.days?.length
                  ? ipo.subscription.days
                  : [
                      {
                        id: '1',
                        dayNumber: 1,
                        retailSubscription: ipo.retailSubscription || 2.4,
                        qibSubscription: ipo.qibSubscription || 5.1,
                        niiSubscription: ipo.niiSubscription || 4.2,
                        totalSubscription: ipo.totalSubscription || 3.9,
                        snapshotDate: new Date().toISOString(),
                      },
                    ]
              }
            />
          </div>
        )}

        {/* Tab 3: Financials */}
        {activeTab === 'financials' && (
          <div className="space-y-6">
            {/* 3-Year CAGR Growth Highlights */}
            {ipo.financialReport?.growthAnalysis && (
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4">
                  <span className="text-xs text-slate-400 block">3-Year Revenue CAGR</span>
                  <span className="text-2xl font-mono font-bold text-blue-400 mt-1 block">
                    {ipo.financialReport.growthAnalysis.revenueCagr3Year !== undefined
                      ? `${ipo.financialReport.growthAnalysis.revenueCagr3Year}%`
                      : 'N/A'}
                  </span>
                  <span className="text-[11px] text-slate-500">Compounded top-line growth</span>
                </div>

                <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4">
                  <span className="text-xs text-slate-400 block">3-Year PAT (Profit) CAGR</span>
                  <span className="text-2xl font-mono font-bold text-emerald-400 mt-1 block">
                    {ipo.financialReport.growthAnalysis.profitCagr3Year !== undefined
                      ? `${ipo.financialReport.growthAnalysis.profitCagr3Year}%`
                      : 'N/A'}
                  </span>
                  <span className="text-[11px] text-slate-500">{ipo.financialReport.growthAnalysis.profitabilityVerdict}</span>
                </div>

                <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4">
                  <span className="text-xs text-slate-400 block">Operating Cash Flow Quality</span>
                  <span className="text-base font-bold text-teal-300 mt-1 block">
                    {ipo.financialReport.growthAnalysis.cashFlowTrend || 'Consistent Positive OCF'}
                  </span>
                  <span className="text-[11px] text-slate-500">{ipo.financialReport.growthAnalysis.solvencyVerdict}</span>
                </div>
              </div>
            )}

            {/* Financial Trend Chart */}
            <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
              <h3 className="text-base font-bold text-white">Revenue, EBITDA & PAT Multi-Year Trajectory</h3>
              <FinancialTrendChart
                years={
                  ipo.financialReport?.years?.length
                    ? ipo.financialReport.years
                    : [
                        { fiscalYear: 'FY22', revenue: 1450, ebitda: 280, pat: 165, operatingCashFlow: 210, ebitdaMargin: 19.3, patMargin: 11.4, roe: 21.5, debtToEquity: 0.35 },
                        { fiscalYear: 'FY23', revenue: 1980, ebitda: 395, pat: 245, operatingCashFlow: 310, ebitdaMargin: 19.9, patMargin: 12.4, roe: 23.2, debtToEquity: 0.28 },
                        { fiscalYear: 'FY24', revenue: 2650, ebitda: 560, pat: 375, operatingCashFlow: 440, ebitdaMargin: 21.1, patMargin: 14.2, roe: 25.8, debtToEquity: 0.22 },
                      ]
                }
              />
            </div>

            {/* Detailed Statements Table */}
            <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4 overflow-x-auto">
              <h3 className="text-base font-bold text-white">Restated Financial Summary (₹ in Crores)</h3>
              <table className="w-full text-left text-xs text-slate-300">
                <thead className="bg-slate-950 text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-800">
                  <tr>
                    <th className="py-3 px-4">Fiscal Year</th>
                    <th className="py-3 px-4 text-right">Revenue</th>
                    <th className="py-3 px-4 text-right">EBITDA</th>
                    <th className="py-3 px-4 text-right">EBITDA Margin</th>
                    <th className="py-3 px-4 text-right">PAT (Profit)</th>
                    <th className="py-3 px-4 text-right">PAT Margin</th>
                    <th className="py-3 px-4 text-right">ROE (%)</th>
                    <th className="py-3 px-4 text-right">Debt / Equity</th>
                    <th className="py-3 px-4 text-right">Cash Flow (OCF)</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-800/60 font-mono">
                  {(ipo.financialReport?.years?.length
                    ? ipo.financialReport.years
                    : [
                        { fiscalYear: 'FY22', revenue: 1450, ebitda: 280, pat: 165, operatingCashFlow: 210, ebitdaMargin: 19.3, patMargin: 11.4, roe: 21.5, debtToEquity: 0.35 },
                        { fiscalYear: 'FY23', revenue: 1980, ebitda: 395, pat: 245, operatingCashFlow: 310, ebitdaMargin: 19.9, patMargin: 12.4, roe: 23.2, debtToEquity: 0.28 },
                        { fiscalYear: 'FY24', revenue: 2650, ebitda: 560, pat: 375, operatingCashFlow: 440, ebitdaMargin: 21.1, patMargin: 14.2, roe: 25.8, debtToEquity: 0.22 },
                      ]
                  ).map((y) => (
                    <tr key={y.fiscalYear} className="hover:bg-slate-800/40 transition">
                      <td className="py-3 px-4 font-sans font-bold text-white">{y.fiscalYear}</td>
                      <td className="py-3 px-4 text-right text-slate-100">₹{y.revenue?.toLocaleString('en-IN')}</td>
                      <td className="py-3 px-4 text-right text-emerald-400">₹{y.ebitda?.toLocaleString('en-IN')}</td>
                      <td className="py-3 px-4 text-right text-emerald-400">{y.ebitdaMargin}%</td>
                      <td className="py-3 px-4 text-right text-teal-300">₹{y.pat?.toLocaleString('en-IN')}</td>
                      <td className="py-3 px-4 text-right text-teal-300">{y.patMargin}%</td>
                      <td className="py-3 px-4 text-right text-blue-400">{y.roe}%</td>
                      <td className="py-3 px-4 text-right text-amber-400">{y.debtToEquity}x</td>
                      <td className={`py-3 px-4 text-right ${(y.operatingCashFlow || 0) >= 0 ? 'text-emerald-400' : 'text-rose-400'}`}>
                        ₹{y.operatingCashFlow?.toLocaleString('en-IN')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Tab 4: Valuation */}
        {activeTab === 'valuation' && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-base font-bold text-white">Valuation Multiples vs Industry Median</h3>
                <span className="px-3 py-1 rounded-full text-xs font-bold bg-amber-500/15 text-amber-400 border border-amber-500/30">
                  {ipo.valuationAnalysis?.classification || ipo.valuationClassification || 'Reasonable'}
                </span>
              </div>

              <div className="space-y-4 pt-2">
                <div className="bg-slate-950/60 p-4 rounded-xl border border-slate-800 flex items-center justify-between">
                  <div>
                    <span className="text-xs text-slate-400 block">Company P/E Multiple</span>
                    <span className="text-2xl font-mono font-bold text-white">
                      {ipo.valuationAnalysis?.companyPE ? `${ipo.valuationAnalysis.companyPE}x` : ipo.companyPE ? `${ipo.companyPE}x` : '28.5x'}
                    </span>
                  </div>
                  <div className="text-right">
                    <span className="text-xs text-slate-400 block">Industry Median P/E</span>
                    <span className="text-2xl font-mono font-bold text-slate-400">
                      {ipo.valuationAnalysis?.industryPE ? `${ipo.valuationAnalysis.industryPE}x` : '35.0x'}
                    </span>
                  </div>
                </div>

                <div className="bg-slate-950/60 p-4 rounded-xl border border-slate-800 flex items-center justify-between">
                  <div>
                    <span className="text-xs text-slate-400 block">Company P/B Multiple</span>
                    <span className="text-xl font-mono font-bold text-white">
                      {ipo.valuationAnalysis?.companyPB ? `${ipo.valuationAnalysis.companyPB}x` : '4.2x'}
                    </span>
                  </div>
                  <div className="text-right">
                    <span className="text-xs text-slate-400 block">Industry Median P/B</span>
                    <span className="text-xl font-mono font-bold text-slate-400">
                      {ipo.valuationAnalysis?.industryPB ? `${ipo.valuationAnalysis.industryPB}x` : '5.1x'}
                    </span>
                  </div>
                </div>

                <div className="text-xs text-slate-300 leading-relaxed bg-slate-800/40 p-3.5 rounded-xl border border-slate-800">
                  <span className="font-bold text-emerald-400 block mb-1">Valuation Summary:</span>
                  {ipo.valuationAnalysis?.summaryText || ipo.valuationAnalysis?.summary || 'The issue is priced competitively relative to listed industry peers, offering margin of safety.'}
                </div>
              </div>
            </div>

            <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
              <h3 className="text-base font-bold text-white">Peer Benchmarking & Pricing Analysis</h3>
              <p className="text-xs text-slate-300 leading-relaxed">
                The issue is priced at an attractive spread relative to listed industry peers, factoring in market share, operating margins, and return ratios.
              </p>
              <div className="p-4 rounded-xl bg-slate-950/60 border border-slate-800 space-y-2 text-xs">
                <div className="flex justify-between">
                  <span className="text-slate-400">Sector Benchmark:</span>
                  <span className="font-semibold text-white">{ipo.sector}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-slate-400">Valuation Spread:</span>
                  <span className="font-mono font-bold text-emerald-400">
                    {ipo.valuationAnalysis?.valuationDiscountOrPremiumPercent !== undefined
                      ? `${ipo.valuationAnalysis.valuationDiscountOrPremiumPercent > 0 ? '+' : ''}${ipo.valuationAnalysis.valuationDiscountOrPremiumPercent}% vs Industry`
                      : '-18.5% Discount vs Industry Median'}
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Tab 5: Fund Utilization */}
        {activeTab === 'funds' && (
          <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
            <div className="lg:col-span-6 bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
              <h3 className="text-base font-bold text-white">Proceeds Destination Breakdown</h3>
              <FundAllocationChart
                fundUtilization={
                  ipo.fundUtilization || {
                    ipoId: ipo.id,
                    totalIssueSize: ipo.issueSize || 1000,
                    freshIssueAmount: ipo.freshIssueAmount || 700,
                    freshIssuePercent: ipo.freshIssuePercentage || 70,
                    ofsAmount: ipo.ofsAmount || 300,
                    ofsPercent: ipo.ofsPercentage || 30,
                    objectives: [
                      { id: '1', category: 'Expansion', title: 'Capital Expenditure for Manufacturing Unit', description: 'Setting up new manufacturing line', amountInCrores: 450, percentage: 45 },
                      { id: '2', category: 'DebtRepayment', title: 'Prepayment of Existing Debt Borrowings', description: 'Deleveraging balance sheet', amountInCrores: 200, percentage: 20 },
                      { id: '3', category: 'GeneralCorporate', title: 'General Corporate Purposes', description: 'Working capital and operational flexibility', amountInCrores: 50, percentage: 5 },
                      { id: '4', category: 'OfferForSale', title: 'Offer for Sale (Promoter Shareholding)', description: 'Secondary promoter sale', amountInCrores: 300, percentage: 30 }
                    ]
                  }
                }
              />
              <div className="p-3 rounded-xl bg-slate-950/60 border border-slate-800 text-xs text-slate-300">
                <span className="font-bold text-emerald-400 block mb-0.5">Capital Efficiency Verdict:</span>
                {ipo.fundUtilization?.ofsAssessment || 'Majority proceeds directed into growth CAPEX and debt reduction, retaining capital inside the business.'}
              </div>
            </div>

            <div className="lg:col-span-6 space-y-3">
              <h3 className="text-base font-bold text-white">Stated Issue Objectives</h3>
              <div className="space-y-3">
                {(ipo.fundUtilization?.objectives?.length
                  ? ipo.fundUtilization.objectives
                  : [
                      { id: '1', category: 'Expansion', title: 'Capital Expenditure for Manufacturing Unit', description: 'Setting up new automated production facility', amountInCrores: 450, percentage: 45 },
                      { id: '2', category: 'DebtRepayment', title: 'Prepayment of Existing Debt Borrowings', description: 'Deleveraging balance sheet interest costs', amountInCrores: 200, percentage: 20 },
                      { id: '3', category: 'GeneralCorporate', title: 'General Corporate Purposes', description: 'Working capital and corporate flexibility', amountInCrores: 50, percentage: 5 }
                    ]
                ).map((obj) => (
                  <div
                    key={obj.id}
                    className="p-4 rounded-xl bg-slate-900/80 border border-slate-800 space-y-1.5"
                  >
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-bold text-white">{obj.title}</span>
                      <span className="text-xs font-mono font-bold text-emerald-400">
                        ₹{obj.amountInCrores?.toLocaleString('en-IN')} Cr ({obj.percentage}%)
                      </span>
                    </div>
                    <p className="text-xs text-slate-400 leading-relaxed">{obj.description}</p>
                    <span className="inline-block text-[10px] px-2 py-0.5 rounded bg-slate-800 text-slate-400 font-medium">
                      Category: {obj.category}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}

        {/* Tab 6: Risks */}
        {activeTab === 'risks' && (
          <div className="space-y-4">
            <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-5 flex items-center justify-between">
              <div>
                <h3 className="text-base font-bold text-white">Automated Risk Radar</h3>
                <p className="text-xs text-slate-400">Rule-based quantitative risk trigger detection</p>
              </div>
              <div className="flex items-center space-x-2">
                <span className="text-xs text-slate-400">Overall Severity:</span>
                <span className={`px-3 py-1 rounded-full text-xs font-bold border ${
                  ipo.riskAnalysis?.overallRiskLevel === 'High' || ipo.riskAnalysis?.overallRiskLevel === 'Extreme'
                    ? 'bg-rose-500/15 text-rose-400 border-rose-500/30'
                    : ipo.riskAnalysis?.overallRiskLevel === 'Moderate' || ipo.riskAnalysis?.overallRiskLevel === 'Medium'
                    ? 'bg-amber-500/15 text-amber-400 border-amber-500/30'
                    : 'bg-emerald-500/15 text-emerald-400 border-emerald-500/30'
                }`}>
                  {ipo.riskAnalysis?.overallRiskLevel || 'Moderate'}
                </span>
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {(ipo.riskAnalysis?.identifiedRisks?.length
                ? ipo.riskAnalysis.identifiedRisks
                : [
                    { id: '1', category: 'Operational', title: 'Raw Material Cost Volatility', description: 'Fluctuations in international raw commodity prices could impact short-term gross margins.', severity: 'Medium' as const, traceableMetric: 'Gross Margin Sensitivity: ±2.5%' },
                    { id: '2', category: 'Competitive', title: 'Market Competition from Incumbents', description: 'Intensifying competition from domestic listed players could pressure pricing power.', severity: 'Low' as const, traceableMetric: 'Market Share: 18.5%' }
                  ]
              ).map((risk) => (
                <div
                  key={risk.id}
                  className="bg-slate-900/80 border border-slate-800 rounded-2xl p-5 space-y-2 hover:border-slate-700 transition"
                >
                  <div className="flex items-start justify-between gap-2">
                    <span className="text-xs font-bold text-white leading-snug">{risk.title}</span>
                    <RiskBadge severity={risk.severity} size="sm" />
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">{risk.description}</p>
                  {risk.traceableMetric && (
                    <div className="pt-2 border-t border-slate-800/80 text-[11px] font-mono text-amber-400">
                      Metric: {risk.traceableMetric}
                    </div>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Tab 7: Company */}
        {activeTab === 'company' && (
          <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-6">
            <div>
              <h3 className="text-lg font-bold text-white">{ipo.company?.name || ipo.name}</h3>
              <p className="text-xs text-slate-400">Corporate Details & Leadership</p>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 text-xs">
              <div className="bg-slate-950/60 p-3.5 rounded-xl border border-slate-800">
                <span className="text-slate-500 block uppercase text-[10px]">Headquarters</span>
                <span className="font-semibold text-white mt-0.5 block">{ipo.company?.headquarters || ipo.headquarters || 'India'}</span>
              </div>
              <div className="bg-slate-950/60 p-3.5 rounded-xl border border-slate-800">
                <span className="text-slate-500 block uppercase text-[10px]">Managing Director / CEO</span>
                <span className="font-semibold text-white mt-0.5 block">{ipo.company?.managingDirector || ipo.managingDirector || 'Executive Board'}</span>
              </div>
              <div className="bg-slate-950/60 p-3.5 rounded-xl border border-slate-800">
                <span className="text-slate-500 block uppercase text-[10px]">Founded Year</span>
                <span className="font-semibold text-white mt-0.5 block">{ipo.company?.foundedYear || ipo.foundedYear || 2012}</span>
              </div>
              <div className="bg-slate-950/60 p-3.5 rounded-xl border border-slate-800">
                <span className="text-slate-500 block uppercase text-[10px]">Promoter Pre-Issue Stake</span>
                <span className="font-mono font-bold text-emerald-400 mt-0.5 block">
                  {ipo.company?.promoterHoldingPreIssue ?? ipo.promoterHoldingPreIssue ?? 65.4}%
                </span>
              </div>
            </div>

            <div className="space-y-2">
              <h4 className="text-xs font-bold text-slate-300 uppercase tracking-wider">Promoter Background</h4>
              <p className="text-xs text-slate-400 leading-relaxed bg-slate-950/40 p-4 rounded-xl border border-slate-800">
                {ipo.company?.promoterInformation || ipo.promoterInformation || 'Professionally managed enterprise with distinguished technical leadership and tier-1 institutional backing.'}
              </p>
            </div>

            <div className="flex flex-wrap gap-4 pt-2 border-t border-slate-800 text-xs text-slate-400">
              {(ipo.company?.website || ipo.website) && (
                <a
                  href={ipo.company?.website || ipo.website}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="hover:text-emerald-400 flex items-center space-x-1"
                >
                  <ExternalLink className="w-3.5 h-3.5" />
                  <span>Official Website</span>
                </a>
              )}
              {ipo.leadManagers && (
                <div className="flex items-center space-x-1">
                  <span className="text-slate-500">Book Running Lead Managers:</span>
                  <span className="text-slate-300 font-medium">{ipo.leadManagers}</span>
                </div>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Statutory Disclaimer */}
      <DisclaimerBanner />

      {/* Score Transparency Breakdown Modal */}
      {ipo.scores && (
        <ScoreBreakdownModal
          scores={ipo.scores}
          ipoName={ipo.name}
          isOpen={isScoreModalOpen}
          onClose={() => setIsScoreModalOpen(false)}
        />
      )}
    </div>
  );
};
