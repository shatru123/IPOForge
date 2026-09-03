import React from 'react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid, Legend } from 'recharts';
import { FinancialYear } from '../../types';

interface FinancialTrendChartProps {
  years: FinancialYear[];
}

export const FinancialTrendChart: React.FC<FinancialTrendChartProps> = ({ years }) => {
  if (!years || years.length === 0) {
    return (
      <div className="h-56 flex items-center justify-center text-xs text-slate-500 bg-slate-900/40 rounded-xl border border-slate-800">
        No financial data statements available.
      </div>
    );
  }

  const data = years.map((y) => ({
    year: y.fiscalYear,
    Revenue: y.revenue,
    EBITDA: y.ebitda,
    PAT: y.pat,
  }));

  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      return (
        <div className="bg-slate-900 border border-slate-700 p-3 rounded-xl shadow-xl text-xs space-y-1 font-mono">
          <p className="font-semibold text-slate-300 font-sans">{label} Financial Performance</p>
          <p className="text-blue-400">Revenue: ₹{payload[0]?.value?.toLocaleString('en-IN')} Cr</p>
          <p className="text-emerald-400">EBITDA: ₹{payload[1]?.value?.toLocaleString('en-IN')} Cr</p>
          <p className="text-teal-300">PAT: ₹{payload[2]?.value?.toLocaleString('en-IN')} Cr</p>
        </div>
      );
    }
    return null;
  };

  return (
    <div className="w-full h-64">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={data} margin={{ top: 10, right: 10, left: -10, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" vertical={false} />
          <XAxis dataKey="year" stroke="#64748b" tick={{ fontSize: 11 }} />
          <YAxis stroke="#64748b" tick={{ fontSize: 11 }} />
          <Tooltip content={<CustomTooltip />} />
          <Legend
            wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }}
            formatter={(value) => <span className="text-slate-400">{value} (₹ Cr)</span>}
          />
          <Bar dataKey="Revenue" fill="#3b82f6" radius={[4, 4, 0, 0]} />
          <Bar dataKey="EBITDA" fill="#10b981" radius={[4, 4, 0, 0]} />
          <Bar dataKey="PAT" fill="#2dd4bf" radius={[4, 4, 0, 0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
};
