import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';
import { GmpAccuracyAnalytics, GmpMover, IpoSummary } from '../types';
import { GmpBadge } from '../components/common/GmpBadge';
import { IpoStatusBadge } from '../components/common/IpoStatusBadge';
import { GmpAccuracyScatterChart } from '../components/charts/GmpAccuracyScatterChart';
import { DisclaimerBanner } from '../components/common/DisclaimerBanner';
import { Target, Activity, Flame, ArrowUpRight } from 'lucide-react';

export const GmpTrackerPage: React.FC = () => {
  const [movers, setMovers] = useState<GmpMover[]>([]);
  const [accuracy, setAccuracy] = useState<GmpAccuracyAnalytics | null>(null);
  const [ipos, setIpos] = useState<IpoSummary[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadGmpData() {
      try {
        const [moversData, accuracyData, iposData] = await Promise.all([
          api.getTopGmpGainers(),
          api.getGmpAccuracy(),
          api.getIpos({ pageSize: 50, sortBy: 'gmpPercentage', sortDescending: true }),
        ]);
        setMovers(moversData || []);
        setAccuracy(accuracyData);
        setIpos(iposData.items || []);
      } catch (err) {
        console.error('Failed to load GMP data', err);
      } finally {
        setLoading(false);
      }
    }
    loadGmpData();
  }, []);

  if (loading) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 space-y-6 animate-pulse">
        <div className="h-32 bg-slate-900 rounded-3xl border border-slate-800" />
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div className="h-64 bg-slate-900 rounded-2xl border border-slate-800" />
          <div className="h-64 bg-slate-900 rounded-2xl border border-slate-800" />
          <div className="h-64 bg-slate-900 rounded-2xl border border-slate-800" />
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      {/* Header */}
      <div>
        <div className="inline-flex items-center space-x-2 px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-xs font-semibold mb-2">
          <Activity className="w-3.5 h-3.5" />
          <span>Real-Time Market Sentiment</span>
        </div>
        <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
          Grey Market Premium (GMP) Intelligence & Accuracy Tracker
        </h1>
        <p className="text-xs sm:text-sm text-slate-400 mt-1 max-w-2xl">
          Track unofficial grey market premiums across active IPOs and backtest historical prediction accuracy against final listing day price discoveries.
        </p>
      </div>

      {/* Accuracy Analytics Row */}
      {accuracy && (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
          <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4">
            <span className="text-xs text-slate-400 block">Historical IPOs Evaluated</span>
            <span className="text-2xl font-mono font-bold text-white mt-1 block">
              {accuracy.totalListedIposAnalyzed} Issues
            </span>
            <span className="text-[10px] text-slate-500">Listed track record</span>
          </div>

          <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4">
            <span className="text-xs text-slate-400 block">Within ±10% Accuracy</span>
            <span className="text-2xl font-mono font-bold text-emerald-400 mt-1 block">
              {accuracy.within10PercentAccuracyRate}%
            </span>
            <span className="text-[10px] text-emerald-500">High precision rate</span>
          </div>

          <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4">
            <span className="text-xs text-slate-400 block">Within ±20% Variance</span>
            <span className="text-2xl font-mono font-bold text-teal-300 mt-1 block">
              {accuracy.within20PercentAccuracyRate}%
            </span>
            <span className="text-[10px] text-teal-500">Tight error band</span>
          </div>

          <div className="bg-slate-900/80 border border-slate-800 rounded-2xl p-4">
            <span className="text-xs text-slate-400 block">Avg Prediction Error</span>
            <span className="text-2xl font-mono font-bold text-amber-400 mt-1 block">
              {accuracy.averagePredictionErrorPercent}%
            </span>
            <span className="text-[10px] text-amber-500">Mean absolute error</span>
          </div>
        </div>
      )}

      {/* Section 1: Top GMP Movers */}
      <section className="space-y-4">
        <div className="flex items-center space-x-2">
          <Flame className="w-5 h-5 text-amber-400" />
          <h2 className="text-lg font-bold text-white">Top GMP Gainers of the Week</h2>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {movers.map((mover) => (
            <Link
              key={mover.ipoId}
              to={`/ipos/${mover.ipoId}`}
              className="glass-panel p-4 rounded-2xl border border-slate-800 hover:border-slate-700 bg-slate-900/60 hover:bg-slate-900 transition flex flex-col justify-between group"
            >
              <div>
                <div className="flex items-center justify-between mb-2">
                  <IpoStatusBadge status={mover.status} type={mover.ipoType} />
                </div>
                <h4 className="font-bold text-white text-sm group-hover:text-emerald-400 transition truncate">
                  {mover.ipoName}
                </h4>
              </div>

              <div className="mt-4 pt-3 border-t border-slate-800 flex items-center justify-between">
                <div>
                  <span className="text-[10px] text-slate-500 uppercase block">Current GMP</span>
                  <span className="font-mono font-bold text-emerald-400 text-sm">
                    +₹{mover.currentGmp} (+{mover.currentGmpPercentage}%)
                  </span>
                </div>
                <div className="text-right">
                  <span className="text-[10px] text-slate-500 uppercase block">Shift</span>
                  <span className={`font-mono text-xs font-semibold ${(mover.changeAmount || 0) > 0 ? 'text-emerald-400' : 'text-slate-400'}`}>
                    {(mover.changeAmount || 0) > 0 ? `+₹${mover.changeAmount}` : '₹0'}
                  </span>
                </div>
              </div>
            </Link>
          ))}
        </div>
      </section>

      {/* Section 2: Historical Prediction Accuracy Chart */}
      {accuracy && accuracy.historicalComparison && accuracy.historicalComparison.length > 0 && (
        <section className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-base font-bold text-white">
                Historical GMP Prediction vs Actual Listing Price Discovery
              </h2>
              <p className="text-xs text-slate-400">
                Comparison of last reported pre-listing GMP premium vs actual listing opening gain
              </p>
            </div>
            <Target className="w-5 h-5 text-emerald-400" />
          </div>

          <GmpAccuracyScatterChart items={accuracy.historicalComparison} />

          {/* Historical Accuracy Table */}
          <div className="overflow-x-auto pt-4">
            <table className="w-full text-left text-xs text-slate-300">
              <thead className="bg-slate-950 text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-800 font-mono">
                <tr>
                  <th className="py-2.5 px-3">IPO Name</th>
                  <th className="py-2.5 px-3 text-right">Issue Price</th>
                  <th className="py-2.5 px-3 text-right">Predicted Gain (GMP)</th>
                  <th className="py-2.5 px-3 text-right">Actual Listing Gain</th>
                  <th className="py-2.5 px-3 text-right">Error Variance</th>
                  <th className="py-2.5 px-3 text-center">Direction Match</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/60 font-mono">
                {accuracy.historicalComparison.map((item) => (
                  <tr key={item.ipoId} className="hover:bg-slate-800/40">
                    <td className="py-2.5 px-3 font-sans font-bold text-white">{item.ipoName}</td>
                    <td className="py-2.5 px-3 text-right text-slate-200">₹{item.issuePrice}</td>
                    <td className="py-2.5 px-3 text-right text-blue-400">+{item.gmpPredictedGainPercent || 0}%</td>
                    <td className="py-2.5 px-3 text-right text-emerald-400 font-bold">+{item.actualListingGainPercent}%</td>
                    <td className="py-2.5 px-3 text-right text-amber-400">{item.predictionErrorPercent || 0}%</td>
                    <td className="py-2.5 px-3 text-center">
                      <span className="text-emerald-400 text-sm">✓</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      )}

      {/* Section 3: Full GMP Table */}
      <section className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
        <h2 className="text-base font-bold text-white">Comprehensive Live GMP Master Board</h2>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs text-slate-300">
            <thead className="bg-slate-950 text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-800">
              <tr>
                <th className="py-3 px-4">IPO & Segment</th>
                <th className="py-3 px-4">Status</th>
                <th className="py-3 px-4 text-right">Price Band</th>
                <th className="py-3 px-4">Latest GMP</th>
                <th className="py-3 px-4 text-right">Est. Listing Price</th>
                <th className="py-3 px-4 text-right">Listing Gain Score</th>
                <th className="py-3 px-4 text-right">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/60">
              {ipos.map((ipo) => {
                const gmpVal = ipo.latestGmp ?? ipo.currentGmp;
                const gmpPct = ipo.latestGmpPercentage ?? ipo.currentGmpPercentage;
                const price = ipo.priceBandHigh || ipo.issuePrice || 0;
                const estPrice = ipo.estimatedListingPrice || (price + (gmpVal || 0));

                return (
                  <tr key={ipo.id} className="hover:bg-slate-800/40 transition">
                    <td className="py-3 px-4">
                      <Link to={`/ipos/${ipo.id}`} className="font-bold text-white hover:text-emerald-400">
                        {ipo.name}
                      </Link>
                      <span className="text-[10px] text-slate-500 block">{ipo.sector} • {ipo.ipoType}</span>
                    </td>
                    <td className="py-3 px-4">
                      <IpoStatusBadge status={ipo.status} />
                    </td>
                    <td className="py-3 px-4 text-right font-mono text-slate-200">
                      ₹{price || '-'}
                    </td>
                    <td className="py-3 px-4">
                      <GmpBadge
                        gmp={gmpVal}
                        percentage={gmpPct}
                        trend={ipo.gmpTrend}
                        size="sm"
                      />
                    </td>
                    <td className="py-3 px-4 text-right font-mono font-bold text-slate-100">
                      ₹{estPrice || '-'}
                    </td>
                    <td className="py-3 px-4 text-right font-mono font-bold text-emerald-400">
                      {ipo.listingGainScore}/100
                    </td>
                    <td className="py-3 px-4 text-right">
                      <Link
                        to={`/ipos/${ipo.id}`}
                        className="inline-flex items-center text-xs font-semibold text-emerald-400 hover:text-emerald-300"
                      >
                        <span>Analyze</span>
                        <ArrowUpRight className="w-3.5 h-3.5 ml-0.5" />
                      </Link>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </section>

      <DisclaimerBanner />
    </div>
  );
};
