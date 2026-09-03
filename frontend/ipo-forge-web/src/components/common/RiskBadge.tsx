import React from 'react';
import { RiskSeverity } from '../../types';
import { AlertTriangle, AlertOctagon, Info } from 'lucide-react';

interface RiskBadgeProps {
  severity: RiskSeverity;
  size?: 'sm' | 'md';
}

export const RiskBadge: React.FC<RiskBadgeProps> = ({ severity, size = 'md' }) => {
  let style = 'bg-slate-800 text-slate-300 border-slate-700';
  let Icon = Info;

  switch (severity) {
    case 'Critical':
    case 'High':
      style = 'bg-rose-500/15 text-rose-400 border-rose-500/30';
      Icon = AlertOctagon;
      break;
    case 'Medium':
      style = 'bg-amber-500/15 text-amber-400 border-amber-500/30';
      Icon = AlertTriangle;
      break;
    case 'Low':
      style = 'bg-emerald-500/15 text-emerald-400 border-emerald-500/30';
      Icon = Info;
      break;
  }

  const pad = size === 'sm' ? 'px-2 py-0.5 text-[10px]' : 'px-2.5 py-1 text-xs';

  return (
    <span className={`inline-flex items-center space-x-1 font-semibold rounded-full border ${style} ${pad}`}>
      <Icon className={size === 'sm' ? 'w-3 h-3' : 'w-3.5 h-3.5'} />
      <span>{severity} Risk</span>
    </span>
  );
};
