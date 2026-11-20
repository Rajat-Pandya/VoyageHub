# Changelog

All notable changes to the VoyageHub project implemented on branch `feature/deploy-config` are documented in this file.

## [feature/deploy-config] - 2026-11-20

### Added
- **Task 1** (`2a34de2`): `feat(config): make Angular API URL configurable via environment`
  - Replaced hardcoded API URL (`http://localhost:5062/api`) in `voyage.service.ts` and `auth.service.ts` with `environment.apiUrl`.
  - Created `environment.ts` (local development: `http://localhost:5062/api`) and `environment.prod.ts` (production placeholder: `https://REPLACE-ME.azurewebsites.net/api`).
  - Added `fileReplacements` in `angular.json` for production configuration.
- **Task 2** (`d5246d0`): `feat(config): make CORS origins configurable`
  - Added `Cors:AllowedOrigins` string array in `appsettings.json` defaulting to localhost/127.0.0.1 on port 4200.
  - Implemented production CORS isolation in `Program.cs` that overrides origins strictly from environment variables (`Cors__AllowedOrigins*`), excluding localhost by default in non-Development.
- **Task 3** (`37f3fa1`): `feat(config): support environment variable override for database connection string`
  - Cleaned `ConnectionStrings:DefaultConnection` in `appsettings.json` with generic unsecret connection string.
  - Retained local SQL Server Express connection in `appsettings.Development.json`.
  - Ensured standard ASP.NET Core `ConnectionStrings__DefaultConnection` environment variable override.
- **Task 4** (`81b273c`): `feat(auth): validate JWT and connection string on startup in production`
  - Initialized local JWT secret key in .NET user-secrets and removed plaintext key from `appsettings.json`.
  - Implemented startup fail-fast validation in `Program.cs`: verifies connection string and JWT key (>= 32 characters) exist in non-Development environments without logging sensitive values.
- **Task 5** (`4b71c6a`): `feat(db): add Azure SQL schema and HashPassword utility`
  - Created `VoyageHub_Schema.azure.sql` derived from `database/VoyageHub_Schema.sql` without `CREATE DATABASE` or `USE` statements.
  - Replaced seeded passwords with `'DISABLED-SET-VIA-DEPLOYMENT-MD'` placeholder for cloud database safety.
  - Implemented `tools/HashPassword` CLI tool (BCrypt.Net-Next, work factor 11) to generate secure password hashes from stdin.
  - Preserved original `database/VoyageHub_Schema.sql` untouched for local development.
- **Task 6** (`8dc10e0`): `feat(frontend): add static client-side build and Static Web App routing config`
  - Configured `"static"` build target in `angular.json` producing client-only CSR build with root `index.html`.
  - Added `npm run build:static` script in `package.json`.
  - Created `staticwebapp.config.json` with `navigationFallback` rewriting client routes to `/index.html`.
- **Task 7** (`f5df3d1`): `feat(auth): enforce token-based identity and authorization`
  - Enforced caller identity and role extraction from JWT claims via `ICurrentUserService`.
  - Scoped Employee access strictly to own requests and stats; Managers see their own and direct reports' data; Finance/Admin filter freely.
  - Scoped approval queue strictly by caller's token role and id.
  - Updated Angular services and components to eliminate client-supplied `employeeId`, `approverId`, and `role` parameters.
  - Added 3 xUnit authorization tests in `VoyageHub.Tests` verifying access control rules.
- **Task 8** (`8badd3d`): `docs(auth): document hosted demo credentials and security isolation`
  - Documented in `README.md` that cloud/hosted instances use disabled seed placeholders and require setting passwords via `tools/HashPassword`.
  - Kept local seed credentials intact for development.
- **Task 9** (`bfaf511`): `feat(api): make Swagger UI and root redirect configuration-driven`
  - Added `Swagger:Enabled` configuration flag (`true` in Development, `false` in Production).
  - Wired Swagger UI and root `/` route: when disabled, Swagger UI is omitted and root returns HTTP 200 text status (`"VoyageHub API is running."`).
- **Task 10** (`5a04a0a`): `feat(data): ensure EF Core transient error resiliency and execution strategy`
  - Confirmed `EnableRetryOnFailure` is configured on SQL Server provider in `Program.cs`.
  - Verified no naked `BeginTransaction` or `TransactionScope` exist.
  - Added `RetryPolicyTests.cs` in `VoyageHub.Tests` confirming execution strategy availability for transient resiliency.
- **Task 11**: `feat(deploy): production dry run and deployment documentation`
  - Verified end-to-end production dry run on published Release artifacts with Azure SQL schema and custom environment variables.
  - Authored `DEPLOYMENT.md` detailing the free Azure deployment route, warm-up checklist, and clean-up steps.
  - Authored `docs/CONFIGURATION.md` configuration matrix.
  - Reconciled `context.md` specifications, invariants, and gap backlog.
