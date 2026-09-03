import React, { useState } from 'react';
import { ScoreBreakdown } from '../../types';
import { X, CheckCircle2, ShieldCheck, Flame, Info } from 'lucide-react';
import { ScoreRing } from './ScoreRing';

interface ScoreBreakdownModalProps {
  scores: ScoreBreakdown;
  ipoName: string;
  isOpen: boolean;
  onClose: () => void;
}

export const ScoreBreakdownModal: React.FC<ScoreBreakdownModalProps> = ({
  scores,
  ipoName,
  isOpen,
  onClose,
}) => {
  const [activeTab, setActiveTab] = useState<'listing' | 'longTerm'>('listing');

  if (!isOpen) return null;

  const listingPillars = scores.listingGainPillars || [];
  const longTermPillars = scores.longTermPillars || [];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-navy-950/80 backdrop-blur-md animate-fadeIn">
      <div className="glass-panel bg-slate-900 border border-slate-700/60 rounded-2xl w-full max-w-3xl max-h-[90vh] flex flex-col shadow-2xl overflow-hidden">
        {/* Modal Header */}
        <div className="px-6 py-5 border-b border-slate-800 flex items-center justify-between bg-slate-950/50">
          <div>
            <div className="flex items-center space-x-2">
              <ShieldCheck className="w-5 h-5 text-emerald-400" />
              <h3 className="text-lg font-bold text-white">Deterministic Scoring Engine Breakdown</h3>
            </div>
            <p className="text-xs text-slate-400 mt-0.5">{ipoName} • 100-Point Explainable Model</p>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg bg-slate-800 text-slate-400 hover:text-white hover:bg-slate-700 transition"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Top Score Summary Banner */}
        <div className="px-6 py-4 bg-slate-950/30 border-b border-slate-800/80 grid grid-cols-1 md:grid-cols-2 gap-4">
          <div
            onClick={() => setActiveTab('listing')}
            className={`p-3 rounded-xl border transition cursor-pointer flex items-center space-x-4 ${
              activeTab === 'listing'
                ? 'bg-slate-800/80 border-emerald-500/50 ring-1 ring-emerald-500/20'
                : 'bg-slate-900/40 border-slate-800 hover:bg-slate-800/40'
            }`}
          >
            <ScoreRing
              score={scores.listingGainScore}
              label=""
              rating={scores.listingRecommendation}
              size="sm"
            />
            <div>
              <div className="flex items-center space-x-1.5 text-xs font-semibold text-emerald-400">
                <Flame className="w-3.5 h-3.5" />
                <span>Listing Gain Score</span>
              </div>
              <p className="text-xs text-slate-300 mt-1 line-clamp-2">{scores.listingGainVerdict}</p>
            </div>
          </div>

          <div
            onClick={() => setActiveTab('longTerm')}
            className={`p-3 rounded-xl border transition cursor-pointer flex items-center space-x-4 ${
              activeTab === 'longTerm'
                ? 'bg-slate-800/80 border-blue-500/50 ring-1 ring-blue-500/20'
                : 'bg-slate-900/40 border-slate-800 hover:bg-slate-800/40'
            }`}
          >
            <ScoreRing
              score={scores.longTermScore}
              label=""
              rating={scores.longTermRecommendation}
              size="sm"
            />
            <div>
              <div className="flex items-center space-x-1.5 text-xs font-semibold text-blue-400">
                <ShieldCheck className="w-3.5 h-3.5" />
                <span>Long-Term Score</span>
              </div>
              <p className="text-xs text-slate-300 mt-1 line-clamp-2">{scores.longTermVerdict}</p>
            </div>
          </div>
        </div>

        {/* Tab Navigation */}
        <div className="flex border-b border-slate-800 bg-slate-900/90 px-6">
          <button
            onClick={() => setActiveTab('listing')}
            className={`py-3 px-4 text-xs font-semibold border-b-2 transition flex items-center space-x-2 ${
              activeTab === 'listing'
                ? 'border-emerald-500 text-emerald-400'
                : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            <span>Listing Gain Model (8 Pillars)</span>
            <span className="text-[10px] px-1.5 py-0.5 rounded bg-slate-800 text-slate-300 font-mono">
              {scores.listingGainScore}/100
            </span>
          </button>
          <button
            onClick={() => setActiveTab('longTerm')}
            className={`py-3 px-4 text-xs font-semibold border-b-2 transition flex items-center space-x-2 ${
              activeTab === 'longTerm'
                ? 'border-blue-500 text-blue-400'
                : 'border-transparent text-slate-400 hover:text-slate-200'
            }`}
          >
            <span>Long-Term Model (11 Pillars)</span>
            <span className="text-[10px] px-1.5 py-0.5 rounded bg-slate-800 text-slate-300 font-mono">
              {scores.longTermScore}/100
            </span>
          </button>
        </div>

        {/* Pillars List Body */}
        <div className="p-6 overflow-y-auto space-y-3 flex-1">
          <div className="flex items-center justify-between text-xs text-slate-400 pb-1">
            <span>PILLAR & REASONING</span>
            <span>POINTS EARNED / MAX</span>
          </div>

          {(activeTab === 'listing' ? listingPillars : longTermPillars).map((pillar: any, idx) => {
            const earned = pillar.score ?? pillar.earnedPoints ?? 0;
            const max = pillar.maxScore ?? pillar.maxPoints ?? 0;
            const reason = pillar.reason ?? pillar.detail ?? '';
            const category = pillar.category || (activeTab === 'listing' ? 'Listing Factor' : 'Fundamental Factor');

            return (
              <div
                key={idx}
                className="p-3.5 rounded-xl bg-slate-800/40 border border-slate-800 flex items-start justify-between space-x-4 hover:border-slate-700 transition"
              >
                <div className="space-y-1 flex-1">
                  <div className="flex items-center space-x-2">
                    <span className="text-xs font-semibold text-slate-200">{pillar.name}</span>
                    <span className="text-[10px] px-2 py-0.5 rounded-full bg-slate-800 text-slate-400 font-medium">
                      {category}
                    </span>
                  </div>
                  <p className="text-xs text-slate-400 leading-relaxed">{reason}</p>
                </div>
                <div className="text-right whitespace-nowrap">
                  <span className={`text-sm font-mono font-bold ${activeTab === 'listing' ? 'text-emerald-400' : 'text-blue-400'}`}>
                    {earned}
                  </span>
                  <span className="text-xs font-mono text-slate-500"> / {max}</span>
                </div>
              </div>
            );
          })}

          {/* Unified Conclusion Callout */}
          {scores.unifiedAnalyticalConclusion && (
            <div className="mt-4 p-4 rounded-xl bg-emerald-950/20 border border-emerald-500/30 flex items-start space-x-3">
              <CheckCircle2 className="w-5 h-5 text-emerald-400 flex-shrink-0 mt-0.5" />
              <div className="text-xs text-slate-300 leading-relaxed">
                <span className="font-semibold text-emerald-300 block mb-0.5">Unified Analytical Conclusion:</span>
                {scores.unifiedAnalyticalConclusion}
              </div>
            </div>
          )}
        </div>

        {/* Modal Footer */}
        <div className="px-6 py-4 border-t border-slate-800 flex items-center justify-between bg-slate-950/70 text-xs text-slate-400">
          <div className="flex items-center space-x-1.5">
            <Info className="w-4 h-4 text-slate-400" />
            <span>Scores recalculated on latest GMP & financial reports</span>
          </div>
          <button
            onClick={onClose}
            className="px-4 py-2 rounded-lg bg-slate-800 hover:bg-slate-700 text-white font-medium transition"
          >
            Close
          </button>
        </div>
      </div>
    </div>
  );
};
