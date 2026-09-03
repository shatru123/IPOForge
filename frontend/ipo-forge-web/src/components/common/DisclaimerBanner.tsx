import React from 'react';
import { AlertCircle } from 'lucide-react';

export const DisclaimerBanner: React.FC = () => {
  return (
    <div className="bg-slate-900/80 border border-slate-800 rounded-xl p-4 flex items-start space-x-3 text-xs text-slate-400">
      <AlertCircle className="w-5 h-5 text-amber-400 flex-shrink-0 mt-0.5" />
      <div className="leading-relaxed">
        <span className="font-semibold text-slate-200 block mb-0.5">Statutory & Research Disclaimer:</span>
        IPOForge is an algorithmic research intelligence portal intended purely for educational and analytical purposes. Grey Market Premium (GMP) numbers are unofficial, volatile, and unregulated indicator estimates. Scoring models are quantitative heuristics and do not constitute SEBI-registered investment advice. Always review the official Red Herring Prospectus (RHP) and consult a certified financial advisor before investing.
      </div>
    </div>
  );
};
