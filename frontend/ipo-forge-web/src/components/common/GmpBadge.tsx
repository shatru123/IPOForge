import React from 'react';
import { GmpTrend } from '../../types';
import { TrendingUp, TrendingDown, Minus } from 'lucide-react';

interface GmpBadgeProps {
  gmp?: number;
  percentage?: number;
  trend?: GmpTrend;
  size?: 'sm' | 'md' | 'lg';
}

export const GmpBadge: React.FC<GmpBadgeProps> = ({
  gmp,
  percentage,
  trend,
  size = 'md',
}) => {
  if (gmp === undefined || gmp === null) {
    return <span className="text-xs text-slate-500 font-mono">₹0 (0.0%)</span>;
  }

  const pct = percentage ?? 0;
  const isPositive = pct > 0;
  const isNeutral = pct === 0;

  const bgStyle = isPositive
    ? 'bg-emerald-500/10 text-emerald-400 border-emerald-500/30'
    : isNeutral
    ? 'bg-slate-500/10 text-slate-400 border-slate-500/30'
    : 'bg-rose-500/10 text-rose-400 border-rose-500/30';

  const TrendIcon = () => {
    if (!trend) {
      return isPositive ? <TrendingUp className="w-3.5 h-3.5" /> : isNeutral ? <Minus className="w-3.5 h-3.5" /> : <TrendingDown className="w-3.5 h-3.5" />;
    }
    switch (trend) {
      case 'StronglyIncreasing':
      case 'Increasing':
        return <TrendingUp className="w-3.5 h-3.5 text-emerald-400" />;
      case 'StronglyDeclining':
      case 'Declining':
        return <TrendingDown className="w-3.5 h-3.5 text-rose-400" />;
      default:
        return <Minus className="w-3.5 h-3.5 text-slate-400" />;
    }
  };

  const padSize = size === 'sm' ? 'px-2 py-0.5 text-xs' : size === 'lg' ? 'px-3 py-1.5 text-base' : 'px-2.5 py-1 text-xs';

  return (
    <div className={`inline-flex items-center space-x-1.5 rounded-full font-mono font-semibold border ${bgStyle} ${padSize}`}>
      <TrendIcon />
      <span>₹{gmp.toLocaleString('en-IN')}</span>
      <span className="opacity-75">({pct > 0 ? `+${pct.toFixed(1)}` : pct.toFixed(1)}%)</span>
    </div>
  );
};
