import React from 'react';
import { Link } from 'react-router-dom';
import { IpoSummary } from '../../types';
import { GmpBadge } from '../common/GmpBadge';
import { IpoStatusBadge } from '../common/IpoStatusBadge';
import { Bookmark, ChevronRight } from 'lucide-react';
import { useWatchlist } from '../../context/WatchlistContext';

interface IpoTableProps {
  ipos: IpoSummary[];
  onOpenScoreBreakdown?: (ipo: IpoSummary) => void;
}

export const IpoTable: React.FC<IpoTableProps> = ({ ipos, onOpenScoreBreakdown }) => {
  const { isInWatchlist, toggleWatchlist } = useWatchlist();

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('en-IN', { month: 'short', day: 'numeric' });
  };

  return (
    <div className="overflow-x-auto rounded-2xl border border-slate-800 bg-slate-900/60 shadow-xl">
      <table className="w-full text-left text-xs text-slate-300">
        <thead className="bg-slate-950/80 text-[11px] uppercase tracking-wider text-slate-400 font-semibold border-b border-slate-800">
          <tr>
            <th className="py-3.5 px-4">IPO & Company</th>
            <th className="py-3.5 px-4">Status & Type</th>
            <th className="py-3.5 px-4 text-center">Scores (Gain / LT)</th>
            <th className="py-3.5 px-4">Issue Dates</th>
            <th className="py-3.5 px-4">Price Band</th>
            <th className="py-3.5 px-4">Issue Size</th>
            <th className="py-3.5 px-4">Current GMP</th>
            <th className="py-3.5 px-4">Subscription</th>
            <th className="py-3.5 px-4 text-right">Actions</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-800/60">
          {ipos.map((ipo) => {
            const saved = isInWatchlist(ipo.id);
            const gmpVal = ipo.latestGmp ?? ipo.currentGmp;
            const gmpPct = ipo.latestGmpPercentage ?? ipo.currentGmpPercentage;

            const price = ipo.priceBandHigh
              ? ipo.priceBandLow && ipo.priceBandLow !== ipo.priceBandHigh
                ? `₹${ipo.priceBandLow}-${ipo.priceBandHigh}`
                : `₹${ipo.priceBandHigh}`
              : ipo.issuePrice
              ? `₹${ipo.issuePrice}`
              : '-';

            return (
              <tr key={ipo.id} className="hover:bg-slate-800/40 transition">
                {/* Name & Sector */}
                <td className="py-3.5 px-4">
                  <div>
                    <Link
                      to={`/ipos/${ipo.id}`}
                      className="font-bold text-white hover:text-emerald-400 transition"
                    >
                      {ipo.name}
                    </Link>
                    <div className="text-[11px] text-slate-400">
                      {ipo.sector} {ipo.symbol && `• ${ipo.symbol}`}
                    </div>
                  </div>
                </td>

                {/* Status */}
                <td className="py-3.5 px-4">
                  <IpoStatusBadge status={ipo.status} type={ipo.ipoType} />
                </td>

                {/* Scores */}
                <td className="py-3.5 px-4">
                  <div className="flex items-center justify-center space-x-2">
                    <button
                      onClick={() => onOpenScoreBreakdown && onOpenScoreBreakdown(ipo)}
                      className={`px-2 py-1 rounded font-mono font-bold text-xs border transition ${
                        (ipo.listingGainScore || 0) >= 75
                          ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400 hover:border-emerald-400'
                          : (ipo.listingGainScore || 0) >= 50
                          ? 'bg-amber-500/10 border-amber-500/30 text-amber-400 hover:border-amber-400'
                          : 'bg-rose-500/10 border-rose-500/30 text-rose-400 hover:border-rose-400'
                      }`}
                      title="Listing Gain Score"
                    >
                      {ipo.listingGainScore ?? '-'}
                    </button>
                    <span className="text-slate-600">/</span>
                    <button
                      onClick={() => onOpenScoreBreakdown && onOpenScoreBreakdown(ipo)}
                      className={`px-2 py-1 rounded font-mono font-bold text-xs border transition ${
                        (ipo.longTermScore || 0) >= 75
                          ? 'bg-blue-500/10 border-blue-500/30 text-blue-400 hover:border-blue-400'
                          : (ipo.longTermScore || 0) >= 50
                          ? 'bg-slate-800 border-slate-700 text-slate-300 hover:border-slate-500'
                          : 'bg-rose-500/10 border-rose-500/30 text-rose-400 hover:border-rose-400'
                      }`}
                      title="Long-Term Score"
                    >
                      {ipo.longTermScore ?? '-'}
                    </button>
                  </div>
                </td>

                {/* Dates */}
                <td className="py-3.5 px-4 text-[11px] text-slate-300 font-mono">
                  {ipo.status === 'Listed' ? (
                    <span className="text-slate-400">Listed: {formatDate(ipo.listingDate || ipo.closeDate)}</span>
                  ) : ipo.status === 'Closed' ? (
                    <span className="text-amber-400/80">Closed: {formatDate(ipo.closeDate)}</span>
                  ) : (
                    <span>{formatDate(ipo.openDate)} - {formatDate(ipo.closeDate)}</span>
                  )}
                </td>

                {/* Price */}
                <td className="py-3.5 px-4 font-mono text-slate-200">{price}</td>

                {/* Issue Size */}
                <td className="py-3.5 px-4 font-mono text-slate-200">
                  {ipo.issueSize ? `₹${ipo.issueSize.toLocaleString('en-IN')} Cr` : '-'}
                </td>

                {/* GMP */}
                <td className="py-3.5 px-4">
                  <GmpBadge
                    gmp={gmpVal}
                    percentage={gmpPct}
                    trend={ipo.gmpTrend}
                    size="sm"
                  />
                </td>

                {/* Subscription */}
                <td className="py-3.5 px-4 font-mono font-semibold">
                  {ipo.status === 'Upcoming' ? (
                    <span className="text-slate-500 text-[11px] font-normal">Soon</span>
                  ) : ipo.totalSubscription !== undefined && ipo.totalSubscription !== null && ipo.totalSubscription > 0 ? (
                    <span className="text-emerald-400">{ipo.totalSubscription}x</span>
                  ) : (
                    <span className="text-slate-500 text-[11px] font-normal">—</span>
                  )}
                </td>

                {/* Actions */}
                <td className="py-3.5 px-4 text-right">
                  <div className="flex items-center justify-end space-x-2">
                    <button
                      onClick={() => toggleWatchlist(ipo.id, ipo.name)}
                      className={`p-1.5 rounded-lg border transition ${
                        saved
                          ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400'
                          : 'bg-slate-800 border-slate-700 text-slate-400 hover:text-white'
                      }`}
                    >
                      <Bookmark className={`w-3.5 h-3.5 ${saved ? 'fill-emerald-400' : ''}`} />
                    </button>

                    <Link
                      to={`/ipos/${ipo.id}`}
                      className="p-1.5 rounded-lg bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 hover:bg-emerald-500 hover:text-slate-950 font-medium transition"
                    >
                      <ChevronRight className="w-3.5 h-3.5" />
                    </Link>
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};
