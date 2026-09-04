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

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return 'TBD';
    return new Date(dateStr).toLocaleDateString('en-IN', { month: 'short', day: 'numeric' });
  };

  const datesDisplay = () => {
    if (ipo.status === 'Open') {
      const daysLeft = ipo.closeDate
        ? Math.ceil((new Date(ipo.closeDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24))
        : null;
      return (
        <span className="text-emerald-400 font-semibold">
          {daysLeft !== null && daysLeft >= 0
            ? daysLeft === 0
              ? 'Closes Today'
              : `Closes in ${daysLeft}d (${formatDate(ipo.closeDate)})`
            : `Open (${formatDate(ipo.openDate)} - ${formatDate(ipo.closeDate)})`}
        </span>
      );
    }
    if (ipo.status === 'Upcoming') {
      return (
        <span className="text-slate-300">
          Opens {formatDate(ipo.openDate)} • Closes {formatDate(ipo.closeDate)}
        </span>
      );
    }
    if (ipo.status === 'Closed' || ipo.status === 'AllotmentOut') {
      return (
        <span className="text-amber-400/90">
          Closed {formatDate(ipo.closeDate)} • Listing {formatDate(ipo.listingDate)}
        </span>
      );
    }
    if (ipo.status === 'Listed') {
      return (
        <span className="text-slate-300">
          Listed on {formatDate(ipo.listingDate || ipo.closeDate)}
        </span>
      );
    }
    return <span>Dates: {formatDate(ipo.openDate)} - {formatDate(ipo.closeDate)}</span>;
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
        <div className="bg-slate-950/50 border border-slate-800/80 rounded-xl p-3 my-2.5 grid grid-cols-2 gap-2">
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

        {/* Profit / Loss & Listing Day Intelligence Highlight */}
        {ipo.status === 'Listed' ? (
          <div className="bg-emerald-950/40 border border-emerald-500/30 rounded-xl p-2.5 my-2.5 flex items-center justify-between">
            <div>
              <span className="text-[10px] text-emerald-400 font-semibold uppercase block">Exact Listing Gain</span>
              <span className="font-mono font-bold text-emerald-300 text-xs">
                +{ipo.actualListingGainPercent ?? ipo.listingGainPercent ?? (ipo.priceBandHigh && ipo.listingPrice ? Math.round(((ipo.listingPrice - ipo.priceBandHigh) / ipo.priceBandHigh) * 100) : 0)}%
                {ipo.actualListingGainPerLot ? ` (+₹${ipo.actualListingGainPerLot.toLocaleString('en-IN')}/lot)` : ipo.priceBandHigh && ipo.listingPrice && ipo.lotSize ? ` (+₹${((ipo.listingPrice - ipo.priceBandHigh) * ipo.lotSize).toLocaleString('en-IN')}/lot)` : ''}
              </span>
            </div>
            <div className="text-right">
              <span className="text-[10px] text-slate-400 uppercase block">Listed / Debut Price</span>
              <span className="font-mono font-bold text-white text-xs">
                ₹{ipo.actualListingPrice ?? ipo.listingPrice ?? '-'}
              </span>
            </div>
          </div>
        ) : (
          <div className="bg-slate-950/70 border border-slate-800 rounded-xl p-2.5 my-2.5 flex items-center justify-between">
            <div>
              <span className="text-[10px] text-slate-400 font-semibold uppercase block">Est. Profit/Loss (per Lot)</span>
              <span className={`font-mono font-bold text-xs ${
                (gmpVal || 0) > 0 ? 'text-emerald-400' : (gmpVal || 0) < 0 ? 'text-rose-400' : 'text-slate-300'
              }`}>
                {ipo.estimatedProfitPerLot !== undefined && ipo.estimatedProfitPerLot !== null
                  ? `${ipo.estimatedProfitPerLot >= 0 ? '+' : ''}₹${ipo.estimatedProfitPerLot.toLocaleString('en-IN')}`
                  : gmpVal && ipo.lotSize
                  ? `${gmpVal >= 0 ? '+' : ''}₹${(gmpVal * ipo.lotSize).toLocaleString('en-IN')}`
                  : '₹0 (At Par)'}
                {gmpPct !== undefined && gmpPct !== 0 ? ` (${gmpPct >= 0 ? '+' : ''}${gmpPct}%)` : ''}
              </span>
            </div>
            <div className="text-right">
              <span className="text-[10px] text-slate-400 uppercase block">Est. Listing Price</span>
              <span className="font-mono font-bold text-slate-200 text-xs">
                ₹{ipo.estimatedListingPrice || ((ipo.priceBandHigh || 0) + (gmpVal || 0)) || 'TBD'}
              </span>
            </div>
          </div>
        )}

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
            <span className="font-mono font-bold">
              {ipo.status === 'Upcoming' ? (
                <span className="text-slate-500 font-normal text-[11px]">Bidding Soon</span>
              ) : ipo.totalSubscription !== undefined && ipo.totalSubscription !== null && ipo.totalSubscription > 0 ? (
                <span className="text-emerald-400">{ipo.totalSubscription}x</span>
              ) : (
                <span className="text-slate-500 font-normal text-[11px]">—</span>
              )}
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
