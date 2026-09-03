import React from 'react';
import { IpoFilterRequest, IpoStatus, IpoType } from '../../types';
import { Filter, RotateCcw } from 'lucide-react';

interface FilterPanelProps {
  filters: IpoFilterRequest;
  onFilterChange: (newFilters: Partial<IpoFilterRequest>) => void;
  onReset: () => void;
  sectors: string[];
}

export const FilterPanel: React.FC<FilterPanelProps> = ({
  filters,
  onFilterChange,
  onReset,
  sectors,
}) => {
  return (
    <div className="bg-slate-900/90 border border-slate-800 rounded-2xl p-5 shadow-xl space-y-4">
      <div className="flex items-center justify-between border-b border-slate-800 pb-3">
        <div className="flex items-center space-x-2 text-sm font-bold text-white">
          <Filter className="w-4 h-4 text-emerald-400" />
          <span>Filter & Screener Controls</span>
        </div>
        <button
          onClick={onReset}
          className="text-xs text-slate-400 hover:text-emerald-400 flex items-center space-x-1 transition"
        >
          <RotateCcw className="w-3 h-3" />
          <span>Reset</span>
        </button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 text-xs">
        {/* Type / Segment */}
        <div>
          <label className="block text-slate-400 font-semibold mb-1.5 uppercase text-[10px] tracking-wider">
            Issue Category
          </label>
          <div className="grid grid-cols-3 gap-1 bg-slate-950 p-1 rounded-xl border border-slate-800">
            {(['All', 'Mainboard', 'Sme'] as const).map((type) => {
              const active = (!filters.ipoType && type === 'All') || filters.ipoType === type;
              return (
                <button
                  key={type}
                  onClick={() => onFilterChange({ ipoType: type === 'All' ? undefined : (type as IpoType) })}
                  className={`py-1.5 rounded-lg font-semibold text-xs transition ${
                    active ? 'bg-slate-800 text-emerald-400 shadow' : 'text-slate-400 hover:text-white'
                  }`}
                >
                  {type}
                </button>
              );
            })}
          </div>
        </div>

        {/* Status */}
        <div>
          <label className="block text-slate-400 font-semibold mb-1.5 uppercase text-[10px] tracking-wider">
            Lifecycle Status
          </label>
          <select
            value={filters.status || ''}
            onChange={(e) =>
              onFilterChange({ status: (e.target.value || undefined) as IpoStatus | undefined })
            }
            className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-100 focus:outline-none focus:border-emerald-500"
          >
            <option value="">All Statuses</option>
            <option value="Open">Currently Open (Bidding Active)</option>
            <option value="Upcoming">Upcoming / Forthcoming</option>
            <option value="AllotmentOut">Allotment Finalized</option>
            <option value="Listed">Listed Historical</option>
            <option value="Closed">Closed</option>
          </select>
        </div>

        {/* Sector */}
        <div>
          <label className="block text-slate-400 font-semibold mb-1.5 uppercase text-[10px] tracking-wider">
            Industry / Sector
          </label>
          <select
            value={filters.sector || ''}
            onChange={(e) => onFilterChange({ sector: e.target.value || undefined })}
            className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-100 focus:outline-none focus:border-emerald-500"
          >
            <option value="">All Sectors</option>
            {sectors.map((sec) => (
              <option key={sec} value={sec}>
                {sec}
              </option>
            ))}
          </select>
        </div>

        {/* Sort By */}
        <div>
          <label className="block text-slate-400 font-semibold mb-1.5 uppercase text-[10px] tracking-wider">
            Sort Order
          </label>
          <select
            value={`${filters.sortBy || 'listingGainScore'}_${filters.sortDescending !== false ? 'desc' : 'asc'}`}
            onChange={(e) => {
              const [field, dir] = e.target.value.split('_');
              onFilterChange({ sortBy: field, sortDescending: dir === 'desc' });
            }}
            className="w-full bg-slate-950 border border-slate-800 rounded-xl px-3 py-2 text-xs text-slate-100 focus:outline-none focus:border-emerald-500"
          >
            <option value="listingGainScore_desc">Highest Listing Gain Score</option>
            <option value="longTermScore_desc">Highest Long-Term Score</option>
            <option value="gmpPercentage_desc">Highest GMP %</option>
            <option value="openDate_desc">Latest Open Date</option>
            <option value="issueSize_desc">Largest Issue Size</option>
          </select>
        </div>
      </div>

      {/* Score Threshold Sliders */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2 border-t border-slate-800/80">
        <div>
          <div className="flex justify-between text-xs mb-1">
            <span className="text-slate-400">Min Listing Gain Score:</span>
            <span className="font-mono font-bold text-emerald-400">
              {filters.minListingGainScore || 0}+
            </span>
          </div>
          <input
            type="range"
            min="0"
            max="90"
            step="10"
            value={filters.minListingGainScore || 0}
            onChange={(e) => onFilterChange({ minListingGainScore: Number(e.target.value) || undefined })}
            className="w-full accent-emerald-500 bg-slate-800 h-1.5 rounded-lg"
          />
        </div>

        <div>
          <div className="flex justify-between text-xs mb-1">
            <span className="text-slate-400">Min Long-Term Score:</span>
            <span className="font-mono font-bold text-blue-400">
              {filters.minLongTermScore || 0}+
            </span>
          </div>
          <input
            type="range"
            min="0"
            max="90"
            step="10"
            value={filters.minLongTermScore || 0}
            onChange={(e) => onFilterChange({ minLongTermScore: Number(e.target.value) || undefined })}
            className="w-full accent-blue-500 bg-slate-800 h-1.5 rounded-lg"
          />
        </div>
      </div>
    </div>
  );
};
