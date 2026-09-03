import React from 'react';
import { ScatterChart, Scatter, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, ReferenceLine } from 'recharts';
import { GmpAccuracyItem } from '../../types';

interface GmpAccuracyScatterChartProps {
  items: GmpAccuracyItem[];
}

export const GmpAccuracyScatterChart: React.FC<GmpAccuracyScatterChartProps> = ({ items }) => {
  if (!items || items.length === 0) {
    return (
      <div className="h-56 flex items-center justify-center text-xs text-slate-500 bg-slate-900/40 rounded-xl border border-slate-800">
        No historical listing comparison points available.
      </div>
    );
  }

  const data = items.map((item) => ({
    name: item.ipoName,
    predicted: item.gmpPredictedGainPercent ?? 0,
    actual: item.actualListingGainPercent ?? 0,
    error: item.predictionErrorPercent ?? 0,
    issuePrice: item.issuePrice,
    actualPrice: item.actualListingPrice,
  }));

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const p = payload[0].payload;
      return (
        <div className="bg-slate-900 border border-slate-700 p-3 rounded-xl shadow-xl text-xs space-y-1 font-mono">
          <p className="font-semibold text-white font-sans">{p.name}</p>
          <p className="text-blue-400">Predicted Gain: +{p.predicted}%</p>
          <p className="text-emerald-400">Actual Listing Gain: +{p.actual}%</p>
          <p className="text-slate-400 text-[10px]">
            Issue: ₹{p.issuePrice} → Listed: ₹{p.actualPrice} (Diff: {p.error}%)
          </p>
        </div>
      );
    }
    return null;
  };

  return (
    <div className="w-full h-72">
      <ResponsiveContainer width="100%" height="100%">
        <ScatterChart margin={{ top: 20, right: 20, bottom: 20, left: -10 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#1e293b" />
          <XAxis
            type="number"
            dataKey="predicted"
            name="Predicted Gain"
            unit="%"
            stroke="#64748b"
            tick={{ fontSize: 11 }}
            label={{ value: 'Predicted Gain via GMP (%)', position: 'bottom', offset: 0, fill: '#94a3b8', fontSize: 11 }}
          />
          <YAxis
            type="number"
            dataKey="actual"
            name="Actual Gain"
            unit="%"
            stroke="#64748b"
            tick={{ fontSize: 11 }}
            label={{ value: 'Actual Listing Gain (%)', angle: -90, position: 'left', offset: 10, fill: '#94a3b8', fontSize: 11 }}
          />
          <Tooltip content={<CustomTooltip />} />
          {/* 45-degree Perfect Accuracy Line */}
          <ReferenceLine
            segment={[{ x: 0, y: 0 }, { x: 150, y: 150 }]}
            stroke="#10b981"
            strokeDasharray="4 4"
            label={{ value: 'Perfect Accuracy', fill: '#10b981', fontSize: 10 }}
          />
          <Scatter name="IPOs" data={data} fill="#38bdf8" shape="circle" />
        </ScatterChart>
      </ResponsiveContainer>
    </div>
  );
};
