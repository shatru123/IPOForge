import React, { createContext, useContext, useEffect, useState } from 'react';
import { api } from '../services/api';
import { WatchlistItem } from '../types';
import { useAuth } from './AuthContext';

interface WatchlistContextType {
  watchlist: WatchlistItem[];
  isInWatchlist: (ipoId: string) => boolean;
  toggleWatchlist: (ipoId: string, name?: string) => Promise<void>;
  refreshWatchlist: () => Promise<void>;
  loading: boolean;
}

const WatchlistContext = createContext<WatchlistContextType | undefined>(undefined);

export const WatchlistProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useAuth();
  const [watchlist, setWatchlist] = useState<WatchlistItem[]>([]);
  const [loading, setLoading] = useState(false);

  const refreshWatchlist = async () => {
    if (isAuthenticated) {
      setLoading(true);
      try {
        const items = await api.getWatchlist();
        setWatchlist(items);
      } catch (err) {
        console.error('Failed to fetch watchlist', err);
      } finally {
        setLoading(false);
      }
    } else {
      // Local storage fallback for guests
      const local = localStorage.getItem('ipoforge_guest_watchlist');
      if (local) {
        try {
          setWatchlist(JSON.parse(local));
        } catch {
          setWatchlist([]);
        }
      }
    }
  };

  useEffect(() => {
    refreshWatchlist();
  }, [isAuthenticated]);

  const isInWatchlist = (ipoId: string) => watchlist.some((w) => w.ipoId === ipoId);

  const toggleWatchlist = async (ipoId: string, name?: string) => {
    if (isInWatchlist(ipoId)) {
      if (isAuthenticated) {
        await api.removeFromWatchlist(ipoId);
      }
      setWatchlist((prev) => {
        const updated = prev.filter((w) => w.ipoId !== ipoId);
        if (!isAuthenticated) {
          localStorage.setItem('ipoforge_guest_watchlist', JSON.stringify(updated));
        }
        return updated;
      });
    } else {
      if (isAuthenticated) {
        const item = await api.addToWatchlist(ipoId);
        setWatchlist((prev) => [...prev, item]);
      } else {
        const guestItem: WatchlistItem = {
          id: `guest_${ipoId}`,
          ipoId,
          ipoName: name || 'IPO',
          sector: 'General',
          ipoType: 'Mainboard',
          status: 'Open',
          listingGainScore: 80,
          longTermScore: 75,
          addedAt: new Date().toISOString(),
        };
        setWatchlist((prev) => {
          const updated = [...prev, guestItem];
          localStorage.setItem('ipoforge_guest_watchlist', JSON.stringify(updated));
          return updated;
        });
      }
    }
  };

  return (
    <WatchlistContext.Provider
      value={{
        watchlist,
        isInWatchlist,
        toggleWatchlist,
        refreshWatchlist,
        loading,
      }}
    >
      {children}
    </WatchlistContext.Provider>
  );
};

export const useWatchlist = () => {
  const context = useContext(WatchlistContext);
  if (!context) {
    throw new Error('useWatchlist must be used within a WatchlistProvider');
  }
  return context;
};
