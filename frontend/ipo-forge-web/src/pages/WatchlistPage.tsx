import React from 'react';
import { Link } from 'react-router-dom';
import { useWatchlist } from '../context/WatchlistContext';
import { useAuth } from '../context/AuthContext';
import { Bookmark, Trash2, ArrowRight, Layers, Bell } from 'lucide-react';
import { IpoStatusBadge } from '../components/common/IpoStatusBadge';
import { DisclaimerBanner } from '../components/common/DisclaimerBanner';

export const WatchlistPage: React.FC = () => {
  const { watchlist, toggleWatchlist, loading } = useWatchlist();
  const { isAuthenticated } = useAuth();

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center space-x-2 text-xs text-emerald-400 font-semibold mb-1">
            <Bookmark className="w-4 h-4 fill-emerald-400" />
            <span>Curated Portfolio Pipeline</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
            Investor Watchlist
          </h1>
          <p className="text-xs sm:text-sm text-slate-400 mt-1">
            {isAuthenticated
              ? 'Synced to your cloud profile across devices.'
              : 'Saved locally in your browser. Sign in to sync across devices!'}
          </p>
        </div>

        <Link
          to="/ipos"
          className="px-4 py-2 bg-emerald-500 hover:bg-emerald-400 text-slate-950 font-bold rounded-xl text-xs flex items-center space-x-1.5 transition self-start sm:self-auto shadow-lg shadow-emerald-950/50"
        >
          <span>Discover More IPOs</span>
          <ArrowRight className="w-4 h-4" />
        </Link>
      </div>

      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-pulse">
          {[1, 2, 3].map((n) => (
            <div key={n} className="h-48 bg-slate-900 rounded-2xl border border-slate-800" />
          ))}
        </div>
      ) : watchlist.length === 0 ? (
        <div className="p-16 text-center bg-slate-900/40 rounded-3xl border border-slate-800 space-y-4 max-w-lg mx-auto">
          <div className="w-12 h-12 rounded-2xl bg-slate-800 flex items-center justify-center mx-auto text-slate-400">
            <Bookmark className="w-6 h-6" />
          </div>
          <h3 className="text-base font-bold text-white">Your Watchlist is Empty</h3>
          <p className="text-xs text-slate-400 leading-relaxed">
            Track upcoming and active IPOs, set price targets, and monitor GMP movements with one-click saving.
          </p>
          <Link
            to="/ipos"
            className="inline-block px-5 py-2.5 bg-emerald-500 hover:bg-emerald-400 text-slate-950 font-bold rounded-xl text-xs transition"
          >
            Explore Active IPOs
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {watchlist.map((item) => (
            <div
              key={item.id}
              className="glass-panel p-5 rounded-2xl border border-slate-800 hover:border-slate-700 bg-slate-900/80 hover:bg-slate-900 transition flex flex-col justify-between shadow-xl group"
            >
              <div>
                <div className="flex items-start justify-between gap-2 mb-2">
                  <div className="flex-1 min-w-0">
                    <Link
                      to={`/ipos/${item.ipoId}`}
                      className="font-bold text-base text-white hover:text-emerald-400 transition block truncate"
                    >
                      {item.ipoName}
                    </Link>
                    <span className="text-xs text-slate-400">{item.sector}</span>
                  </div>

                  <button
                    onClick={() => toggleWatchlist(item.ipoId)}
                    className="p-1.5 rounded-lg text-slate-500 hover:text-rose-400 hover:bg-rose-500/10 transition"
                    title="Remove from watchlist"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>

                <div className="flex items-center space-x-2 my-3">
                  <IpoStatusBadge status={item.status} type={item.ipoType} />
                </div>

                {/* Score Pill */}
                <div className="bg-slate-950/60 p-3 rounded-xl border border-slate-800 flex items-center justify-between text-xs font-mono">
                  <div>
                    <span className="text-slate-500 text-[10px] block font-sans">Listing Gain Score</span>
                    <span className="text-emerald-400 font-bold text-sm">
                      {item.listingGainScore}/100
                    </span>
                  </div>
                  <div className="text-right">
                    <span className="text-slate-500 text-[10px] block font-sans">Current GMP</span>
                    <span className="text-emerald-400 font-bold text-sm">
                      {item.currentGmp ? `+₹${item.currentGmp} (${item.currentGmpPercentage}%)` : '₹0 (0%)'}
                    </span>
                  </div>
                </div>
              </div>

              <div className="mt-4 pt-3 border-t border-slate-800 flex items-center justify-between text-xs">
                <span className="text-[11px] text-slate-500">
                  Added {new Date(item.addedAt).toLocaleDateString('en-IN')}
                </span>

                <Link
                  to={`/ipos/${item.ipoId}`}
                  className="text-emerald-400 font-semibold hover:text-emerald-300 flex items-center space-x-1"
                >
                  <span>Analyze</span>
                  <ArrowRight className="w-3.5 h-3.5" />
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}

      <DisclaimerBanner />
    </div>
  );
};
