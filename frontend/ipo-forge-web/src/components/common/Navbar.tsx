import React, { useState, useEffect, useRef } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { Flame, Search, Bookmark, ShieldCheck, User, LogOut, Menu, X, Database, Layers } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { useWatchlist } from '../../context/WatchlistContext';
import { api } from '../../services/api';
import { IpoSearchDto } from '../../types';
import { AuthModal } from './AuthModal';

export const Navbar: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, isAuthenticated, logout } = useAuth();
  const { watchlist } = useWatchlist();

  const [searchQuery, setSearchQuery] = useState('');
  const [searchResults, setSearchResults] = useState<IpoSearchDto[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [showSearchDropdown, setShowSearchDropdown] = useState(false);
  const [showAuthModal, setShowAuthModal] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const searchRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const timer = setTimeout(async () => {
      if (searchQuery.trim().length >= 2) {
        setIsSearching(true);
        try {
          const results = await api.searchIpos(searchQuery.trim());
          setSearchResults(results);
          setShowSearchDropdown(true);
        } catch {
          setSearchResults([]);
        } finally {
          setIsSearching(false);
        }
      } else {
        setSearchResults([]);
        setShowSearchDropdown(false);
      }
    }, 250);

    return () => clearTimeout(timer);
  }, [searchQuery]);

  // Close dropdown on outside click
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (searchRef.current && !searchRef.current.contains(event.target as Node)) {
        setShowSearchDropdown(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const navLinks = [
    { name: 'Dashboard', path: '/' },
    { name: 'IPO Explorer', path: '/ipos' },
    { name: 'GMP Tracker', path: '/gmp' },
    { name: 'Companies', path: '/companies' },
  ];

  return (
    <>
      <header className="sticky top-0 z-40 bg-navy-950/90 backdrop-blur-md border-b border-slate-800">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            {/* Logo */}
            <div className="flex items-center space-x-8">
              <Link to="/" className="flex items-center space-x-2.5 group">
                <div className="w-9 h-9 rounded-xl bg-gradient-to-tr from-emerald-600 to-teal-400 p-0.5 flex items-center justify-center shadow-lg shadow-emerald-950/40">
                  <div className="w-full h-full bg-slate-950 rounded-[10px] flex items-center justify-center group-hover:bg-slate-900 transition">
                    <Flame className="w-5 h-5 text-emerald-400 animate-pulse" />
                  </div>
                </div>
                <div>
                  <div className="flex items-center space-x-1.5">
                    <span className="font-extrabold text-lg tracking-tight text-white">IPO<span className="text-emerald-400">Forge</span></span>
                    <span className="text-[10px] uppercase font-bold tracking-widest px-1.5 py-0.2 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">IND</span>
                  </div>
                  <span className="text-[10px] text-slate-400 hidden sm:block -mt-1 font-medium">Research • Analyze • Decide</span>
                </div>
              </Link>

              {/* Desktop Nav Links */}
              <nav className="hidden md:flex items-center space-x-1">
                {navLinks.map((link) => {
                  const isActive = location.pathname === link.path;
                  return (
                    <Link
                      key={link.path}
                      to={link.path}
                      className={`px-3.5 py-1.5 rounded-lg text-xs font-semibold transition ${
                        isActive
                          ? 'bg-slate-800/80 text-emerald-400 shadow-inner'
                          : 'text-slate-300 hover:text-white hover:bg-slate-900'
                      }`}
                    >
                      {link.name}
                    </Link>
                  );
                })}
              </nav>
            </div>

            {/* Global Search Bar */}
            <div className="relative flex-1 max-w-xs mx-4 hidden sm:block" ref={searchRef}>
              <div className="relative">
                <Search className="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
                <input
                  type="text"
                  placeholder="Search IPO, company, sector..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onFocus={() => {
                    if (searchResults.length > 0) setShowSearchDropdown(true);
                  }}
                  className="w-full pl-9 pr-4 py-1.5 bg-slate-900/90 border border-slate-800 rounded-lg text-xs text-slate-100 placeholder-slate-500 focus:outline-none focus:border-emerald-500/60 focus:ring-1 focus:ring-emerald-500/30 transition"
                />
              </div>

              {/* Search Dropdown */}
              {showSearchDropdown && (
                <div className="absolute top-full left-0 right-0 mt-2 bg-slate-900 border border-slate-700/80 rounded-xl shadow-2xl overflow-hidden z-50 animate-fadeIn">
                  {isSearching ? (
                    <div className="p-4 text-center text-xs text-slate-400">Searching Indian IPOs...</div>
                  ) : searchResults.length > 0 ? (
                    <div className="divide-y divide-slate-800 max-h-80 overflow-y-auto">
                      {searchResults.map((res) => (
                        <div
                          key={res.id}
                          onClick={() => {
                            navigate(`/ipos/${res.id}`);
                            setShowSearchDropdown(false);
                            setSearchQuery('');
                          }}
                          className="p-3 hover:bg-slate-800/60 cursor-pointer flex items-center justify-between transition"
                        >
                          <div>
                            <div className="flex items-center space-x-2">
                              <span className="text-xs font-bold text-white">{res.name}</span>
                              <span className="text-[10px] px-1.5 py-0.2 rounded bg-slate-800 text-slate-400">{res.ipoType}</span>
                            </div>
                            <span className="text-[11px] text-slate-400">{res.sector}</span>
                          </div>
                          <div className="text-right">
                            {res.currentGmp !== undefined && (
                              <span className="text-xs font-mono font-semibold text-emerald-400 block">
                                +₹{res.currentGmp} ({res.currentGmpPercentage}%)
                              </span>
                            )}
                            <span className="text-[10px] font-mono text-slate-400">
                              Score: {res.listingGainScore}/100
                            </span>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div className="p-4 text-center text-xs text-slate-500">No matching IPOs found</div>
                  )}
                </div>
              )}
            </div>

            {/* Right Action Items */}
            <div className="flex items-center space-x-3">
              {/* Watchlist Quick Button */}
              <Link
                to="/watchlist"
                className="relative p-2 rounded-lg bg-slate-900/80 border border-slate-800 text-slate-300 hover:text-white hover:border-slate-700 transition flex items-center space-x-1.5"
                title="Watchlist"
              >
                <Bookmark className="w-4 h-4 text-emerald-400" />
                <span className="text-xs font-semibold hidden md:inline">Watchlist</span>
                {watchlist.length > 0 && (
                  <span className="w-4 h-4 rounded-full bg-emerald-500 text-[10px] font-mono font-bold text-slate-950 flex items-center justify-center">
                    {watchlist.length}
                  </span>
                )}
              </Link>

              {/* Admin Link */}
              <Link
                to="/admin"
                className="p-2 rounded-lg bg-slate-900/80 border border-slate-800 text-slate-400 hover:text-white hover:border-slate-700 transition"
                title="Data Synchronization Center"
              >
                <Database className="w-4 h-4" />
              </Link>

              {/* Auth Button */}
              {isAuthenticated ? (
                <div className="flex items-center space-x-2 bg-slate-900 border border-slate-800 px-3 py-1.5 rounded-lg">
                  <User className="w-4 h-4 text-emerald-400" />
                  <span className="text-xs font-medium text-slate-200 hidden sm:inline">{user?.fullName || user?.email}</span>
                  <button
                    onClick={logout}
                    className="text-slate-400 hover:text-rose-400 transition ml-1"
                    title="Logout"
                  >
                    <LogOut className="w-3.5 h-3.5" />
                  </button>
                </div>
              ) : (
                <button
                  onClick={() => setShowAuthModal(true)}
                  className="px-3.5 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-slate-950 font-bold text-xs shadow-md shadow-emerald-950/50 transition"
                >
                  Sign In
                </button>
              )}

              {/* Mobile Menu Button */}
              <button
                onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
                className="md:hidden p-2 rounded-lg bg-slate-900 border border-slate-800 text-slate-400 hover:text-white"
              >
                {isMobileMenuOpen ? <X className="w-5 h-5" /> : <Menu className="w-5 h-5" />}
              </button>
            </div>
          </div>
        </div>

        {/* Mobile Nav Drawer */}
        {isMobileMenuOpen && (
          <div className="md:hidden px-4 pt-2 pb-4 bg-navy-950 border-b border-slate-800 space-y-2 animate-fadeIn">
            {navLinks.map((link) => (
              <Link
                key={link.path}
                to={link.path}
                onClick={() => setIsMobileMenuOpen(false)}
                className="block px-3 py-2 rounded-lg text-sm font-semibold text-slate-300 hover:bg-slate-800 hover:text-white"
              >
                {link.name}
              </Link>
            ))}
            <Link
              to="/watchlist"
              onClick={() => setIsMobileMenuOpen(false)}
              className="block px-3 py-2 rounded-lg text-sm font-semibold text-slate-300 hover:bg-slate-800 hover:text-white"
            >
              Watchlist ({watchlist.length})
            </Link>
            <Link
              to="/admin"
              onClick={() => setIsMobileMenuOpen(false)}
              className="block px-3 py-2 rounded-lg text-sm font-semibold text-slate-300 hover:bg-slate-800 hover:text-white"
            >
              Data Sync & Logs
            </Link>
          </div>
        )}
      </header>

      {/* Auth Dialog */}
      <AuthModal isOpen={showAuthModal} onClose={() => setShowAuthModal(false)} />
    </>
  );
};
