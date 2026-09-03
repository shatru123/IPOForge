import React, { useState } from 'react';
import { IpoSummary } from '../../types';
import {
  evaluateComparison,
  generateComparisonImage,
  shareToWhatsApp,
  generateWhatsAppShareText,
  exportComparisonCsv,
} from '../../utils/imageExporter';
import {
  X,
  Share2,
  Download,
  Copy,
  FileSpreadsheet,
  Check,
  Trophy,
  Sparkles,
  Calendar,
  Layers,
  ArrowRight,
} from 'lucide-react';
import { ScoreRing } from '../common/ScoreRing';
import { GmpBadge } from '../common/GmpBadge';
import { IpoStatusBadge } from '../common/IpoStatusBadge';
import { Link } from 'react-router-dom';

interface IpoComparisonModalProps {
  ipos: IpoSummary[];
  allAvailableIpos?: IpoSummary[];
  isOpen: boolean;
  onClose: () => void;
  onToggleIpoSelection?: (ipo: IpoSummary) => void;
}

export const IpoComparisonModal: React.FC<IpoComparisonModalProps> = ({
  ipos,
  allAvailableIpos = [],
  isOpen,
  onClose,
  onToggleIpoSelection,
}) => {
  const [copied, setCopied] = useState(false);
  const [downloadingImage, setDownloadingImage] = useState(false);

  if (!isOpen || ipos.length === 0) return null;

  const decision = evaluateComparison(ipos);

  const handleCopyText = async () => {
    const text = generateWhatsAppShareText(ipos);
    try {
      await navigator.clipboard.writeText(text);
      setCopied(true);
      setTimeout(() => setCopied(false), 2500);
    } catch (err) {
      console.error('Failed to copy', err);
    }
  };

  const handleDownloadImage = async () => {
    try {
      setDownloadingImage(true);
      const dataUrl = await generateComparisonImage(ipos);
      const link = document.createElement('a');
      link.href = dataUrl;
      link.download = `IPOForge_Comparison_${new Date().toISOString().split('T')[0]}.png`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } catch (err) {
      console.error('Failed to generate image', err);
    } finally {
      setDownloadingImage(false);
    }
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return 'TBD';
    return new Date(dateStr).toLocaleDateString('en-IN', { month: 'short', day: 'numeric' });
  };

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto bg-slate-950/80 backdrop-blur-md flex items-center justify-center p-3 sm:p-6 animate-in fade-in duration-200">
      <div className="relative w-full max-w-6xl bg-slate-900 border border-slate-800 rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[92vh]">
        {/* Modal Header */}
        <div className="p-6 border-b border-slate-800 bg-slate-950/60 flex flex-wrap items-center justify-between gap-4">
          <div>
            <div className="flex items-center space-x-2">
              <span className="p-1.5 rounded-lg bg-emerald-500/10 text-emerald-400 border border-emerald-500/30">
                <Sparkles className="w-4 h-4" />
              </span>
              <h2 className="text-xl sm:text-2xl font-extrabold text-white tracking-tight">
                IPO Comparison & Which to Apply Matrix
              </h2>
            </div>
            <p className="text-xs text-slate-400 mt-1">
              Deterministic 19-pillar evaluation comparing live GMP, fundamentals, valuations, and listing margin of safety.
            </p>
          </div>

          <div className="flex items-center space-x-2">
            {/* WhatsApp Share */}
            <button
              onClick={() => shareToWhatsApp(ipos)}
              className="px-3.5 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-xs shadow-lg shadow-emerald-950/50 transition flex items-center space-x-1.5"
              title="Share comparison directly to WhatsApp"
            >
              <Share2 className="w-4 h-4" />
              <span>Share on WhatsApp</span>
            </button>

            {/* Download Infographic PNG */}
            <button
              onClick={handleDownloadImage}
              disabled={downloadingImage}
              className="px-3.5 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 border border-slate-700 text-white font-semibold text-xs transition flex items-center space-x-1.5"
              title="Download high-resolution infographic card"
            >
              <Download className="w-4 h-4 text-emerald-400" />
              <span>{downloadingImage ? 'Generating...' : 'Download Image'}</span>
            </button>

            {/* Copy Text Summary */}
            <button
              onClick={handleCopyText}
              className="px-3 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-300 font-semibold text-xs transition flex items-center space-x-1"
              title="Copy formatted summary to clipboard"
            >
              {copied ? <Check className="w-4 h-4 text-emerald-400" /> : <Copy className="w-4 h-4" />}
              <span>{copied ? 'Copied!' : 'Copy'}</span>
            </button>

            {/* Export CSV */}
            <button
              onClick={() => exportComparisonCsv(ipos)}
              className="p-2 rounded-xl bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-300 transition"
              title="Download Excel / CSV data sheet"
            >
              <FileSpreadsheet className="w-4 h-4" />
            </button>

            {/* Close Button */}
            <button
              onClick={onClose}
              className="p-2 rounded-xl bg-slate-800 hover:bg-rose-500/20 hover:text-rose-400 border border-slate-700 text-slate-400 transition"
            >
              <X className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Modal Scrollable Body */}
        <div className="p-6 overflow-y-auto space-y-6">
          {/* Winner AI Decision Banner */}
          {decision.winnerName && (
            <div className="bg-gradient-to-r from-emerald-950/60 via-slate-900 to-navy-950/60 border border-emerald-500/40 rounded-2xl p-5 shadow-lg flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
              <div className="flex items-start space-x-3.5">
                <div className="p-3 rounded-2xl bg-amber-500/20 text-amber-400 border border-amber-500/30">
                  <Trophy className="w-6 h-6" />
                </div>
                <div>
                  <div className="flex items-center space-x-2">
                    <span className="px-2.5 py-0.5 rounded-full bg-emerald-500/20 border border-emerald-500/40 text-emerald-400 text-[11px] font-bold uppercase tracking-wider">
                      Recommended #1 Choice to Apply
                    </span>
                  </div>
                  <h3 className="text-xl font-bold text-white mt-1">
                    {decision.winnerName}
                  </h3>
                  <p className="text-xs text-slate-300 mt-0.5 max-w-2xl">
                    {decision.reason}
                  </p>
                </div>
              </div>

              <div className="flex items-center space-x-2 self-end sm:self-center">
                <Link
                  to={`/ipos/${decision.winnerId}`}
                  className="px-4 py-2 rounded-xl bg-emerald-500 hover:bg-emerald-400 text-slate-950 font-bold text-xs transition flex items-center space-x-1"
                >
                  <span>Explore 30s Breakdown</span>
                  <ArrowRight className="w-3.5 h-3.5" />
                </Link>
              </div>
            </div>
          )}

          {/* Side-by-Side Comparison Grid */}
          <div className={`grid grid-cols-1 md:grid-cols-${Math.min(ipos.length, 4)} gap-4`}>
            {decision.rankings.map((ranked) => {
              const ipo = ranked.ipo;
              const isWinner = ranked.rank === 1;

              return (
                <div
                  key={ipo.id}
                  className={`rounded-2xl p-5 flex flex-col justify-between border transition-all ${
                    isWinner
                      ? 'bg-slate-900/90 border-emerald-500/60 shadow-xl shadow-emerald-950/20'
                      : 'bg-slate-900/50 border-slate-800'
                  }`}
                >
                  <div>
                    {/* Rank Badge */}
                    <div className="flex items-center justify-between mb-3">
                      <span
                        className="px-2.5 py-1 rounded-lg text-xs font-bold font-mono"
                        style={{ backgroundColor: `${ranked.color}20`, color: ranked.color, border: `1px solid ${ranked.color}40` }}
                      >
                        {ranked.badge}
                      </span>
                      <IpoStatusBadge status={ipo.status} type={ipo.ipoType} />
                    </div>

                    {/* Name */}
                    <Link
                      to={`/ipos/${ipo.id}`}
                      className="text-base font-bold text-white hover:text-emerald-400 transition block truncate"
                    >
                      {ipo.name}
                    </Link>
                    <span className="text-xs text-slate-400 block mt-0.5">{ipo.sector}</span>

                    {/* Dual Score Rings */}
                    <div className="bg-slate-950/60 border border-slate-800/80 rounded-xl p-3 my-3 grid grid-cols-2 gap-2">
                      <ScoreRing
                        score={ipo.listingGainScore}
                        label="Listing Gain"
                        rating={ipo.listingRecommendation}
                        size="sm"
                      />
                      <ScoreRing
                        score={ipo.longTermScore}
                        label="Long-Term"
                        rating={ipo.longTermRecommendation}
                        size="sm"
                      />
                    </div>

                    {/* Specs Table */}
                    <div className="space-y-2 text-xs divide-y divide-slate-800/60 pt-1">
                      <div className="flex justify-between py-1">
                        <span className="text-slate-400">Live GMP</span>
                        <GmpBadge
                          gmp={ipo.latestGmp}
                          percentage={ipo.latestGmpPercentage}
                          trend={ipo.gmpTrend}
                          size="sm"
                        />
                      </div>
                      <div className="flex justify-between py-1">
                        <span className="text-slate-400">Est. Listing Price</span>
                        <span className="font-mono font-bold text-white">
                          ₹{ipo.estimatedListingPrice || ipo.priceBandHigh || '-'}
                        </span>
                      </div>
                      <div className="flex justify-between py-1">
                        <span className="text-slate-400">Price Band</span>
                        <span className="font-mono font-bold text-slate-200">
                          ₹{ipo.priceBandLow || 0} - ₹{ipo.priceBandHigh || 0}
                        </span>
                      </div>
                      <div className="flex justify-between py-1">
                        <span className="text-slate-400">Lot Size / Min Inv</span>
                        <span className="font-mono text-slate-200">
                          {ipo.lotSize} shs (₹{(ipo.minimumInvestment || 0).toLocaleString('en-IN')})
                        </span>
                      </div>
                      <div className="flex justify-between py-1">
                        <span className="text-slate-400">Issue Size</span>
                        <span className="font-mono text-slate-200">
                          {ipo.issueSize ? `₹${ipo.issueSize.toLocaleString('en-IN')} Cr` : 'TBD'}
                        </span>
                      </div>
                      <div className="flex justify-between py-1">
                        <span className="text-slate-400">Subscription</span>
                        <span className="font-mono font-bold text-emerald-400">
                          {ipo.status === 'Upcoming' ? 'Bidding Soon' : ipo.totalSubscription ? `${ipo.totalSubscription}x` : '—'}
                        </span>
                      </div>
                      <div className="flex justify-between py-1">
                        <span className="text-slate-400">Bidding Dates</span>
                        <span className="font-mono text-slate-300">
                          {formatDate(ipo.openDate)} – {formatDate(ipo.closeDate)}
                        </span>
                      </div>
                      <div className="flex justify-between py-1">
                        <span className="text-slate-400">Listing Date</span>
                        <span className="font-mono text-slate-300">
                          {formatDate(ipo.listingDate)}
                        </span>
                      </div>
                    </div>
                  </div>

                  {/* Verdict Footer */}
                  <div className="mt-4 pt-3 border-t border-slate-800">
                    <div
                      className="p-2.5 rounded-xl text-[11px] leading-relaxed"
                      style={{ backgroundColor: `${ranked.color}15`, border: `1px solid ${ranked.color}30`, color: '#E2E8F0' }}
                    >
                      <span className="font-bold block" style={{ color: ranked.color }}>
                        Decision Verdict:
                      </span>
                      {ranked.verdict}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>

          {/* Optional: Add More IPOs to Compare Ticker */}
          {allAvailableIpos.length > ipos.length && onToggleIpoSelection && (
            <div className="pt-4 border-t border-slate-800">
              <span className="text-xs font-bold text-slate-400 uppercase tracking-wider block mb-2.5">
                Add More Active IPOs to Compare:
              </span>
              <div className="flex flex-wrap gap-2">
                {allAvailableIpos
                  .filter((a) => !ipos.some((i) => i.id === a.id))
                  .slice(0, 8)
                  .map((cand) => (
                    <button
                      key={cand.id}
                      onClick={() => onToggleIpoSelection(cand)}
                      className="px-3 py-1.5 rounded-xl bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-300 text-xs font-medium transition flex items-center space-x-1.5"
                    >
                      <span>+ {cand.name}</span>
                      <span className="text-[10px] text-emerald-400">
                        (+{(cand.latestGmpPercentage || 0).toFixed(0)}% GMP)
                      </span>
                    </button>
                  ))}
              </div>
            </div>
          )}
        </div>

        {/* Modal Footer */}
        <div className="p-4 border-t border-slate-800 bg-slate-950/60 flex items-center justify-between text-xs text-slate-500">
          <span>
            Created by Shatrughna Ambhore • ambhoreshatrughna@gmail.com • +91 9604466334
          </span>
          <span className="text-[11px]">
            Data synced from Indian Market Public Aggregators
          </span>
        </div>
      </div>
    </div>
  );
};
