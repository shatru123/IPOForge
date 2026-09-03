import React from 'react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from 'recharts';
import { SubscriptionSnapshot } from '../../types';

interface SubscriptionChartProps {
  days: SubscriptionSnapshot[];
}

export const SubscriptionChart: React.FC<SubscriptionChartProps> = ({ days }) => {
  if (!days || days.length === 0) {
    return (
      <div className="h-56 flex items-center justify-center text-xs text-slate-500 bg-slate-900/40 rounded-xl border border-slate-800">
        Subscription bidding data not yet reported.
      </div>
    );
  }

  const data = days.map((d) => ({
    name: `Day ${d.dayNumber}`,
    QIB: d.qibSubscription,
    NII: d.niiSubscription,
    Retail: d.retailSubscription,
    Total: d.totalSubscription,
  }));

  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      return (
        <div className="bg-slate-900 border border-slate-700 p-3 rounded-xl shadow-xl text-xs space-y-1 font-mono">
          <p className="font-semibold text-slate-300 font-sans">{label} Bidding Progress</p>
          <p className="text-purple-400">QIB (Inst.): {payload[0]?.value}x</p>
          <p className="text-amber-400">NII (HNI): {payload[1]?.value}x</p>
          <p className="text-blue-400">Retail: {payload[2]?.value}x</p>
          <div className="border-t border-slate-800 pt-1 text-emerald-400 font-bold">
            Total Multiplier: {payload[3]?.value}x
          </div>
        </div>
      );
    }
    return null;
  };

  return (
    <div className="w-full h-64">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={data} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" vertical={false} />
          <XAxis dataKey="name" stroke="#64748b" tick={{ fontSize: 11 }} />
          <YAxis stroke="#64748b" tick={{ fontSize: 11 }} />
          <Tooltip content={<CustomTooltip />} />
          <Legend
            wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }}
            formatter={(value) => <span className="text-slate-400">{value}</span>}
          />
          <Bar dataKey="QIB" fill="#a855f7" radius={[4, 4, 0, 0]} />
          <Bar dataKey="NII" fill="#f59e0b" radius={[4, 4, 0, 0]} />
          <Bar dataKey="Retail" fill="#3b82f6" radius={[4, 4, 0, 0]} />
          <Bar dataKey="Total" fill="#10b981" radius={[4, 4, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
};
