import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../services/api';
import { CompanyDetail, CompanySummary } from '../types';
import { FinancialTrendChart } from '../components/charts/FinancialTrendChart';
import { DisclaimerBanner } from '../components/common/DisclaimerBanner';
import { Building2, Search, ExternalLink, ArrowRight, ChevronRight, Layers } from 'lucide-react';

export const CompanyDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();

  // If no ID is provided, render Company Directory List
  const [companies, setCompanies] = useState<CompanySummary[]>([]);
  const [selectedCompany, setSelectedCompany] = useState<CompanyDetail | null>(null);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadData() {
      setLoading(true);
      try {
        if (id) {
          const detail = await api.getCompanyById(id);
          setSelectedCompany(detail);
        } else {
          const list = await api.getCompanies(search);
          setCompanies(list.items);
        }
      } catch (err) {
        console.error('Failed to load company data', err);
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, [id, search]);

  if (id && selectedCompany) {
    return (
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
        {/* Breadcrumbs */}
        <div className="flex items-center space-x-2 text-xs text-slate-400">
          <Link to="/" className="hover:text-emerald-400 transition">Home</Link>
          <ChevronRight className="w-3.5 h-3.5 text-slate-600" />
          <Link to="/companies" className="hover:text-emerald-400 transition">Companies</Link>
          <ChevronRight className="w-3.5 h-3.5 text-slate-600" />
          <span className="text-slate-200 font-medium">{selectedCompany.name}</span>
        </div>

        {/* Company Header */}
        <div className="glass-panel bg-slate-900 border border-slate-800 rounded-3xl p-6 sm:p-8 space-y-6">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <div className="flex items-center space-x-2 text-xs text-emerald-400 font-semibold mb-1">
                <span>{selectedCompany.sector}</span>
                <span>•</span>
                <span>{selectedCompany.industry}</span>
              </div>
              <h1 className="text-2xl sm:text-3xl font-extrabold text-white">{selectedCompany.name}</h1>
              <p className="text-xs text-slate-400 mt-0.5">Legal Name: {selectedCompany.legalName} • CIN: {selectedCompany.cin || '-'}</p>
            </div>

            {selectedCompany.website && (
              <a
                href={selectedCompany.website}
                target="_blank"
                rel="noopener noreferrer"
                className="px-4 py-2 bg-slate-800 hover:bg-slate-700 text-emerald-400 border border-slate-700 rounded-xl text-xs font-semibold flex items-center space-x-1.5 transition self-start sm:self-auto"
              >
                <span>Visit Website</span>
                <ExternalLink className="w-3.5 h-3.5" />
              </a>
            )}
          </div>

          <p className="text-xs sm:text-sm text-slate-300 leading-relaxed max-w-3xl">
            {selectedCompany.description}
          </p>

          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-xs">
            <div className="bg-slate-950/60 p-3.5 rounded-xl border border-slate-800">
              <span className="text-slate-500 uppercase text-[10px] block">Headquarters</span>
              <span className="font-semibold text-white mt-0.5 block">{selectedCompany.headquarters || 'India'}</span>
            </div>
            <div className="bg-slate-950/60 p-3.5 rounded-xl border border-slate-800">
              <span className="text-slate-500 uppercase text-[10px] block">Managing Director</span>
              <span className="font-semibold text-white mt-0.5 block">{selectedCompany.managingDirector || '-'}</span>
            </div>
            <div className="bg-slate-950/60 p-3.5 rounded-xl border border-slate-800">
              <span className="text-slate-500 uppercase text-[10px] block">Founded Year</span>
              <span className="font-semibold text-white mt-0.5 block">{selectedCompany.foundedYear || '-'}</span>
            </div>
            <div className="bg-slate-950/60 p-3.5 rounded-xl border border-slate-800">
              <span className="text-slate-500 uppercase text-[10px] block">Pre-Issue Promoter Stake</span>
              <span className="font-mono font-bold text-emerald-400 mt-0.5 block">
                {selectedCompany.promoterHoldingPreIssue !== undefined ? `${selectedCompany.promoterHoldingPreIssue}%` : '-'}
              </span>
            </div>
          </div>
        </div>

        {/* Associated IPO Issues */}
        {selectedCompany.ipos && selectedCompany.ipos.length > 0 && (
          <section className="space-y-4">
            <h3 className="text-lg font-bold text-white">Associated IPO Offerings</h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {selectedCompany.ipos.map((ipo) => (
                <div key={ipo.id} className="bg-slate-900/80 border border-slate-800 rounded-2xl p-5 flex items-center justify-between">
                  <div>
                    <h4 className="font-bold text-white text-sm">{ipo.name}</h4>
                    <span className="text-xs text-slate-400">{ipo.ipoType} • Status: {ipo.status}</span>
                  </div>
                  <Link
                    to={`/ipos/${ipo.id}`}
                    className="px-4 py-2 rounded-xl bg-emerald-500/10 border border-emerald-500/30 text-emerald-400 text-xs font-bold hover:bg-emerald-500 hover:text-slate-950 transition flex items-center space-x-1"
                  >
                    <span>Full Analysis</span>
                    <ArrowRight className="w-3.5 h-3.5" />
                  </Link>
                </div>
              ))}
            </div>
          </section>
        )}

        {/* Financial Multi-Year History */}
        {selectedCompany.financials && selectedCompany.financials.length > 0 && (
          <section className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
            <h3 className="text-base font-bold text-white">Historical Financial Statement Multi-Year Growth</h3>
            <FinancialTrendChart years={selectedCompany.financials} />
          </section>
        )}

        <DisclaimerBanner />
      </div>
    );
  }

  // Directory View
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
            Corporate Issuer Directory
          </h1>
          <p className="text-xs sm:text-sm text-slate-400 mt-1">
            Browse companies seeking equity public capital in Indian markets.
          </p>
        </div>

        <div className="relative w-full sm:w-80">
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            type="text"
            placeholder="Search company, sector..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-9 pr-3 py-2 bg-slate-900 border border-slate-800 rounded-xl text-xs text-white placeholder-slate-500 focus:outline-none focus:border-emerald-500"
          />
        </div>
      </div>

      {loading ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6 animate-pulse">
          {[1, 2, 3, 4, 5, 6].map((n) => (
            <div key={n} className="h-44 bg-slate-900 rounded-2xl border border-slate-800" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {companies.map((c) => (
            <Link
              key={c.id}
              to={`/companies/${c.id}`}
              className="glass-panel p-5 rounded-2xl border border-slate-800 hover:border-slate-700 bg-slate-900/70 hover:bg-slate-900 transition flex flex-col justify-between group shadow-lg"
            >
              <div>
                <div className="flex items-center space-x-1.5 text-xs text-emerald-400 font-semibold mb-1">
                  <Building2 className="w-3.5 h-3.5" />
                  <span>{c.sector}</span>
                </div>
                <h3 className="font-bold text-base text-white group-hover:text-emerald-400 transition">
                  {c.name}
                </h3>
                <p className="text-xs text-slate-400 line-clamp-2 mt-2 leading-relaxed">
                  {c.description}
                </p>
              </div>

              <div className="mt-4 pt-3 border-t border-slate-800/80 flex items-center justify-between text-xs text-slate-400">
                <span>HQ: {c.headquarters || 'India'}</span>
                <span className="text-emerald-400 font-semibold group-hover:translate-x-0.5 transition flex items-center">
                  <span>View Details</span>
                  <ArrowRight className="w-3.5 h-3.5 ml-1" />
                </span>
              </div>
            </Link>
          ))}
        </div>
      )}

      <DisclaimerBanner />
    </div>
  );
};
