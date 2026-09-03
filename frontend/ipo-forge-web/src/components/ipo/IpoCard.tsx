import React from 'react';
import { Link } from 'react-router-dom';
import { IpoSummary } from '../../types';
import { ScoreRing } from '../common/ScoreRing';
import { GmpBadge } from '../common/GmpBadge';
import { IpoStatusBadge } from '../common/IpoStatusBadge';
import { Bookmark, Calendar, ArrowRight } from 'lucide-react';
import { useWatchlist } from '../../context/WatchlistContext';

interface IpoCardProps {
  ipo: IpoSummary;
  onOpenScoreBreakdown?: (ipo: IpoSummary) => void;
}

export const IpoCard: React.FC<IpoCardProps> = ({ ipo, onOpenScoreBreakdown }) => {
  const { isInWatchlist, toggleWatchlist } = useWatchlist();
  const saved = isInWatchlist(ipo.id);

  const gmpVal = ipo.latestGmp ?? ipo.currentGmp;
  const gmpPct = ipo.latestGmpPercentage ?? ipo.currentGmpPercentage;

  const priceDisplay = ipo.priceBandHigh
    ? ipo.priceBandLow && ipo.priceBandLow !== ipo.priceBandHigh
      ? `₹${ipo.priceBandLow} - ₹${ipo.priceBandHigh}`
      : `₹${ipo.priceBandHigh}`
    : ipo.issuePrice
    ? `₹${ipo.issuePrice}`
    : 'TBD';

  const datesDisplay = () => {
    if (ipo.status === 'Open' && ipo.closeDate) {
      const daysLeft = Math.ceil(
        (new Date(ipo.closeDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24)
      );
      return (
        <span className="text-emerald-400 font-semibold">
          Closes in {daysLeft > 0 ? `${daysLeft}d` : 'Today'}
        </span>
      );
    }
    if (ipo.openDate) {
      return (
        <span>
          {new Date(ipo.openDate).toLocaleDateString('en-IN', { month: 'short', day: 'numeric' })} -{' '}
          {ipo.closeDate
            ? new Date(ipo.closeDate).toLocaleDateString('en-IN', { month: 'short', day: 'numeric' })
            : 'TBD'}
        </span>
      );
    }
    return <span>Dates to be announced</span>;
  };

  return (
    <div className="glass-panel bg-slate-900/90 hover:bg-slate-900 border border-slate-800 hover:border-slate-700/80 rounded-2xl p-5 shadow-xl transition-all duration-300 flex flex-col justify-between group hover:shadow-2xl hover:shadow-emerald-950/20">
      {/* Card Header */}
      <div>
        <div className="flex items-start justify-between gap-2 mb-2.5">
          <div className="flex-1 min-w-0">
            <div className="flex items-center space-x-2">
              <Link
                to={`/ipos/${ipo.id}`}
                className="font-bold text-base text-white hover:text-emerald-400 transition truncate block"
              >
                {ipo.name}
              </Link>
            </div>
            <div className="flex items-center space-x-2 text-xs text-slate-400 mt-0.5">
              <span>{ipo.sector}</span>
              {ipo.symbol && <span>• {ipo.symbol}</span>}
            </div>
          </div>

          <button
            onClick={(e) => {
              e.preventDefault();
              toggleWatchlist(ipo.id, ipo.name);
            }}
            className={`p-2 rounded-lg border transition ${
              saved
                ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400'
                : 'bg-slate-800/40 border-slate-800 text-slate-500 hover:text-slate-300 hover:bg-slate-800'
            }`}
            title={saved ? 'Remove from watchlist' : 'Add to watchlist'}
          >
            <Bookmark className={`w-4 h-4 ${saved ? 'fill-emerald-400' : ''}`} />
          </button>
        </div>

        {/* Status and Type */}
        <div className="flex items-center justify-between mt-1 mb-4">
          <IpoStatusBadge status={ipo.status} type={ipo.ipoType} />
          <GmpBadge
            gmp={gmpVal}
            percentage={gmpPct}
            trend={ipo.gmpTrend}
            size="sm"
          />
        </div>

        {/* Dual Score Rings Section */}
        <div className="bg-slate-950/50 border border-slate-800/80 rounded-xl p-3 my-3 grid grid-cols-2 gap-2">
          <ScoreRing
            score={ipo.listingGainScore}
            label="Listing Gain"
            rating={ipo.listingRecommendation}
            size="sm"
            onClick={onOpenScoreBreakdown ? () => onOpenScoreBreakdown(ipo) : undefined}
          />
          <ScoreRing
            score={ipo.longTermScore}
            label="Long-Term"
            rating={ipo.longTermRecommendation}
            size="sm"
            onClick={onOpenScoreBreakdown ? () => onOpenScoreBreakdown(ipo) : undefined}
          />
        </div>

        {/* Core Deal Specs Grid */}
        <div className="grid grid-cols-2 gap-2.5 text-xs pt-1 pb-2">
          <div className="bg-slate-800/30 p-2 rounded-lg border border-slate-800/60">
            <span className="text-slate-400 text-[10px] block uppercase font-medium">Price Band</span>
            <span className="font-mono font-bold text-slate-100">{priceDisplay}</span>
          </div>
          <div className="bg-slate-800/30 p-2 rounded-lg border border-slate-800/60">
            <span className="text-slate-400 text-[10px] block uppercase font-medium">Issue Size</span>
            <span className="font-mono font-bold text-slate-100">
              {ipo.issueSize ? `₹${ipo.issueSize.toLocaleString('en-IN')} Cr` : 'TBD'}
            </span>
          </div>
          <div className="bg-slate-800/30 p-2 rounded-lg border border-slate-800/60">
            <span className="text-slate-400 text-[10px] block uppercase font-medium">Lot Size / Min Inv</span>
            <span className="font-mono font-bold text-slate-100">
              {ipo.lotSize ? `${ipo.lotSize} shs (₹${ipo.minimumInvestment?.toLocaleString('en-IN') || '-'})` : '-'}
            </span>
          </div>
          <div className="bg-slate-800/30 p-2 rounded-lg border border-slate-800/60">
            <span className="text-slate-400 text-[10px] block uppercase font-medium">Subscription</span>
            <span className="font-mono font-bold text-emerald-400">
              {ipo.totalSubscription !== undefined ? `${ipo.totalSubscription}x` : 'N/A'}
            </span>
          </div>
        </div>
      </div>

      {/* Card Footer */}
      <div className="pt-3 border-t border-slate-800 flex items-center justify-between text-xs text-slate-400 mt-2">
        <div className="flex items-center space-x-1.5 text-[11px]">
          <Calendar className="w-3.5 h-3.5 text-slate-500" />
          {datesDisplay()}
        </div>

        <Link
          to={`/ipos/${ipo.id}`}
          className="inline-flex items-center space-x-1 text-xs font-semibold text-emerald-400 hover:text-emerald-300 transition group-hover:translate-x-0.5"
        >
          <span>30s Analysis</span>
          <ArrowRight className="w-3.5 h-3.5" />
        </Link>
      </div>
    </div>
  );
};
