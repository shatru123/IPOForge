import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { WatchlistProvider } from './context/WatchlistContext';
import { Navbar } from './components/common/Navbar';
import { Footer } from './components/common/Footer';
import { DashboardPage } from './pages/DashboardPage';
import { IpoExplorerPage } from './pages/IpoExplorerPage';
import { IpoDetailPage } from './pages/IpoDetailPage';
import { GmpTrackerPage } from './pages/GmpTrackerPage';
import { CompanyDetailPage } from './pages/CompanyDetailPage';
import { WatchlistPage } from './pages/WatchlistPage';
import { AdminPage } from './pages/AdminPage';

export const App: React.FC = () => {
  return (
    <AuthProvider>
      <WatchlistProvider>
        <Router>
          <div className="flex flex-col min-h-screen bg-navy-950 text-slate-100 font-sans selection:bg-emerald-500 selection:text-slate-950">
            <Navbar />
            <main className="flex-1">
              <Routes>
                <Route path="/" element={<DashboardPage />} />
                <Route path="/ipos" element={<IpoExplorerPage />} />
                <Route path="/ipos/:id" element={<IpoDetailPage />} />
                <Route path="/gmp" element={<GmpTrackerPage />} />
                <Route path="/companies" element={<CompanyDetailPage />} />
                <Route path="/companies/:id" element={<CompanyDetailPage />} />
                <Route path="/watchlist" element={<WatchlistPage />} />
                <Route path="/admin" element={<AdminPage />} />
              </Routes>
            </main>
            <Footer />
          </div>
        </Router>
      </WatchlistProvider>
    </AuthProvider>
  );
};

export default App;
