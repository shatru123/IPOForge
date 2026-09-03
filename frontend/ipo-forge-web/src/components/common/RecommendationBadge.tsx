import React from 'react';
import { RecommendationRating } from '../../types';

interface RecommendationBadgeProps {
  rating: RecommendationRating;
  prefix?: string;
  size?: 'sm' | 'md';
}

export const RecommendationBadge: React.FC<RecommendationBadgeProps> = ({
  rating,
  prefix,
  size = 'md',
}) => {
  let style = 'bg-slate-800 text-slate-300 border-slate-700';

  switch (rating) {
    case 'Strong':
      style = 'bg-emerald-500/15 text-emerald-400 border-emerald-500/30';
      break;
    case 'Positive':
      style = 'bg-green-500/15 text-green-400 border-green-500/30';
      break;
    case 'Neutral':
      style = 'bg-yellow-500/15 text-yellow-400 border-yellow-500/30';
      break;
    case 'Weak':
      style = 'bg-orange-500/15 text-orange-400 border-orange-500/30';
      break;
    case 'Avoid':
      style = 'bg-rose-500/15 text-rose-400 border-rose-500/30';
      break;
  }

  const pad = size === 'sm' ? 'px-2 py-0.5 text-[11px]' : 'px-2.5 py-1 text-xs';

  return (
    <span className={`inline-flex items-center font-semibold rounded-full border ${style} ${pad}`}>
      {prefix && <span className="opacity-75 mr-1 font-normal">{prefix}:</span>}
      {rating}
    </span>
  );
};
