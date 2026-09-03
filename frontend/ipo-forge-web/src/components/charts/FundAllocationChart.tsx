import React from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts';
import { FundUtilization } from '../../types';

interface FundAllocationChartProps {
  fundUtilization: FundUtilization;
}

const COLORS = ['#10b981', '#3b82f6', '#f59e0b', '#8b5cf6', '#ec4899', '#64748b'];

export const FundAllocationChart: React.FC<FundAllocationChartProps> = ({ fundUtilization }) => {
  if (!fundUtilization || !fundUtilization.objectives || fundUtilization.objectives.length === 0) {
    // Default Fresh vs OFS
    const data = [
      { name: 'Fresh Issue (Growth Capital)', value: fundUtilization?.freshIssueAmount || 1 },
      { name: 'Offer for Sale (Promoter / PE Exit)', value: fundUtilization?.ofsAmount || 0 },
    ].filter((d) => d.value > 0);

    return (
      <div className="w-full h-64">
        <ResponsiveContainer width="100%" height="100%">
          <PieChart>
            <Pie
              data={data}
              innerRadius={55}
              outerRadius={80}
              paddingAngle={4}
              dataKey="value"
            >
              <Cell fill="#10b981" />
              <Cell fill="#f59e0b" />
            </Pie>
            <Tooltip
              formatter={(val: any) => [`₹${Number(val).toLocaleString('en-IN')} Cr`, 'Amount']}
              contentStyle={{ backgroundColor: '#0f172a', borderColor: '#334155', borderRadius: '8px', fontSize: '12px' }}
            />
            <Legend
              wrapperStyle={{ fontSize: '11px', paddingTop: '10px' }}
              formatter={(value) => <span className="text-slate-300">{value}</span>}
            />
          </PieChart>
        </ResponsiveContainer>
      </div>
    );
  }

  const data = fundUtilization.objectives.map((obj) => ({
    name: obj.title.length > 35 ? `${obj.title.substring(0, 32)}...` : obj.title,
    value: obj.amountInCrores,
    percentage: obj.percentageOfTotal,
    category: obj.category,
  }));

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const p = payload[0].payload;
      return (
        <div className="bg-slate-900 border border-slate-700 p-3 rounded-xl shadow-xl text-xs space-y-1">
          <p className="font-semibold text-slate-200">{p.name}</p>
          <p className="text-emerald-400 font-mono font-bold">
            ₹{p.value?.toLocaleString('en-IN')} Cr ({p.percentage}%)
          </p>
          <span className="text-[10px] text-slate-400 block">Category: {p.category}</span>
        </div>
      );
    }
    return null;
  };

  return (
    <div className="w-full h-64">
      <ResponsiveContainer width="100%" height="100%">
        <PieChart>
          <Pie
            data={data}
            innerRadius={50}
            outerRadius={80}
            paddingAngle={3}
            dataKey="value"
          >
            {data.map((_, index) => (
              <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
            ))}
          </Pie>
          <Tooltip content={<CustomTooltip />} />
          <Legend
            wrapperStyle={{ fontSize: '10px', paddingTop: '5px' }}
            formatter={(value) => <span className="text-slate-400">{value}</span>}
          />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
};
