# ADR-003: Angular SSR Support with Static Client-Side Rendered (CSR) Build

## Status
Accepted

## Context
Angular 21 introduces unified Server-Side Rendering (SSR) via `@angular/ssr` and Express 5 as a standard feature. While SSR improves initial page load metrics and Search Engine Optimization (SEO) for public websites, corporate enterprise intranet tools (like VoyageHub) typically run behind authentication where SEO is irrelevant. Furthermore, zero-cost static hosting platforms like Azure Static Web Apps require pure static client-side bundles (`index.html` + JS/CSS) rather than running a long-lived Node.js server.

## Decision
Maintain **dual build targets** in `angular.json`:
1. **Universal SSR Target (`ng build`)**: Retains the default Angular SSR build producing `dist/AngularBasics/server/server.mjs` for Node.js or Docker container deployments.
2. **Dedicated Static CSR Target (`npm run build:static`)**: Configures `"static"` build target with `ssr: false`, generating pure client-side bundles in `dist/AngularBasics/browser/` paired with `staticwebapp.config.json` for serverless Azure Static Web Apps deployment.

## Consequences
- **Positive**: Maximum deployment flexibility. The project can be deployed to serverless static hosting (Azure Static Web Apps) without Node compute costs, while retaining full SSR capabilities for containerized or enterprise Node hosting.
- **Negative / Trade-offs**: Requires maintaining two build scripts in `package.json` (`build` and `build:static`).
