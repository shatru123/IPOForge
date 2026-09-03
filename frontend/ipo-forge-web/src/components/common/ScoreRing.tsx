import React from 'react';
import { RecommendationRating } from '../../types';

interface ScoreRingProps {
  score: number;
  label: string;
  rating?: RecommendationRating;
  size?: 'sm' | 'md' | 'lg';
  showRatingBadge?: boolean;
  onClick?: () => void;
}

export const ScoreRing: React.FC<ScoreRingProps> = ({
  score,
  label,
  rating,
  size = 'md',
  showRatingBadge = true,
  onClick,
}) => {
  const safeScore = Math.min(100, Math.max(0, Math.round(score)));

  // Color mapping based on score
  let strokeColor = '#ef4444'; // Avoid (0-34)
  let badgeBg = 'bg-rose-500/10 text-rose-400 border-rose-500/30';
  let textColor = 'text-rose-400';

  if (safeScore >= 80) {
    strokeColor = '#10b981'; // Strong (80-100)
    badgeBg = 'bg-emerald-500/10 text-emerald-400 border-emerald-500/30';
    textColor = 'text-emerald-400';
  } else if (safeScore >= 65) {
    strokeColor = '#22c55e'; // Positive (65-79)
    badgeBg = 'bg-green-500/10 text-green-400 border-green-500/30';
    textColor = 'text-green-400';
  } else if (safeScore >= 50) {
    strokeColor = '#eab308'; // Neutral (50-64)
    badgeBg = 'bg-yellow-500/10 text-yellow-400 border-yellow-500/30';
    textColor = 'text-yellow-400';
  } else if (safeScore >= 35) {
    strokeColor = '#f97316'; // Weak (35-49)
    badgeBg = 'bg-orange-500/10 text-orange-400 border-orange-500/30';
    textColor = 'text-orange-400';
  }

  const dim = size === 'sm' ? 84 : size === 'lg' ? 148 : 112;
  const strokeWidth = size === 'sm' ? 6 : size === 'lg' ? 10 : 8;
  const radius = (dim - strokeWidth * 2) / 2;
  const circumference = 2 * Math.PI * radius;
  const strokeDashoffset = circumference - (safeScore / 100) * circumference;

  return (
    <div
      onClick={onClick}
      className={`flex flex-col items-center select-none ${onClick ? 'cursor-pointer hover:scale-[1.02] transition-transform' : ''}`}
    >
      <div className="relative flex items-center justify-center" style={{ width: dim, height: dim }}>
        <svg width={dim} height={dim} className="transform -rotate-90">
          {/* Background circle */}
          <circle
            cx={dim / 2}
            cy={dim / 2}
            r={radius}
            stroke="#1e293b"
            strokeWidth={strokeWidth}
            fill="transparent"
          />
          {/* Progress circle */}
          <circle
            cx={dim / 2}
            cy={dim / 2}
            r={radius}
            stroke={strokeColor}
            strokeWidth={strokeWidth}
            strokeDasharray={circumference}
            strokeDashoffset={strokeDashoffset}
            strokeLinecap="round"
            fill="transparent"
            className="transition-all duration-1000 ease-out"
          />
        </svg>

        {/* Center score display */}
        <div className="absolute inset-0 flex flex-col items-center justify-center text-center">
          <span className={`font-mono font-bold tracking-tight ${textColor} ${size === 'sm' ? 'text-lg' : size === 'lg' ? 'text-3xl' : 'text-2xl'}`}>
            {safeScore}
          </span>
          <span className="text-[10px] text-slate-400 uppercase font-medium tracking-wider -mt-0.5">/ 100</span>
        </div>
      </div>

      <span className="text-xs font-semibold text-slate-300 mt-2 text-center">{label}</span>

      {showRatingBadge && rating && (
        <span className={`mt-1 text-[11px] font-semibold px-2 py-0.5 rounded-full border ${badgeBg}`}>
          {rating}
        </span>
      )}
    </div>
  );
};
