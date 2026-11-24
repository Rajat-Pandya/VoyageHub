# ADR-004: Free-Tier Cloud Hosting Architecture

## Status
Accepted

## Context
VoyageHub requires a reliable, publicly accessible cloud demonstration deployment without incurring ongoing cloud infrastructure hosting costs. Multiple cloud providers and free-tier strategies were evaluated:
1. **Azure Free Tiers**: Azure App Service F1 + Azure Static Web Apps + Azure SQL Database Free Offer.
2. **MonsterASP.NET Single-Site**: Windows IIS hosting with static files in `wwwroot`.
3. **Local Docker + Cloudflare Tunnel**: Local container deployment exposed via tunnel.

## Decision
Design and configure VoyageHub primarily for the **Azure Free Tiers architecture** while maintaining full compatibility with single-site IIS hosting and containerized local tunnels:
- **Client**: Azure Static Web Apps (Free plan, 100GB bandwidth/month).
- **Backend API**: Azure App Service (F1 Free plan, 1GB RAM, 60 CPU minutes/day).
- **Database**: Azure SQL Database (Free Offer, 100,000 vCore seconds/month, 32GB storage, auto-pause enabled).
- **Resilience**: Configure `EnableRetryOnFailure` on EF Core to gracefully handle database wake-up latency when resuming from auto-pause.

## Consequences
- **Positive**: Total infrastructure cost is $0.00/month. Separates concerns across dedicated managed services with HTTPS and automated deployment integration.
- **Negative / Trade-offs**: Free-tier resources sleep upon inactivity: App Service F1 shuts down after 20 minutes of idle time, and Azure SQL auto-pauses when free limits are approached, requiring a 30–60 second warm-up request prior to live demonstrations.
