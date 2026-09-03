import React, { useEffect, useState } from 'react';
import { api } from '../services/api';
import { IpoFilterRequest, IpoSummary, PagedResult, ScoreBreakdown } from '../types';
import { IpoCard } from '../components/ipo/IpoCard';
import { IpoTable } from '../components/ipo/IpoTable';
import { FilterPanel } from '../components/ipo/FilterPanel';
import { ScoreBreakdownModal } from '../components/common/ScoreBreakdownModal';
import { IpoComparisonModal } from '../components/comparison/IpoComparisonModal';
import { DisclaimerBanner } from '../components/common/DisclaimerBanner';
import { LayoutGrid, List, Search, ChevronLeft, ChevronRight, Scale, Share2 } from 'lucide-react';
import { useSearchParams } from 'react-router-dom';

export const IpoExplorerPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();

  const [ipos, setIpos] = useState<IpoSummary[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState<'grid' | 'table'>('grid');

  // Filter state
  const [filters, setFilters] = useState<IpoFilterRequest>({
    status: (searchParams.get('status') as any) || undefined,
    ipoType: (searchParams.get('type') as any) || undefined,
    search: searchParams.get('q') || undefined,
    sortBy: 'listingGainScore',
    sortDescending: true,
    pageNumber: 1,
    pageSize: 12,
  });

  // Score Modal State
  const [selectedIpoForScore, setSelectedIpoForScore] = useState<IpoSummary | null>(null);
  const [activeBreakdown, setActiveBreakdown] = useState<ScoreBreakdown | null>(null);
  const [isScoreModalOpen, setIsScoreModalOpen] = useState(false);

  // Comparison State
  const [isComparisonModalOpen, setIsComparisonModalOpen] = useState(false);
  const [comparisonIpos, setComparisonIpos] = useState<IpoSummary[]>([]);

  const sectors = [
    'Technology',
    'Energy & Utilities',
    'Financial Services',
    'Consumer Discretionary',
    'Automobile',
    'Consumer Staples',
    'Capital Goods & Automation',
    'Healthcare & Pharma',
    'Infrastructure & Logistics',
  ];

  const fetchIpos = async () => {
    setLoading(true);
    try {
      const res: PagedResult<IpoSummary> = await api.getIpos({
        ...filters,
        pageNumber,
      });
      setIpos(res.items);
      setTotalCount(res.totalCount);
      setTotalPages(res.totalPages || 1);

      if (comparisonIpos.length === 0 && res.items.length > 0) {
        setComparisonIpos(res.items.slice(0, 3));
      }
    } catch (err) {
      console.error('Failed to fetch IPOs', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchIpos();
  }, [filters, pageNumber]);

  const handleFilterChange = (newFilters: Partial<IpoFilterRequest>) => {
    setFilters((prev) => ({ ...prev, ...newFilters }));
    setPageNumber(1);
  };

  const handleReset = () => {
    setFilters({
      sortBy: 'listingGainScore',
      sortDescending: true,
      pageNumber: 1,
      pageSize: 12,
    });
    setPageNumber(1);
    setSearchParams({});
  };

  const handleOpenScoreBreakdown = async (ipo: IpoSummary) => {
    setSelectedIpoForScore(ipo);
    try {
      const detail = await api.getIpoById(ipo.id);
      if (detail.scores) {
        setActiveBreakdown(detail.scores);
        setIsScoreModalOpen(true);
      }
    } catch (err) {
      console.error('Failed to load score details', err);
    }
  };

  const handleToggleIpoSelection = (ipo: IpoSummary) => {
    setComparisonIpos((prev) => {
      if (prev.some((i) => i.id === ipo.id)) {
        return prev.filter((i) => i.id !== ipo.id);
      } else {
        return [...prev, ipo];
      }
    });
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
            Indian IPO Explorer & Screener
          </h1>
          <p className="text-xs sm:text-sm text-slate-400 mt-1">
            Real-time intelligence on Mainboard & SME IPOs with multi-pillar algorithmic verdicts.
          </p>
        </div>

        {/* View Switcher & Compare Action */}
        <div className="flex items-center space-x-3 self-start sm:self-auto">
          <button
            onClick={() => setIsComparisonModalOpen(true)}
            className="px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-xs shadow-lg shadow-emerald-950/40 transition flex items-center space-x-1.5"
          >
            <Scale className="w-4 h-4" />
            <span>Compare & Which to Apply ({comparisonIpos.length})</span>
          </button>

          <div className="bg-slate-900 border border-slate-800 p-1 rounded-xl flex items-center space-x-1">
            <button
              onClick={() => setViewMode('grid')}
              className={`p-2 rounded-lg text-xs font-semibold flex items-center space-x-1.5 transition ${
                viewMode === 'grid'
                  ? 'bg-slate-800 text-emerald-400 shadow'
                  : 'text-slate-400 hover:text-white'
              }`}
              title="Card Grid View"
            >
              <LayoutGrid className="w-4 h-4" />
              <span className="hidden sm:inline">Cards</span>
            </button>
            <button
              onClick={() => setViewMode('table')}
              className={`p-2 rounded-lg text-xs font-semibold flex items-center space-x-1.5 transition ${
                viewMode === 'table'
                  ? 'bg-slate-800 text-emerald-400 shadow'
                  : 'text-slate-400 hover:text-white'
              }`}
              title="High-Density Table View"
            >
              <List className="w-4 h-4" />
              <span className="hidden sm:inline">Table</span>
            </button>
          </div>
        </div>
      </div>

      {/* Filter & Screener Panel */}
      <FilterPanel
        filters={filters}
        onFilterChange={handleFilterChange}
        onReset={handleReset}
        sectors={sectors}
      />

      {/* Results Count & Search Input */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 text-xs text-slate-400">
        <div>
          Showing <span className="font-bold text-white">{ipos.length}</span> of{' '}
          <span className="font-bold text-white">{totalCount}</span> IPO opportunities
        </div>

        <div className="relative w-full sm:w-72">
          <Search className="w-3.5 h-3.5 text-slate-500 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search by name, symbol..."
            value={filters.search || ''}
            onChange={(e) => handleFilterChange({ search: e.target.value || undefined })}
            className="w-full pl-8 pr-3 py-1.5 bg-slate-900 border border-slate-800 rounded-lg text-xs text-white placeholder-slate-500 focus:outline-none focus:border-emerald-500"
          />
        </div>
      </div>

      {/* Main IPO Display (Grid or Table) */}
      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 animate-pulse">
          {[1, 2, 3, 4, 5, 6].map((n) => (
            <div key={n} className="h-80 bg-slate-900 rounded-2xl border border-slate-800" />
          ))}
        </div>
      ) : ipos.length === 0 ? (
        <div className="p-12 text-center bg-slate-900/40 rounded-2xl border border-slate-800 space-y-3">
          <p className="text-slate-300 font-semibold text-sm">No IPOs matched your active filter criteria.</p>
          <p className="text-xs text-slate-500">Try adjusting your score sliders or resetting filters.</p>
          <button
            onClick={handleReset}
            className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-xs font-semibold text-white transition"
          >
            Reset All Filters
          </button>
        </div>
      ) : viewMode === 'grid' ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {ipos.map((ipo) => (
            <IpoCard
              key={ipo.id}
              ipo={ipo}
              onOpenScoreBreakdown={handleOpenScoreBreakdown}
            />
          ))}
        </div>
      ) : (
        <IpoTable
          ipos={ipos}
          onOpenScoreBreakdown={handleOpenScoreBreakdown}
        />
      )}

      {/* Pagination Controls */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between pt-4 border-t border-slate-800 text-xs text-slate-400">
          <div>
            Page <span className="font-bold text-white">{pageNumber}</span> of{' '}
            <span className="font-bold text-white">{totalPages}</span>
          </div>
          <div className="flex items-center space-x-2">
            <button
              onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
              disabled={pageNumber === 1}
              className="px-3 py-1.5 rounded-lg border border-slate-800 bg-slate-900 text-slate-300 hover:bg-slate-800 disabled:opacity-40 disabled:cursor-not-allowed transition flex items-center space-x-1"
            >
              <ChevronLeft className="w-4 h-4" />
              <span>Previous</span>
            </button>
            <button
              onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
              disabled={pageNumber === totalPages}
              className="px-3 py-1.5 rounded-lg border border-slate-800 bg-slate-900 text-slate-300 hover:bg-slate-800 disabled:opacity-40 disabled:cursor-not-allowed transition flex items-center space-x-1"
            >
              <span>Next</span>
              <ChevronRight className="w-4 h-4" />
            </button>
          </div>
        </div>
      )}

      {/* Statutory Disclaimer */}
      <DisclaimerBanner />

      {/* Score Transparency Modal */}
      {selectedIpoForScore && activeBreakdown && (
        <ScoreBreakdownModal
          scores={activeBreakdown}
          ipoName={selectedIpoForScore.name}
          isOpen={isScoreModalOpen}
          onClose={() => {
            setIsScoreModalOpen(false);
            setSelectedIpoForScore(null);
            setActiveBreakdown(null);
          }}
        />
      )}

      {/* Comparison Modal */}
      <IpoComparisonModal
        ipos={comparisonIpos}
        allAvailableIpos={ipos}
        isOpen={isComparisonModalOpen}
        onClose={() => setIsComparisonModalOpen(false)}
        onToggleIpoSelection={handleToggleIpoSelection}
      />
    </div>
  );
};
