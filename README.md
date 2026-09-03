# IPOForge — Indian IPO Intelligence Platform

> **Research. Analyze. Decide.**
> Understand any Mainboard or SME IPO in **30 seconds** with explainable quantitative scoring, real-time GMP tracking, financial benchmarking, and risk radars.

[![IPOForge CI Pipeline](https://github.com/shatru123/IPOForge/actions/workflows/ci.yml/badge.svg)](https://github.com/shatru123/IPOForge/actions/workflows/ci.yml)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![React Version](https://img.shields.io/badge/React-19-blue.svg)](https://react.dev/)
[![Database](https://img.shields.io/badge/Database-PostgreSQL-336791.svg)](https://www.postgresql.org/)
[![Deploy on Render](https://img.shields.io/badge/Deploy-Render%20Free%20Tier-46E3B7.svg)](https://render.com)
[![Author](https://img.shields.io/badge/Author-Shatrughna%20Ambhore-emerald.svg)](mailto:ambhoreshatrughna@gmail.com)

---

## 👨‍💻 Created By

* **Shatrughna Ambhore**
* 📧 **Email**: [ambhoreshatrughna@gmail.com](mailto:ambhoreshatrughna@gmail.com)
* 📞 **Phone**: [+91 9604466334](tel:+919604466334)
* 🌐 **GitHub**: [github.com/shatru123/IPOForge](https://github.com/shatru123/IPOForge)

---

## 🌟 Product Overview

**IPOForge** is a production-grade Indian IPO intelligence platform engineered for retail, HNI, and institutional investors seeking fast, objective, data-driven decisions on upcoming and active initial public offerings on the BSE and NSE.

### What Problem Does It Solve?
Reading a 400-page Red Herring Prospectus (RHP) takes hours. IPOForge parses financial statements, computes 3-year CAGRs, evaluates valuation multiples against industry benchmarks, tracks Grey Market Premium (GMP) momentum, analyzes subscription multiples across bidding tiers, and scores every issue through transparent, deterministic **100-point algorithmic engines**.

---

## 🚀 Key Features

* **⚡ 30-Second Decision Engine**:
  * Dual **Score Rings** with clear rating brackets: **Strong** (80–100 🟢), **Positive** (65–79 🟢), **Neutral** (50–64 🟡), **Weak** (35–49 🟠), **Avoid** (0–34 🔴).
  * 100% explainable score pillar breakdown modal revealing earned points, category weights, and mathematical rationale.
* **📈 Live GMP Tracking & Historical Accuracy Analytics**:
  * Interactive historical GMP area charts with 24-hour, 3-day, and 7-day momentum deltas.
  * Historical accuracy scatter plot backtesting pre-listing GMP predictions against final listing day price discovery.
* **📊 Subscription Bidding Multiples**:
  * Day 1, Day 2, and Day 3 bidding demand breakdown across QIB (Institutional), NII (HNI), and Retail categories.
* **🏢 Financial Multi-Year Deep-Dive**:
  * 3-Year CAGR for Revenue, EBITDA, and PAT.
  * Restated statements with EBITDA Margins, PAT Margins, ROE, ROCE, Debt/Equity, and Operating Cash Flow quality.
* **⚖️ Valuation Benchmarking**:
  * P/E and P/B multiples mapped against Industry Sector Medians.
  * Algorithmic classification: *Attractive*, *Reasonable*, *Premium*, *Very Expensive*.
* **💰 Fund Utilization & OFS Analysis**:
  * Interactive donut visualization of Fresh Issue (Growth Capital) vs Offer for Sale (OFS / PE Exit) and stated objective allocations.
* **⚠️ Automated Risk Radar**:
  * Severity-ranked risk detection (debt leverage, customer concentration, negative cash flows, legal liabilities).
* **⭐ Cloud & Local Watchlist**:
  * Sync saved IPOs with custom target listing price notes across devices.
* **☁️ 100% Free-Tier Architecture**:
  * Engineered for Render Free Tier (ASP.NET Core Web API + PostgreSQL + React Static Site), zero required paid API keys, resilient in-memory fallback.

---

## 🏛️ System Architecture

IPOForge follows the **Clean Architecture / Onion Architecture** principles with strict dependency inversion:

```
IPOForge/
├── src/
│   ├── IPOForge.Domain/          # Pure entities, value objects, domain enums, auditable bases
│   ├── IPOForge.Contracts/       # API requests, responses, DTOs, paged results
│   ├── IPOForge.Application/     # 100-pt Scoring Engine, Valuation, Risk, Financial, GMP engines
│   ├── IPOForge.Infrastructure/  # EF Core DbContext, PostgreSQL/InMemory, Identity, Public Scraper, Caching
│   ├── IPOForge.Api/             # ASP.NET Core controllers, Serilog, Swagger OpenAPI, Global Exceptions
│   └── IPOForge.Worker/          # Standalone background synchronization worker service
├── frontend/
│   └── ipo-forge-web/            # React 19, TypeScript, Vite, Tailwind CSS, Recharts, Lucide Icons
├── tests/
│   ├── IPOForge.UnitTests/       # 8 unit test suites (Valuation, GMP, Financials, Scoring, Risks)
│   └── IPOForge.IntegrationTests/# ASP.NET Core WebApplicationFactory integration tests with DB isolation
├── Dockerfile                    # Multi-stage production container build
├── docker-compose.yml            # Local orchestration (PostgreSQL + IPOForge Web API + UI)
├── render.yaml                   # Free-tier deployment blueprint for Render
└── .github/workflows/ci.yml      # Automated GitHub Actions CI pipeline
```

---

## 🧮 100-Point Deterministic Scoring Methodology

### 1. Listing Gain Engine (100 Points)
| Pillar | Weight | Description & Evaluation Logic |
| :--- | :---: | :--- |
| **GMP Momentum & Absolute Spread** | 25 pts | Ratio of GMP to Issue Price (>50% = 25 pts, >25% = 18 pts, >10% = 10 pts) |
| **Institutional QIB Subscription** | 20 pts | High-conviction institutional bidding (>20x = 20 pts, >10x = 15 pts, >5x = 10 pts) |
| **Total Subscription Demand** | 15 pts | Aggregate market oversubscription (>30x = 15 pts, >10x = 10 pts) |
| **Valuation Room & Margin of Safety** | 15 pts | P/E discount relative to listed sector peers (>20% discount = 15 pts) |
| **Issue Structure (Fresh vs OFS)** | 10 pts | Higher Fresh Issue percentage indicates growth capital retention (>70% Fresh = 10 pts) |
| **Retail Demand Multiple** | 5 pts | Retail investor bidding participation (>5x = 5 pts) |
| **Lead Manager Track Record** | 5 pts | Past listing day performance of book running lead managers |
| **Market Sentiment Trend** | 5 pts | Overall broader market index posture (Nifty/Sensex) |

### 2. Long-Term Investment Engine (100 Points)
| Pillar | Weight | Description & Evaluation Logic |
| :--- | :---: | :--- |
| **Revenue Growth Consistency (3Y CAGR)** | 15 pts | Compounded top-line expansion (>20% CAGR = 15 pts, >10% = 10 pts) |
| **Profitability & PAT Margin Quality** | 15 pts | Net profit margins and bottom-line stability (>15% margin = 15 pts) |
| **Return on Equity (ROE) & Capital Efficiency** | 12 pts | Sustained ROE (>20% = 12 pts, >15% = 8 pts) |
| **Debt & Balance Sheet Solvency** | 12 pts | Debt-to-Equity ratio (<0.5x = 12 pts, <1.0x = 8 pts, >2.0x = 0 pts) |
| **Valuation Sustainability** | 10 pts | Long-term justifiable earnings multiple vs peers |
| **Operating Cash Flow Generation** | 10 pts | Positive and growing OCF/EBITDA ratio |
| **Competitive Moats & Industry Outlook** | 8 pts | High barriers to entry and market leadership |
| **Promoter Stake & Corporate Governance** | 6 pts | Post-issue promoter skin-in-the-game (>50% holding) |
| **Capital Allocation (Fresh vs OFS)** | 4 pts | Primary capital used for CAPEX vs secondary exits |
| **Working Capital Cycle** | 4 pts | Efficient cash conversion cycle |
| **Regulatory & Litigation Risks** | 4 pts | Low legal and regulatory friction |

---

## 📡 API Reference

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `GET` | `/health` / `/healthz` | Live service & database health check with uptime & metrics |
| `GET` | `/health/ready` | Readiness probe for container orchestrators |
| `GET` | `/api/dashboard` | Aggregated market dashboard summary (Open, Upcoming, Listed, Avg GMP) |
| `GET` | `/api/ipos` | Filtered & paged IPO explorer (`status`, `ipoType`, `minScore`, `sortBy`) |
| `GET` | `/api/ipos/{id}` | Complete 30-second IPO intelligence report |
| `GET` | `/api/ipos/search?q={query}` | Instant auto-complete search across names, symbols, and sectors |
| `GET` | `/api/ipos/open` | List of currently open IPOs taking bids |
| `GET` | `/api/ipos/upcoming` | List of forthcoming pipeline IPOs |
| `GET` | `/api/ipos/listed` | List of recently listed historical issues |
| `GET` | `/api/gmp/top-movers` | Top GMP gainers of the week |
| `GET` | `/api/gmp/accuracy` | Historical GMP prediction accuracy analytics |
| `GET` | `/api/companies` | Corporate issuer directory with search |
| `GET` | `/api/companies/{id}` | Company overview, financials, and associated IPOs |
| `GET` | `/api/watchlist` | Get authenticated user's saved watchlist |
| `POST` | `/api/watchlist` | Add IPO to watchlist with optional target price |
| `DELETE`| `/api/watchlist/{ipoId}` | Remove IPO from watchlist |
| `POST` | `/api/auth/register` | Register new investor profile |
| `POST` | `/api/auth/login` | Authenticate and obtain JWT bearer token |
| `GET` | `/api/auth/me` | Retrieve active user profile |
| `POST` | `/api/admin/data-refresh` | Trigger on-demand scraper sync & score recalculations |
| `GET` | `/api/admin/status` | Ingestion source health and sync logs |

---

## 💻 Local Development Quickstart

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* [Node.js 20+](https://nodejs.org/) & npm
* [Docker Desktop](https://www.docker.com/) (Optional, for containerized run)

### Option 1: Run with .NET & Vite Locally

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/shatru123/IPOForge.git
   cd IPOForge
   ```

2. **Run Backend API** (Uses fast In-Memory DB by default for zero-friction setup):
   ```bash
   export PORT="5050"
   dotnet run --project src/IPOForge.Api/IPOForge.Api.csproj
   ```
   * Health Check: `http://localhost:5050/health`
   * Swagger UI: `http://localhost:5050/swagger`

3. **Run Frontend Web UI**:
   ```bash
   cd frontend/ipo-forge-web
   npm install
   npm run dev
   ```
   * Open `http://localhost:5173` in your browser.

---

### Option 2: Run with Docker Compose

Run the entire stack (PostgreSQL + ASP.NET Core API + React Frontend) with a single command:

```bash
docker-compose up --build
```
* Access the unified full-stack application at: `http://localhost:5000`
* Health Check: `http://localhost:5000/health`

---

## 🧪 Automated Testing

IPOForge includes a comprehensive automated test suite with **100% passing tests**:

```bash
# Run all Unit and Integration Tests
dotnet test IPOForge.slnx -v normal
```

---

## 🌐 Deploying to Render Free Tier

1. Fork or push this repository to your GitHub account.
2. In [Render Dashboard](https://dashboard.render.com/), click **New +** → **Blueprint**.
3. Select your `IPOForge` repository.
4. Render will read `render.yaml` and provision:
   * **`ipoforge-postgres`**: Free PostgreSQL database.
   * **`ipoforge-app`**: Free Web Service running the multi-stage Docker container.
5. Click **Apply**. Within a few minutes, your production IPOForge intelligence platform is live!

---

## ⚖️ Statutory & Compliance Disclaimer

> **IMPORTANT DISCLAIMER**: IPOForge is an algorithmic financial intelligence portal built strictly for educational, analytical, and academic purposes. Grey Market Premium (GMP) data is sourced from informal market channels and is subject to extreme volatility. Quantitative score ratings and verdicts are mathematical calculations and do not constitute SEBI-registered financial, investment, or legal advice. Always read the official Red Herring Prospectus (RHP) filed with SEBI and consult a certified financial advisor before investing.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
