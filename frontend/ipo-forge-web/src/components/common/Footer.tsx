import React from 'react';
import { Link } from 'react-router-dom';
import { Flame, ShieldAlert, ExternalLink, Activity, Mail, Phone, User, Heart } from 'lucide-react';

export const Footer: React.FC = () => {
  return (
    <footer className="bg-navy-950 border-t border-slate-800 text-slate-400 text-xs mt-20">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-8">
          {/* Col 1: Platform Overview */}
          <div className="space-y-3">
            <div className="flex items-center space-x-2">
              <Flame className="w-5 h-5 text-emerald-400" />
              <span className="text-white font-extrabold text-base tracking-tight">
                IPO<span className="text-emerald-400">Forge</span>
              </span>
            </div>
            <p className="text-slate-400 leading-relaxed text-xs">
              Next-generation Indian IPO Intelligence platform enabling rapid 30-second investment evaluation through multi-pillar quantitative scoring, valuation benchmarking, and GMP trends.
            </p>
            <div className="flex items-center space-x-2 text-[11px] text-emerald-400 font-mono">
              <Activity className="w-3.5 h-3.5 animate-pulse" />
              <span>Free-Tier Engine • Render & PostgreSQL</span>
            </div>
          </div>

          {/* Col 2: Navigation */}
          <div>
            <h4 className="text-white font-bold mb-3 text-xs uppercase tracking-wider">Platform Navigation</h4>
            <ul className="space-y-2">
              <li><Link to="/" className="hover:text-emerald-400 transition">Market Dashboard</Link></li>
              <li><Link to="/ipos" className="hover:text-emerald-400 transition">IPO Explorer & Screener</Link></li>
              <li><Link to="/gmp" className="hover:text-emerald-400 transition">Real-Time GMP Tracker</Link></li>
              <li><Link to="/companies" className="hover:text-emerald-400 transition">Company Directory</Link></li>
              <li><Link to="/watchlist" className="hover:text-emerald-400 transition">Investor Watchlist</Link></li>
              <li>
                <a href="http://localhost:5050/health" target="_blank" rel="noopener noreferrer" className="hover:text-emerald-400 transition flex items-center space-x-1 font-mono text-[11px]">
                  <span>System Healthcheck API</span>
                  <ExternalLink className="w-3 h-3" />
                </a>
              </li>
            </ul>
          </div>

          {/* Col 3: Creator Profile & Contact */}
          <div>
            <h4 className="text-white font-bold mb-3 text-xs uppercase tracking-wider">Creator & Developer</h4>
            <div className="bg-slate-900/90 border border-slate-800 rounded-xl p-3.5 space-y-2.5">
              <div className="flex items-center space-x-2">
                <div className="w-7 h-7 rounded-lg bg-emerald-500/20 border border-emerald-500/40 flex items-center justify-center text-emerald-400">
                  <User className="w-4 h-4" />
                </div>
                <div>
                  <span className="text-white font-bold text-xs block">Shatrughna Ambhore</span>
                  <span className="text-[10px] text-emerald-400 font-medium">Platform Architect</span>
                </div>
              </div>

              <div className="pt-2 border-t border-slate-800 space-y-1.5 text-[11px]">
                <a
                  href="mailto:ambhoreshatrughna@gmail.com"
                  className="flex items-center space-x-2 text-slate-300 hover:text-emerald-400 transition"
                >
                  <Mail className="w-3.5 h-3.5 text-emerald-400 flex-shrink-0" />
                  <span className="truncate">ambhoreshatrughna@gmail.com</span>
                </a>
                <a
                  href="tel:+919604466334"
                  className="flex items-center space-x-2 text-slate-300 hover:text-emerald-400 transition"
                >
                  <Phone className="w-3.5 h-3.5 text-emerald-400 flex-shrink-0" />
                  <span>+91 9604466334</span>
                </a>
              </div>
            </div>
          </div>

          {/* Col 4: Regulatory Compliance */}
          <div>
            <h4 className="text-white font-bold mb-3 text-xs uppercase tracking-wider">Regulatory Compliance</h4>
            <div className="bg-slate-900/60 p-3.5 rounded-xl border border-slate-800 text-[11px] leading-relaxed text-slate-400 space-y-1">
              <div className="flex items-center space-x-1 text-amber-400 font-semibold mb-1">
                <ShieldAlert className="w-4 h-4" />
                <span>Statutory Disclaimer</span>
              </div>
              <p>
                Not SEBI registered advice. All scores, valuations, and predictions are algorithmic computations for academic and research reference only.
              </p>
            </div>
          </div>
        </div>

        <div className="pt-8 border-t border-slate-800/80 flex flex-col sm:flex-row items-center justify-between text-[11px] text-slate-500 gap-2">
          <p>© {new Date().getFullYear()} IPOForge. Designed & Created with <Heart className="w-3 h-3 inline text-rose-500 fill-rose-500 mx-0.5" /> by <strong className="text-slate-300">Shatrughna Ambhore</strong>.</p>
          <div className="flex items-center space-x-4 font-mono text-[10px]">
            <a href="mailto:ambhoreshatrughna@gmail.com" className="hover:text-emerald-400 transition">ambhoreshatrughna@gmail.com</a>
            <span>•</span>
            <a href="tel:+919604466334" className="hover:text-emerald-400 transition">+91 9604466334</a>
          </div>
        </div>
      </div>
    </footer>
  );
};
