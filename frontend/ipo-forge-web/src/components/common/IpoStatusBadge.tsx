import React from 'react';
import { IpoStatus, IpoType } from '../../types';

interface IpoStatusBadgeProps {
  status: IpoStatus;
  type?: IpoType;
}

export const IpoStatusBadge: React.FC<IpoStatusBadgeProps> = ({ status, type }) => {
  let style = 'bg-slate-800 text-slate-300 border-slate-700';
  let dotColor = 'bg-slate-400';

  switch (status) {
    case 'Open':
      style = 'bg-emerald-500/15 text-emerald-400 border-emerald-500/30 ring-1 ring-emerald-500/20 animate-pulse';
      dotColor = 'bg-emerald-400';
      break;
    case 'Upcoming':
      style = 'bg-blue-500/15 text-blue-400 border-blue-500/30';
      dotColor = 'bg-blue-400';
      break;
    case 'AllotmentOut':
      style = 'bg-purple-500/15 text-purple-400 border-purple-500/30';
      dotColor = 'bg-purple-400';
      break;
    case 'Closed':
      style = 'bg-amber-500/15 text-amber-400 border-amber-500/30';
      dotColor = 'bg-amber-400';
      break;
    case 'Listed':
      style = 'bg-slate-700/40 text-slate-300 border-slate-700';
      dotColor = 'bg-slate-400';
      break;
  }

  return (
    <div className="flex items-center space-x-1.5">
      <span className={`inline-flex items-center space-x-1.5 text-xs font-semibold px-2.5 py-0.5 rounded-full border ${style}`}>
        <span className={`w-1.5 h-1.5 rounded-full ${dotColor}`} />
        <span>{status === 'AllotmentOut' ? 'Allotment Out' : status}</span>
      </span>

      {type && (
        <span className={`text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded border ${
          type === 'Sme' ? 'bg-amber-950/40 text-amber-400 border-amber-800/60' : 'bg-slate-800 text-slate-400 border-slate-700'
        }`}>
          {type}
        </span>
      )}
    </div>
  );
};
