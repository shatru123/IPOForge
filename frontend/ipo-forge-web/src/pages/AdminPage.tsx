import React, { useEffect, useState } from 'react';
import { api } from '../services/api';
import { AdminSyncStatus } from '../types';
import { RefreshCw, Database, CheckCircle2, AlertTriangle, Clock, Server, Cloud, Shield } from 'lucide-react';

export const AdminPage: React.FC = () => {
  const [status, setStatus] = useState<AdminSyncStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [refreshMessage, setRefreshMessage] = useState<string | null>(null);

  const fetchStatus = async () => {
    try {
      const data = await api.getAdminStatus();
      setStatus(data);
    } catch (err) {
      console.error('Failed to load admin status', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchStatus();
  }, []);

  const handleTriggerRefresh = async () => {
    setRefreshing(true);
    setRefreshMessage(null);
    try {
      const res = await api.triggerDataRefresh(true);
      setRefreshMessage(res.message);
      await fetchStatus();
    } catch (err: any) {
      setRefreshMessage(`Error: ${err.message || 'Data refresh trigger failed'}`);
    } finally {
      setRefreshing(false);
    }
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center space-x-2 text-xs text-emerald-400 font-semibold mb-1">
            <Database className="w-4 h-4" />
            <span>Data Ingestion & Pipeline Orchestration</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-extrabold text-white tracking-tight">
            Data Synchronization Center
          </h1>
          <p className="text-xs sm:text-sm text-slate-400 mt-1">
            Manage real-time scraper connectors, trigger manual refreshes, and inspect ingestion logs.
          </p>
        </div>

        {/* Trigger Button */}
        <button
          onClick={handleTriggerRefresh}
          disabled={refreshing}
          className="px-5 py-2.5 rounded-xl bg-emerald-500 hover:bg-emerald-400 text-slate-950 font-bold text-xs flex items-center space-x-2 transition disabled:opacity-50 shadow-lg shadow-emerald-950/60 self-start sm:self-auto"
        >
          <RefreshCw className={`w-4 h-4 ${refreshing ? 'animate-spin' : ''}`} />
          <span>{refreshing ? 'Refreshing Pipeline...' : 'Trigger Full Data Refresh'}</span>
        </button>
      </div>

      {refreshMessage && (
        <div className={`p-4 rounded-2xl text-xs flex items-center space-x-3 ${
          refreshMessage.startsWith('Error')
            ? 'bg-rose-500/10 border border-rose-500/30 text-rose-300'
            : 'bg-emerald-500/10 border border-emerald-500/30 text-emerald-300'
        }`}>
          {refreshMessage.startsWith('Error') ? <AlertTriangle className="w-5 h-5" /> : <CheckCircle2 className="w-5 h-5" />}
          <span>{refreshMessage}</span>
        </div>
      )}

      {/* Free Tier Architecture Info Card */}
      <div className="bg-gradient-to-r from-slate-950 via-slate-900 to-navy-900 border border-slate-800 rounded-3xl p-6 shadow-xl space-y-4">
        <div className="flex items-center space-x-2">
          <Cloud className="w-5 h-5 text-emerald-400" />
          <h3 className="text-base font-bold text-white">Free-Tier Cloud Deployment Profile (Render Target)</h3>
        </div>
        <p className="text-xs text-slate-300 leading-relaxed max-w-3xl">
          IPOForge is designed to run 100% free with zero required paid API dependencies. Background ingestion is triggered on-demand via the protected admin HTTP endpoint, background worker service, or cron job.
        </p>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 text-xs font-mono pt-2">
          <div className="bg-slate-950/60 p-3 rounded-xl border border-slate-800">
            <span className="text-slate-500 text-[10px] block font-sans">Frontend Static Host</span>
            <span className="text-emerald-400 font-bold">Render Static Site / Vercel</span>
          </div>
          <div className="bg-slate-950/60 p-3 rounded-xl border border-slate-800">
            <span className="text-slate-500 text-[10px] block font-sans">Backend Web Service</span>
            <span className="text-blue-400 font-bold">ASP.NET Core .NET 10</span>
          </div>
          <div className="bg-slate-950/60 p-3 rounded-xl border border-slate-800">
            <span className="text-slate-500 text-[10px] block font-sans">Relational Database</span>
            <span className="text-purple-400 font-bold">PostgreSQL / In-Memory</span>
          </div>
        </div>
      </div>

      {/* Data Sources Health Status */}
      <section className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
        <h3 className="text-base font-bold text-white">Configured Exchange & Registrar Feeds</h3>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs text-slate-300">
            <thead className="bg-slate-950 text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-800">
              <tr>
                <th className="py-3 px-4">Source Feed</th>
                <th className="py-3 px-4">Provider Key</th>
                <th className="py-3 px-4">Type</th>
                <th className="py-3 px-4">Health Status</th>
                <th className="py-3 px-4 text-right">Last Sync</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-800/60 font-mono">
              {status?.dataSources?.map((ds) => (
                <tr key={ds.id} className="hover:bg-slate-800/40">
                  <td className="py-3 px-4 font-sans font-bold text-white">{ds.name}</td>
                  <td className="py-3 px-4 text-slate-400">{ds.providerKey}</td>
                  <td className="py-3 px-4 text-slate-300">{ds.sourceType}</td>
                  <td className="py-3 px-4">
                    <span className="inline-flex items-center space-x-1.5 px-2.5 py-0.5 rounded-full bg-emerald-500/10 text-emerald-400 border border-emerald-500/30 text-[11px] font-semibold">
                      <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse" />
                      <span>{ds.healthStatus}</span>
                    </span>
                  </td>
                  <td className="py-3 px-4 text-right text-slate-400">
                    {ds.lastSyncAt ? new Date(ds.lastSyncAt).toLocaleTimeString('en-IN') : 'Just now'}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      {/* Ingestion Execution History Logs */}
      <section className="bg-slate-900/80 border border-slate-800 rounded-2xl p-6 shadow-xl space-y-4">
        <h3 className="text-base font-bold text-white">Recent Synchronization & Calculation Logs</h3>
        {status?.recentLogs && status.recentLogs.length > 0 ? (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs text-slate-300 font-mono">
              <thead className="bg-slate-950 text-slate-400 uppercase text-[10px] tracking-wider border-b border-slate-800">
                <tr>
                  <th className="py-3 px-4">Timestamp</th>
                  <th className="py-3 px-4">Feed Source</th>
                  <th className="py-3 px-4">Execution Status</th>
                  <th className="py-3 px-4 text-right">Records Updated</th>
                  <th className="py-3 px-4 text-right">Duration (ms)</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/60">
                {status.recentLogs.map((log) => (
                  <tr key={log.id} className="hover:bg-slate-800/40">
                    <td className="py-3 px-4 text-slate-400">
                      {new Date(log.executedAt).toLocaleTimeString('en-IN')}
                    </td>
                    <td className="py-3 px-4 font-bold text-slate-200">{log.source}</td>
                    <td className="py-3 px-4">
                      <span className="text-emerald-400 font-bold">{log.status}</span>
                    </td>
                    <td className="py-3 px-4 text-right text-teal-300 font-bold">
                      {log.recordsUpdated}
                    </td>
                    <td className="py-3 px-4 text-right text-slate-400">{log.durationMs}ms</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="text-xs text-slate-500 py-4">No recent synchronization history logs yet.</p>
        )}
      </section>
    </div>
  );
};
