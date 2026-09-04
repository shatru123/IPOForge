import { IpoSummary } from '../types';

export interface ComparisonDecision {
  winnerId: string;
  winnerName: string;
  reason: string;
  hasActiveCandidates: boolean;
  rankings: {
    ipo: IpoSummary;
    rank: number;
    badge: string;
    verdict: string;
    applyAdvice: 'Strong Apply' | 'Apply for Listing Gains' | 'Long-Term Only' | 'Neutral' | 'Avoid' | 'Closed' | 'Listed';
    color: string;
    isApplyable: boolean;
  }[];
}

export function evaluateComparison(ipos: IpoSummary[]): ComparisonDecision {
  if (!ipos || ipos.length === 0) {
    return {
      winnerId: '',
      winnerName: 'N/A',
      reason: 'No IPOs selected',
      hasActiveCandidates: false,
      rankings: []
    };
  }

  // Calculate composite decision metric
  const evaluated = ipos.map((ipo) => {
    const listScore = ipo.listingGainScore ?? 50;
    const ltScore = ipo.longTermScore ?? 50;
    const gmpPct = ipo.latestGmpPercentage ?? 0;

    const todayStr = new Date().toISOString().split('T')[0];
    const isCloseDateTodayOrPast = ipo.closeDate ? ipo.closeDate.split('T')[0] <= todayStr : false;

    const isListed = ipo.status === 'Listed';
    const isClosed = ipo.status === 'Closed' || ipo.status === 'AllotmentOut' || isCloseDateTodayOrPast;
    const isOpen = ipo.status === 'Open' && !isCloseDateTodayOrPast;
    const isUpcoming = ipo.status === 'Upcoming' && !isCloseDateTodayOrPast;

    const isApplyable = !isListed && !isClosed && (isOpen || isUpcoming);

    let applyAdvice: 'Strong Apply' | 'Apply for Listing Gains' | 'Long-Term Only' | 'Neutral' | 'Avoid' | 'Closed' | 'Listed';
    let badge = 'Neutral';
    let color = '#94A3B8';
    let verdict = '';
    let composite = 0;

    if (isListed) {
      applyAdvice = 'Listed';
      badge = '⚪ Already Listed';
      color = '#64748B';
      const listGain = ipo.actualListingGainPercent ?? ipo.listingGainPercent ?? gmpPct;
      verdict = `Listed on ${formatShortDate(ipo.listingDate || ipo.closeDate)} (Listing Day Return: +${listGain}%). Trading in secondary market.`;
      composite = -100; // Not candidate for "Best to Apply"
    } else if (isClosed) {
      applyAdvice = 'Closed';
      badge = '🔴 Bidding Closed';
      color = '#F87171';
      verdict = `Bidding closed on ${formatShortDate(ipo.closeDate)}. Expected listing on ${formatShortDate(ipo.listingDate)}. Check allotment status on registrar.`;
      composite = -50; // Not candidate for "Best to Apply"
    } else {
      // Active / Upcoming candidates
      composite = listScore * 0.55 + ltScore * 0.35 + Math.min(30, gmpPct * 0.4);
      if (isOpen) composite += 8; // Bonus for actively taking applications today

      if (listScore >= 75 && gmpPct >= 20) {
        applyAdvice = 'Strong Apply';
        badge = isOpen ? '🟢 Open: Strong Apply' : '🔵 Upcoming: Strong Pre-Apply';
        color = '#10B981';
        verdict = `High Listing Gain expected (+${gmpPct.toFixed(1)}% GMP) with strong institutional backing.`;
      } else if (listScore >= 65 || gmpPct >= 10) {
        applyAdvice = 'Apply for Listing Gains';
        badge = isOpen ? '🟢 Open: Listing Gain Play' : '🔵 Upcoming: Listing Gain Play';
        color = '#34D399';
        verdict = `Favorable listing upside (+${gmpPct.toFixed(1)}% GMP) for short-term allotment gains.`;
      } else if (ltScore >= 75 && listScore < 60) {
        applyAdvice = 'Long-Term Only';
        badge = '🔵 Long-Term Fundamental';
        color = '#60A5FA';
        verdict = `High business quality & ROE, but listing day premium is currently subdued.`;
      } else if (listScore >= 48) {
        applyAdvice = 'Neutral';
        badge = '🟡 Moderate / Neutral';
        color = '#FBBF24';
        verdict = `Moderate risk-reward. Monitor day-2 QIB subscription velocity before bidding.`;
      } else {
        applyAdvice = 'Avoid';
        badge = '🔴 Avoid / High Risk';
        color = '#F87171';
        verdict = `Weak grey market demand or rich valuation multiples. Low margin of safety.`;
      }
    }

    return {
      ipo,
      composite,
      badge,
      verdict,
      applyAdvice,
      color,
      isApplyable
    };
  });

  // Sort applyable candidates first by composite score, then closed/listed
  evaluated.sort((a, b) => b.composite - a.composite);

  const rankings = evaluated.map((item, index) => ({
    ipo: item.ipo,
    rank: index + 1,
    badge: index === 0 && item.isApplyable ? `🏆 Top Pick (${item.badge})` : item.badge,
    verdict: item.verdict,
    applyAdvice: item.applyAdvice,
    color: item.color,
    isApplyable: item.isApplyable
  }));

  const applyableWinners = rankings.filter((r) => r.isApplyable);
  const hasActiveCandidates = applyableWinners.length > 0;
  const winner = hasActiveCandidates ? applyableWinners[0] : rankings[0];

  return {
    winnerId: hasActiveCandidates ? winner.ipo.id : '',
    winnerName: hasActiveCandidates ? winner.ipo.name : 'No Active IPO Open to Apply',
    reason: hasActiveCandidates
      ? `${winner.ipo.name} is currently ${winner.ipo.status.toUpperCase()} with highest Listing Gain score (${winner.ipo.listingGainScore}/100) and +${(winner.ipo.latestGmpPercentage || 0).toFixed(1)}% live GMP.`
      : 'All selected IPOs have ended bidding or are already listed. Select open or upcoming IPOs to evaluate.',
    hasActiveCandidates,
    rankings
  };
}

export async function generateComparisonImage(ipos: IpoSummary[]): Promise<string> {
  const decision = evaluateComparison(ipos);
  const count = ipos.length;

  const width = Math.max(1000, count * 340 + 80);
  const height = 800;

  const canvas = document.createElement('canvas');
  const dpr = 2; // High-DPI Retina
  canvas.width = width * dpr;
  canvas.height = height * dpr;
  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('Canvas 2D context not supported');

  ctx.scale(dpr, dpr);

  // 1. Background Gradient
  const bgGradient = ctx.createLinearGradient(0, 0, width, height);
  bgGradient.addColorStop(0, '#090D16');
  bgGradient.addColorStop(0.5, '#0F172A');
  bgGradient.addColorStop(1, '#020617');
  ctx.fillStyle = bgGradient;
  ctx.fillRect(0, 0, width, height);

  // Subtle ambient glow
  const glow = ctx.createRadialGradient(width / 2, 80, 20, width / 2, 80, 400);
  glow.addColorStop(0, 'rgba(16, 185, 129, 0.15)');
  glow.addColorStop(1, 'rgba(16, 185, 129, 0)');
  ctx.fillStyle = glow;
  ctx.fillRect(0, 0, width, height);

  // Outer Border
  ctx.strokeStyle = '#1E293B';
  ctx.lineWidth = 2;
  ctx.strokeRect(10, 10, width - 20, height - 20);

  // 2. Header Branding
  ctx.fillStyle = '#10B981';
  ctx.font = 'bold 12px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif';
  ctx.fillText('⚡ IPOFORGE INTELLIGENCE — REAL-TIME IPO DECISION ENGINE', 40, 48);

  ctx.fillStyle = '#FFFFFF';
  ctx.font = 'bold 24px -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif';
  ctx.fillText('IPO Comparison: Which is Better to Apply?', 40, 80);

  // Winner Callout Box
  if (decision.hasActiveCandidates) {
    const boxX = width - 420;
    const boxY = 32;
    const boxW = 380;
    const boxH = 68;

    ctx.fillStyle = 'rgba(16, 185, 129, 0.12)';
    ctx.strokeStyle = 'rgba(16, 185, 129, 0.4)';
    ctx.lineWidth = 1.5;
    roundRect(ctx, boxX, boxY, boxW, boxH, 10);
    ctx.fill();
    ctx.stroke();

    ctx.fillStyle = '#F59E0B';
    ctx.font = 'bold 12px sans-serif';
    ctx.fillText(`🏆 #1 TOP PICK: ${decision.winnerName}`, boxX + 14, boxY + 24);

    ctx.fillStyle = '#CBD5E1';
    ctx.font = '11px sans-serif';
    ctx.fillText(decision.reason.length > 55 ? decision.reason.slice(0, 52) + '...' : decision.reason, boxX + 14, boxY + 46);
  }

  // Divider
  ctx.strokeStyle = '#334155';
  ctx.lineWidth = 1;
  ctx.beginPath();
  ctx.moveTo(40, 115);
  ctx.lineTo(width - 40, 115);
  ctx.stroke();

  // 3. Render Columns for Each IPO
  const cardWidth = (width - 80 - (count - 1) * 20) / count;
  const startY = 135;

  decision.rankings.forEach((ranked, idx) => {
    const ipo = ranked.ipo;
    const cardX = 40 + idx * (cardWidth + 20);
    const cardH = 560;

    const isWinner = ranked.rank === 1 && ranked.isApplyable;

    // Card background
    ctx.fillStyle = isWinner ? 'rgba(15, 23, 42, 0.95)' : 'rgba(15, 23, 42, 0.75)';
    ctx.strokeStyle = isWinner ? '#10B981' : ranked.isApplyable ? '#334155' : '#475569';
    ctx.lineWidth = isWinner ? 2 : 1;
    roundRect(ctx, cardX, startY, cardWidth, cardH, 14);
    ctx.fill();
    ctx.stroke();

    // Top Badge (Rank & Recommendation)
    ctx.fillStyle = ranked.color;
    ctx.font = 'bold 12px sans-serif';
    ctx.fillText(`${ranked.badge}`, cardX + 16, startY + 28);

    // IPO Name
    ctx.fillStyle = '#FFFFFF';
    ctx.font = 'bold 16px sans-serif';
    const truncatedName = ipo.name.length > 24 ? ipo.name.slice(0, 22) + '...' : ipo.name;
    ctx.fillText(truncatedName, cardX + 16, startY + 54);

    // Sector & Lifecycle Status
    ctx.fillStyle = '#94A3B8';
    ctx.font = '11px sans-serif';
    ctx.fillText(`${ipo.sector || 'Mainboard'} • ${ipo.status}`, cardX + 16, startY + 74);

    // Scores Row
    const scoreBoxY = startY + 90;
    ctx.fillStyle = 'rgba(2, 6, 23, 0.6)';
    roundRect(ctx, cardX + 14, scoreBoxY, cardWidth - 28, 64, 8);
    ctx.fill();

    // Listing Gain Score
    ctx.fillStyle = '#94A3B8';
    ctx.font = '10px sans-serif';
    ctx.fillText('LISTING GAIN', cardX + 26, scoreBoxY + 22);
    ctx.fillStyle = '#10B981';
    ctx.font = 'bold 20px sans-serif';
    ctx.fillText(`${ipo.listingGainScore || '-'}`, cardX + 26, scoreBoxY + 48);

    // Long Term Score
    ctx.fillStyle = '#94A3B8';
    ctx.font = '10px sans-serif';
    ctx.fillText('LONG-TERM', cardX + cardWidth / 2 + 10, scoreBoxY + 22);
    ctx.fillStyle = '#60A5FA';
    ctx.font = 'bold 20px sans-serif';
    ctx.fillText(`${ipo.longTermScore || '-'}`, cardX + cardWidth / 2 + 10, scoreBoxY + 48);

    // Metrics Table
    const metricsStartY = scoreBoxY + 80;
    const isClosedOrListed = ipo.status === 'Closed' || ipo.status === 'AllotmentOut' || ipo.status === 'Listed';

    const metrics = [
      { label: 'Live GMP', val: `+₹${ipo.latestGmp || 0} (+${(ipo.latestGmpPercentage || 0).toFixed(1)}%)`, valColor: '#10B981' },
      { label: 'Est. Listing Price', val: `₹${ipo.estimatedListingPrice || ipo.priceBandHigh || '-'}`, valColor: '#FFFFFF' },
      { label: 'Price Band', val: `₹${ipo.priceBandLow || 0} - ₹${ipo.priceBandHigh || 0}`, valColor: '#E2E8F0' },
      { label: 'Lot Size / Min Inv', val: `${ipo.lotSize || '-'} shs (₹${(ipo.minimumInvestment || 0).toLocaleString('en-IN')})`, valColor: '#E2E8F0' },
      { label: 'Issue Size', val: `₹${ipo.issueSize ? ipo.issueSize.toLocaleString('en-IN') + ' Cr' : 'TBD'}`, valColor: '#E2E8F0' },
      { label: 'Subscription', val: ipo.status === 'Upcoming' ? 'Bidding Soon' : `${ipo.totalSubscription || '-'}x`, valColor: '#34D399' },
      { label: 'Bidding Dates', val: `${formatShortDate(ipo.openDate)} – ${formatShortDate(ipo.closeDate)}`, valColor: '#CBD5E1' },
      { label: isClosedOrListed ? 'Listing / Listed On' : 'Expected Listing', val: formatShortDate(ipo.listingDate || ipo.closeDate), valColor: '#CBD5E1' }
    ];

    metrics.forEach((m, mIdx) => {
      const rowY = metricsStartY + mIdx * 30;
      ctx.fillStyle = '#94A3B8';
      ctx.font = '11px sans-serif';
      ctx.fillText(m.label, cardX + 16, rowY);

      ctx.fillStyle = m.valColor;
      ctx.font = 'bold 11px sans-serif';
      const textWidth = ctx.measureText(m.val).width;
      ctx.fillText(m.val, cardX + cardWidth - 16 - textWidth, rowY);

      if (mIdx < metrics.length - 1) {
        ctx.strokeStyle = 'rgba(51, 65, 85, 0.4)';
        ctx.beginPath();
        ctx.moveTo(cardX + 16, rowY + 6);
        ctx.lineTo(cardX + cardWidth - 16, rowY + 6);
        ctx.stroke();
      }
    });

    // Verdict Footer Box inside Card
    const verdictY = startY + cardH - 68;
    ctx.fillStyle = isWinner ? 'rgba(16, 185, 129, 0.15)' : 'rgba(30, 41, 59, 0.6)';
    roundRect(ctx, cardX + 12, verdictY, cardWidth - 24, 56, 6);
    ctx.fill();

    ctx.fillStyle = isWinner ? '#34D399' : '#E2E8F0';
    ctx.font = '10px sans-serif';
    wrapText(ctx, ranked.verdict, cardX + 18, verdictY + 16, cardWidth - 36, 13);
  });

  // 4. Footer Branding & Author Contact
  const footerY = height - 25;
  ctx.fillStyle = '#64748B';
  ctx.font = '10.5px sans-serif';
  ctx.fillText('IPOForge • Research. Analyze. Decide. • https://github.com/shatru123/IPOForge', 40, footerY);

  ctx.fillStyle = '#94A3B8';
  ctx.font = 'bold 10.5px sans-serif';
  const authorText = 'Created by Shatrughna Ambhore (ambhoreshatrughna@gmail.com | +91 9604466334)';
  const authorWidth = ctx.measureText(authorText).width;
  ctx.fillText(authorText, width - 40 - authorWidth, footerY);

  return canvas.toDataURL('image/png');
}

export function generateWhatsAppShareText(ipos: IpoSummary[], activeOnly: boolean = false): string {
  const todayStr = new Date().toISOString().split('T')[0];
  const targetIpos = activeOnly
    ? ipos.filter((i) => {
        const isClosedOrClosingToday = i.closeDate ? i.closeDate.split('T')[0] <= todayStr : false;
        return (i.status === 'Open' || i.status === 'Upcoming') && !isClosedOrClosingToday;
      })
    : ipos;

  const decision = evaluateComparison(targetIpos.length > 0 ? targetIpos : ipos);
  const dateStr = new Date().toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' });

  let text = `🚀 *IPOForge Intelligence — Real-Time Indian IPO Comparison* 📊\n`;
  text += `📅 *Date:* ${dateStr}\n\n`;

  if (decision.hasActiveCandidates) {
    text += `🏆 *#1 TOP PICK TO APPLY: ${decision.winnerName}*\n`;
    text += `💡 *Verdict:* ${decision.reason}\n\n`;
  }

  text += `━━━━━━━━━━━━━━━━━━━━━\n`;
  text += `*IPO BREAKDOWN & DATES:*\n\n`;

  decision.rankings.forEach((r) => {
    const ipo = r.ipo;
    const gmpVal = ipo.latestGmp || 0;
    const gmpPct = ipo.latestGmpPercentage ? ipo.latestGmpPercentage.toFixed(1) : '0.0';
    const price = ipo.priceBandHigh ? `₹${ipo.priceBandLow || ipo.priceBandHigh} - ₹${ipo.priceBandHigh}` : 'TBD';
    const estListing = ipo.estimatedListingPrice ? `₹${ipo.estimatedListingPrice}` : 'TBD';
    const dates = `${formatShortDate(ipo.openDate)} to ${formatShortDate(ipo.closeDate)}`;
    const sub = ipo.status === 'Upcoming' ? 'Bidding Soon' : `${ipo.totalSubscription || '-'}x`;
    const isClosedOrClosingToday = ipo.closeDate ? ipo.closeDate.split('T')[0] <= todayStr : false;

    const statusBadge =
      ipo.status === 'Listed'
        ? `⚪ LISTED (Listed on ${formatShortDate(ipo.listingDate || ipo.closeDate)})`
        : ipo.status === 'Closed' || ipo.status === 'AllotmentOut' || isClosedOrClosingToday
        ? `🔴 CLOSED (Bidding ended on ${formatShortDate(ipo.closeDate)} • Listing on ${formatShortDate(ipo.listingDate)})`
        : ipo.status === 'Open'
        ? `🟢 OPEN NOW (Closes on ${formatShortDate(ipo.closeDate)})`
        : `🔵 UPCOMING (Opens on ${formatShortDate(ipo.openDate)})`;

    if (ipo.status === 'Listed') {
      const listGain = ipo.actualListingGainPercent ?? ipo.listingGainPercent ?? gmpPct;
      const listPrice = ipo.actualListingPrice ?? ipo.listingPrice;
      const listGainPerLot = ipo.actualListingGainPerLot ?? (ipo.priceBandHigh && ipo.listingPrice && ipo.lotSize ? (ipo.listingPrice - ipo.priceBandHigh) * ipo.lotSize : 0);
      text += `📌 *${r.rank}. ${ipo.name}*\n`;
      text += `• *Status:* ${statusBadge}\n`;
      text += `• *Issue Price:* ${price} | *Debut Price:* ₹${listPrice || '-'}\n`;
      text += `• *Exact Listing Gain:* +${listGain}% (+₹${listGainPerLot.toLocaleString('en-IN')}/lot)\n`;
      text += `• *Listing Score:* ${ipo.listingGainScore || '-'}/100 | Long-Term: ${ipo.longTermScore || '-'}/100\n`;
      text += `• *Subscription:* ${sub}\n`;
      text += `• *Decision:* _${r.verdict}_\n\n`;
    } else {
      const estProfit = ipo.estimatedProfitPerLot ?? (gmpVal && ipo.lotSize ? gmpVal * ipo.lotSize : 0);
      text += `📌 *${r.rank}. ${ipo.name}*\n`;
      text += `• *Status:* ${statusBadge}\n`;
      text += `• *Bidding Window:* ${dates}\n`;
      text += `• *Price Band:* ${price} | Lot: ${ipo.lotSize || '-'} shs (Min: ₹${(ipo.minimumInvestment || 0).toLocaleString('en-IN')})\n`;
      text += `• *Current GMP:* +₹${gmpVal} (+${gmpPct}%)\n`;
      text += `• *Est. Profit/Loss (per Lot):* ${estProfit >= 0 ? '+' : ''}₹${estProfit.toLocaleString('en-IN')}\n`;
      text += `• *Est. Listing Price:* ${estListing}\n`;
      text += `• *Listing Score:* ${ipo.listingGainScore || '-'}/100 | Long-Term: ${ipo.longTermScore || '-'}/100\n`;
      text += `• *Subscription:* ${sub}\n`;
      text += `• *Decision:* _${r.verdict}_\n\n`;
    }
  });

  text += `━━━━━━━━━━━━━━━━━━━━━\n`;
  text += `🔍 *View Complete Visual 30-Second Analysis:*\n`;
  text += `👉 ${window.location.origin}\n\n`;
  text += `_IPOForge — Research. Analyze. Decide._\n`;
  text += `_Created by Shatrughna Ambhore (ambhoreshatrughna@gmail.com)_`;

  return text;
}

export function shareToWhatsApp(ipos: IpoSummary[], activeOnly: boolean = false): void {
  const text = generateWhatsAppShareText(ipos, activeOnly);
  const url = `https://api.whatsapp.com/send?text=${encodeURIComponent(text)}`;
  window.open(url, '_blank');
}

export function exportComparisonCsv(ipos: IpoSummary[]): void {
  const decision = evaluateComparison(ipos);
  const headers = [
    'Rank',
    'Recommendation',
    'IPO Name',
    'Symbol',
    'Type',
    'Status',
    'Sector',
    'Price Band Low (INR)',
    'Price Band High (INR)',
    'Lot Size',
    'Min Investment (INR)',
    'Issue Size (INR Cr)',
    'Live GMP (INR)',
    'Live GMP (%)',
    'Estimated Listing Price (INR)',
    'Listing Gain Score',
    'Long-Term Score',
    'Subscription (x)',
    'Open Date',
    'Close Date',
    'Allotment Date',
    'Listing Date',
    'Analytical Verdict'
  ];

  const rows = decision.rankings.map((r) => {
    const ipo = r.ipo;
    return [
      r.rank,
      `"${r.applyAdvice}"`,
      `"${ipo.name}"`,
      `"${ipo.symbol || ''}"`,
      ipo.ipoType,
      ipo.status,
      `"${ipo.sector || ''}"`,
      ipo.priceBandLow || 0,
      ipo.priceBandHigh || 0,
      ipo.lotSize || 0,
      ipo.minimumInvestment || 0,
      ipo.issueSize || 0,
      ipo.latestGmp || 0,
      ipo.latestGmpPercentage ? ipo.latestGmpPercentage.toFixed(2) : 0,
      ipo.estimatedListingPrice || 0,
      ipo.listingGainScore || 0,
      ipo.longTermScore || 0,
      ipo.totalSubscription || 0,
      ipo.openDate ? new Date(ipo.openDate).toISOString().split('T')[0] : '',
      ipo.closeDate ? new Date(ipo.closeDate).toISOString().split('T')[0] : '',
      ipo.allotmentDate ? new Date(ipo.allotmentDate).toISOString().split('T')[0] : '',
      ipo.listingDate ? new Date(ipo.listingDate).toISOString().split('T')[0] : '',
      `"${r.verdict.replace(/"/g, '""')}"`
    ].join(',');
  });

  const csvContent = 'data:text/csv;charset=utf-8,' + [headers.join(','), ...rows].join('\n');
  const encodedUri = encodeURI(csvContent);
  const link = document.createElement('a');
  link.setAttribute('href', encodedUri);
  link.setAttribute('download', `IPOForge_Comparison_${new Date().toISOString().split('T')[0]}.csv`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
}

function formatShortDate(dateStr?: string): string {
  if (!dateStr) return 'TBD';
  return new Date(dateStr).toLocaleDateString('en-IN', { day: 'numeric', month: 'short' });
}

function roundRect(
  ctx: CanvasRenderingContext2D,
  x: number,
  y: number,
  width: number,
  height: number,
  radius: number
): void {
  ctx.beginPath();
  ctx.moveTo(x + radius, y);
  ctx.lineTo(x + width - radius, y);
  ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
  ctx.lineTo(x + width, y + height - radius);
  ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
  ctx.lineTo(x + radius, y + height);
  ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
  ctx.lineTo(x, y + radius);
  ctx.quadraticCurveTo(x, y, x + radius, y);
  ctx.closePath();
}

function wrapText(
  ctx: CanvasRenderingContext2D,
  text: string,
  x: number,
  y: number,
  maxWidth: number,
  lineHeight: number
): void {
  const words = text.split(' ');
  let line = '';

  for (let n = 0; n < words.length; n++) {
    const testLine = line + words[n] + ' ';
    const metrics = ctx.measureText(testLine);
    const testWidth = metrics.width;
    if (testWidth > maxWidth && n > 0) {
      ctx.fillText(line, x, y);
      line = words[n] + ' ';
      y += lineHeight;
    } else {
      line = testLine;
    }
  }
  ctx.fillText(line, x, y);
}
