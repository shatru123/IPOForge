import React from 'react';
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import { GmpSnapshot } from '../../types';

interface GmpTrendChartProps {
  snapshots: GmpSnapshot[];
  issuePrice?: number;
}

export const GmpTrendChart: React.FC<GmpTrendChartProps> = ({ snapshots, issuePrice }) => {
  if (!snapshots || snapshots.length === 0) {
    return (
      <div className="h-56 flex items-center justify-center text-xs text-slate-500 bg-slate-900/40 rounded-xl border border-slate-800">
        No historical GMP quotes recorded yet.
      </div>
    );
  }

  const data = snapshots.map((s) => ({
    date: new Date(s.observedAt).toLocaleDateString('en-IN', { month: 'short', day: 'numeric' }),
    gmp: s.gmp,
    percentage: s.gmpPercentage,
    estPrice: s.estimatedListingPrice,
    source: s.source,
  }));

  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      const p = payload[0].payload;
      return (
        <div className="bg-slate-900 border border-slate-700 p-3 rounded-xl shadow-xl text-xs space-y-1">
          <p className="font-semibold text-slate-300">{label}</p>
          <p className="text-emerald-400 font-mono font-bold">
            GMP: +₹{p.gmp} ({p.percentage}%)
          </p>
          <p className="text-slate-400 font-mono">
            Est. Listing: ₹{p.estPrice}
          </p>
          <p className="text-[10px] text-slate-500">Source: {p.source}</p>
        </div>
      );
    }
    return null;
  };

  return (
    <div className="w-full h-64">
      <ResponsiveContainer width="100%" height="100%">
        <AreaChart data={data} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
          <defs>
            <linearGradient id="gmpGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor="#10b981" stopOpacity={0.4} />
              <stop offset="95%" stopColor="#10b981" stopOpacity={0.0} />
            </linearGradient>
          </defs>
          <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" vertical={false} />
          <XAxis dataKey="date" stroke="#64748b" tick={{ fontSize: 11 }} />
          <YAxis stroke="#64748b" tick={{ fontSize: 11 }} domain={['auto', 'auto']} />
          <Tooltip content={<CustomTooltip />} />
          <Area
            type="monotone"
            dataKey="gmp"
            stroke="#10b981"
            strokeWidth={2.5}
            fillOpacity={1}
            fill="url(#gmpGradient)"
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
};
