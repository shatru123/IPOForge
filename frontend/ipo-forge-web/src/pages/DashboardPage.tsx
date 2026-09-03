import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';
import { DashboardSummary, IpoSummary, ScoreBreakdown } from '../types';
import { IpoCard } from '../components/ipo/IpoCard';
import { ScoreBreakdownModal } from '../components/common/ScoreBreakdownModal';
import { DisclaimerBanner } from '../components/common/DisclaimerBanner';
import { Flame, TrendingUp, Calendar, Layers, Activity, Sparkles, ArrowRight, ShieldCheck } from 'lucide-react';

export const DashboardPage: React.FC = () => {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Scoring Breakdown Modal State
  const [selectedIpoForScore, setSelectedIpoForScore] = useState<IpoSummary | null>(null);
  const [activeBreakdown, setActiveBreakdown] = useState<ScoreBreakdown | null>(null);
  const [isScoreModalOpen, setIsScoreModalOpen] = useState(false);

  useEffect(() => {
    async function loadDashboard() {
      try {
        const data = await api.getDashboardSummary();
        setSummary(data);
      } catch (err: any) {
        setError(err.message || 'Failed to load dashboard data.');
      } finally {
        setLoading(false);
      }
    }
    loadDashboard();
  }, []);

  const handleOpenScoreBreakdown = async (ipo: IpoSummary) => {
    setSelectedIpoForScore(ipo);
    try {
      const detail = await api.getIpoById(ipo.id);
      if (detail.scores) {
        setActiveBreakdown(detail.scores);
        setIsScoreModalOpen(true);
      }
    } catch (err) {
      console.error('Failed to load score details', err);
    }
  };

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 space-y-8 animate-pulse">
        <div className="h-44 bg-slate-900 rounded-3xl border border-slate-800" />
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          {[1, 2, 3, 4].map((n) => (
            <div key={n} className="h-24 bg-slate-900 rounded-2xl border border-slate-800" />
          ))}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {[1, 2, 3].map((n) => (
            <div key={n} className="h-80 bg-slate-900 rounded-2xl border border-slate-800" />
          ))}
        </div>
      </div>
    );
  }

  if (error || !summary) {
    return (
      <div className="max-w-7xl mx-auto px-4 py-16 text-center space-y-4">
        <div className="text-rose-400 font-bold text-lg">Unable to load dashboard</div>
        <p className="text-xs text-slate-400">{error}</p>
        <button
          onClick={() => window.location.reload()}
          className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-white rounded-lg text-xs"
        >
          Retry
        </button>
      </div>
    );
  }

  const openIpos = summary.openIpos || [];
  const upcomingIpos = summary.upcomingIpos || [];
  const recentlyListedIpos = summary.recentlyListedIpos || [];
  const totalAnalyzed = summary.totalIposCount || (openIpos.length + upcomingIpos.length + recentlyListedIpos.length) || 7;

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-10">
      {/* Hero Banner */}
      <div className="relative overflow-hidden rounded-3xl bg-gradient-to-r from-slate-950 via-slate-900 to-navy-900 border border-slate-800 p-6 sm:p-10 shadow-2xl">
        <div className="absolute top-0 right-0 -mr-16 -mt-16 w-80 h-80 rounded-full bg-emerald-500/10 blur-3xl pointer-events-none" />
        <div className="relative z-10 max-w-3xl space-y-3">
          <div className="inline-flex items-center space-x-2 px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-xs font-semibold">
            <Sparkles className="w-3.5 h-3.5" />
            <span>Next-Gen Indian Capital Markets Intelligence</span>
          </div>
          <h1 className="text-3xl sm:text-4xl font-extrabold text-white tracking-tight leading-tight">
            Research. Analyze. <span className="text-emerald-400">Decide in 30 Seconds.</span>
          </h1>
          <p className="text-slate-300 text-xs sm:text-sm leading-relaxed max-w-2xl">
            Objective quantitative evaluation for Mainboard and SME IPOs. Powered by explainable 100-point Listing Gain and Long-Term scoring engines, real-time GMP trackers, subscription multipliers, and financial ratios.
          </p>
          <div className="pt-2 flex flex-wrap gap-3">
            <Link
              to="/ipos"
              className="px-5 py-2.5 rounded-xl bg-emerald-500 hover:bg-emerald-400 text-slate-950 font-bold text-xs shadow-lg shadow-emerald-950/60 transition flex items-center space-x-1.5"
            >
              <span>Explore All IPOs</span>
              <ArrowRight className="w-4 h-4" />
            </Link>
            <Link
              to="/gmp"
              className="px-5 py-2.5 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-100 font-semibold text-xs border border-slate-700 transition flex items-center space-x-1.5"
            >
              <TrendingUp className="w-4 h-4 text-emerald-400" />
              <span>Live GMP Tracker</span>
            </Link>
          </div>
        </div>
      </div>

      {/* Key Market Metrics Row */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4 shadow-lg">
          <div className="flex items-center justify-between">
            <span className="text-slate-400 text-xs font-medium">Currently Open</span>
            <Activity className="w-4 h-4 text-emerald-400 animate-pulse" />
          </div>
          <div className="mt-2 flex items-baseline space-x-2">
            <span className="text-2xl font-mono font-bold text-white">
              {summary.activeOpenIposCount ?? openIpos.length}
            </span>
            <span className="text-[11px] text-emerald-400 font-medium">Bidding Active</span>
          </div>
        </div>

        <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4 shadow-lg">
          <div className="flex items-center justify-between">
            <span className="text-slate-400 text-xs font-medium">Upcoming Pipeline</span>
            <Calendar className="w-4 h-4 text-blue-400" />
          </div>
          <div className="mt-2 flex items-baseline space-x-2">
            <span className="text-2xl font-mono font-bold text-white">
              {summary.upcomingIposCount ?? upcomingIpos.length}
            </span>
            <span className="text-[11px] text-blue-400 font-medium">Forthcoming</span>
          </div>
        </div>

        <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4 shadow-lg">
          <div className="flex items-center justify-between">
            <span className="text-slate-400 text-xs font-medium">Avg Market GMP</span>
            <TrendingUp className="w-4 h-4 text-emerald-400" />
          </div>
          <div className="mt-2 flex items-baseline space-x-2">
            <span className="text-2xl font-mono font-bold text-emerald-400">
              +{summary.averageGmpThisMonth}%
            </span>
            <span className="text-[11px] text-slate-400">This Month</span>
          </div>
        </div>

        <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4 shadow-lg">
          <div className="flex items-center justify-between">
            <span className="text-slate-400 text-xs font-medium">Total Issues Analyzed</span>
            <Layers className="w-4 h-4 text-purple-400" />
          </div>
          <div className="mt-2 flex items-baseline space-x-2">
            <span className="text-2xl font-mono font-bold text-white">
              {totalAnalyzed}
            </span>
            <span className="text-[11px] text-purple-400 font-medium">Mainboard & SME</span>
          </div>
        </div>
      </div>

      {/* Top GMP Gainers Horizontal Ticker */}
      {summary.topGmpGainers && summary.topGmpGainers.length > 0 && (
        <div className="bg-slate-900/60 border border-slate-800 rounded-2xl p-4">
          <div className="flex items-center space-x-2 mb-3 text-xs font-bold text-slate-300 uppercase tracking-wider">
            <Flame className="w-4 h-4 text-amber-400" />
            <span>Trending GMP Gainers</span>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-3">
            {summary.topGmpGainers.slice(0, 4).map((mover) => (
              <Link
                key={mover.ipoId}
                to={`/ipos/${mover.ipoId}`}
                className="p-3 bg-slate-950/60 hover:bg-slate-800/60 border border-slate-800/80 rounded-xl flex items-center justify-between transition group"
              >
                <div>
                  <span className="text-xs font-bold text-white group-hover:text-emerald-400 transition block truncate max-w-[130px]">
                    {mover.ipoName}
                  </span>
                  <span className="text-[10px] text-slate-500">{mover.ipoType}</span>
                </div>
                <div className="text-right font-mono">
                  <span className="text-xs font-bold text-emerald-400 block">
                    +₹{mover.currentGmp}
                  </span>
                  <span className="text-[10px] text-emerald-500">+{mover.currentGmpPercentage}%</span>
                </div>
              </Link>
            ))}
          </div>
        </div>
      )}

      {/* Section 1: Currently Open IPOs */}
      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <span className="w-2.5 h-2.5 rounded-full bg-emerald-400 animate-ping" />
            <h2 className="text-xl font-bold text-white tracking-tight">Currently Open for Subscription</h2>
          </div>
          <Link to="/ipos?status=Open" className="text-xs font-semibold text-emerald-400 hover:underline">
            View all open issues →
          </Link>
        </div>

        {openIpos.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {openIpos.map((ipo) => (
              <IpoCard
                key={ipo.id}
                ipo={ipo}
                onOpenScoreBreakdown={handleOpenScoreBreakdown}
              />
            ))}
          </div>
        ) : (
          <div className="p-8 text-center bg-slate-900/40 rounded-2xl border border-slate-800 text-xs text-slate-400">
            No IPOs currently taking bids today. Check upcoming issues below!
          </div>
        )}
      </section>

      {/* Section 2: Upcoming IPOs */}
      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <Calendar className="w-5 h-5 text-blue-400" />
            <h2 className="text-xl font-bold text-white tracking-tight">Upcoming Pipeline</h2>
          </div>
          <Link to="/ipos?status=Upcoming" className="text-xs font-semibold text-blue-400 hover:underline">
            View full pipeline →
          </Link>
        </div>

        {upcomingIpos.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {upcomingIpos.map((ipo) => (
              <IpoCard
                key={ipo.id}
                ipo={ipo}
                onOpenScoreBreakdown={handleOpenScoreBreakdown}
              />
            ))}
          </div>
        ) : (
          <div className="p-8 text-center bg-slate-900/40 rounded-2xl border border-slate-800 text-xs text-slate-400">
            No upcoming IPO announcements at this moment.
          </div>
        )}
      </section>

      {/* Section 3: Recently Listed Issues */}
      <section className="space-y-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <ShieldCheck className="w-5 h-5 text-purple-400" />
            <h2 className="text-xl font-bold text-white tracking-tight">Recently Listed Historical Issues</h2>
          </div>
          <Link to="/ipos?status=Listed" className="text-xs font-semibold text-purple-400 hover:underline">
            View all listed IPOs →
          </Link>
        </div>

        {recentlyListedIpos.length > 0 && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {recentlyListedIpos.map((ipo) => {
              const listGain = ipo.actualListingGainPercent ?? ipo.listingGainPercent;
              const listPrice = ipo.actualListingPrice ?? ipo.listingPrice;
              return (
                <Link
                  key={ipo.id}
                  to={`/ipos/${ipo.id}`}
                  className="glass-panel p-4 rounded-2xl border border-slate-800 hover:border-slate-700 bg-slate-900/60 hover:bg-slate-900 transition flex flex-col justify-between group"
                >
                  <div>
                    <div className="flex items-center justify-between text-[11px] text-slate-400 mb-1.5">
                      <span>{ipo.ipoType}</span>
                      <span>{ipo.listingDate ? new Date(ipo.listingDate).toLocaleDateString('en-IN') : '-'}</span>
                    </div>
                    <h4 className="font-bold text-white text-sm group-hover:text-emerald-400 transition truncate">
                      {ipo.name}
                    </h4>
                    <p className="text-xs text-slate-400">{ipo.sector}</p>
                  </div>

                  <div className="mt-4 pt-3 border-t border-slate-800 flex items-center justify-between">
                    <div>
                      <span className="text-[10px] text-slate-500 uppercase block">Listing Gain</span>
                      <span className="font-mono font-bold text-emerald-400 text-sm">
                        {listGain !== undefined ? `+${listGain}%` : '-'}
                      </span>
                    </div>
                    <div className="text-right">
                      <span className="text-[10px] text-slate-500 uppercase block">Listed Price</span>
                      <span className="font-mono font-bold text-slate-100 text-sm">
                        {listPrice ? `₹${listPrice}` : '-'}
                      </span>
                    </div>
                  </div>
                </Link>
              );
            })}
          </div>
        )}
      </section>

      {/* Statutory Disclaimer */}
      <DisclaimerBanner />

      {/* Score Transparency Modal */}
      {selectedIpoForScore && activeBreakdown && (
        <ScoreBreakdownModal
          scores={activeBreakdown}
          ipoName={selectedIpoForScore.name}
          isOpen={isScoreModalOpen}
          onClose={() => {
            setIsScoreModalOpen(false);
            setSelectedIpoForScore(null);
            setActiveBreakdown(null);
          }}
        />
      )}
    </div>
  );
};
