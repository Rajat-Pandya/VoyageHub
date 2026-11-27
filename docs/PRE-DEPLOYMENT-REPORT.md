# VoyageHub Pre-Deployment Audit Report

**Audit Date:** 2026-11-27  
**Target Hosting:** Azure Static Web Apps (Angular 21 CSR) + Azure App Service on Linux (.NET 10) + Azure SQL Database  
**Audited Commit:** `b691203` (`master`, tag `pre-release-prep`)  
**Audit Mode:** Strictly read-only (no tracked files modified, no commits, no pushes, no fixes applied).

---

## Executive Summary

| Severity | Count | Summary |
| :--- | :---: | :--- |
| **BLOCKER** | 5 | 1. `VoyageHub_Schema.azure.sql` is missing 5 EF Core-mapped columns (`TRAVEL_REQUESTS.IsInternational`, `EXPENSES.RejectionReason`, `EXPENSES.OriginalCurrency`, `EXPENSES.OriginalAmount`, `EXPENSES.ExchangeRate`), causing HTTP 500 `SqlException` on requests & expenses.<br>2. `appsettings.json` ships a LocalDB `DefaultConnection` string, so missing `ConnectionStrings__DefaultConnection` in Production does **not** fail fast when run from `publish/`.<br>3. Missing `Cors__AllowedOrigins` in Production does **not** fail fast (`Program.cs:19-35`).<br>4. Unauthenticated `[AllowAnonymous]` endpoints expose user directory/template data (`GET /api/auth/personas`, `GET /api/employees/csv-template`).<br>5. IDOR / missing ownership scoping on `SupportController` (`GET /api/support/tickets`, `GET /api/support/tickets/{id}`, `POST /api/support/tickets` trusting `dto.UserId`) and `GET /api/expenses/proof/{fileName}` (any authenticated user can read any receipt). |
| **ACTION REQUIRED** | 1 | `VoyageHub-Frontend/src/environments/environment.prod.ts` still contains `apiUrl: 'https://REPLACE-ME.azurewebsites.net/api'` (must be updated before final static build). |
| **WARNING** | 9 | Hardcoded JWT fallback key in `Program.cs:39`, `JwtTokenService.cs:23`, `appsettings.example.json:13`; 500 responses in `AuthController` and `SupportController` leak `ex.Message`; no `UseForwardedHeaders` before `UseHttpsRedirection` for App Service Linux TLS termination; `.gitignore` missing `publish/`, `.env*`, `secrets.json`; frontend initial bundle exceeds 500 kB budget (736.22 kB raw); `PRESET_PERSONAS` bundled in prod JS; `Manager` can pass `?employeeId=` on `POST /api/travelrequests`; `VoyageHub_Schema.azure.sql` exists in two places and drops/re-seeds demo data; docs (`README.md`, `CHANGELOG.md`, `context.md`) cite stale test counts (77/49 vs actual 94/69). |
| **NOT VERIFIED** | 2 | Linux container dry run (`mcr.microsoft.com/dotnet/aspnet:10.0`) — Docker Desktop Linux Engine daemon not running; SonarQube live metrics/quality gate — local SonarQube not running and SonarQube MCP not configured in session. |

---

## 1. Repository State

- **Current Branch:** `master` (`remotes/origin/master`)
- **Working Tree (`git status -s`):** Clean prior to writing this report.
- **Unmerged Local Branches:** None (`git branch -a` shows only `* master`).
- **Commits Not on `main`:** `git log main..HEAD` returned `fatal: ambiguous argument 'main..HEAD'` (the repository uses `master`, not `main`).
- **Tags:** `pre-release-prep`
- **Deployed Commit Short Hash:** `b691203`
- **Last 15 Commits (`git log -n 15 --oneline`):**
  - `b691203` feat(requests): add transport mode selector (Flight/Train/Cab/Bus) in New Request
  - `eda11f5` chore: fix App_Data/Receipts ignore pattern with **/ prefix
  - `bd2cb59` chore: ignore and untrack SonarQube, coverage.xml, and uploaded receipt artifacts
  - `9cefedd` feat(enterprise): add close guard, rejection reasons, resubmit, multi-currency FX, duplicate detection, and CSV export
  - `dcea1cf` fix(dashboard): show employee name under request ID for Admin and Finance only
  - `7247296` feat(dashboard): show employee name with request ID and role-aware overview title
  - `69db372` First 26/11/26
  - `79152ea` fix(api): ignore EF navigation cycles in JSON serializer to prevent 500 on POST
  - `822fb07` feat(reimbursements): redesign My Reimbursements as Navan/Concur trip expense reports
  - `20221e4` feat(reimbursements): add Corporate-Paid vs Claimed breakdown and Self-Paid itinerary option
  - `0446713` feat(expenses): add one-click itinerary expense claim and trip spend context
  - `fdd6443` feat(finance): add trip link, submitted date, and budget context to expense review table
  - `4efde59` fix(finance): replace mock invoice stamp with real claim summary and inline PDF preview
  - `f8db86d` fix(requests): bust GET cache and immediately update detail signals after mutations
  - `b58f081` fix(requests): scope approved banner, cancel, back link, and proof actions to role and owner
- **`.gitignore` Coverage (`.gitignore:1-78`):**
  - Covered: `bin/`, `obj/`, `node_modules/`, `.sonarqube/`, `.scannerwork/`, `coverage/`, `coverage.xml`, `*.lcov`, `appsettings.*.local.json`, `**/App_Data/Receipts/`, `dist/`, `.angular/`.
  - **WARNING — Missing from `.gitignore`:** `publish/` (publish output folders), `.env` / `.env*`, and `secrets.json` (user-secrets).
- **Tracked Files Check (`git ls-files`):**
  - No `bin/`, `obj/`, `node_modules/`, `.sonarqube/`, `.scannerwork/`, `coverage/`, or `Receipts/` files are tracked.
  - Largest tracked files: `Assets/SRS_Travel_Management_System.pdf` (315.9 KB), `VoyageHub-Frontend/package-lock.json` (307.6 KB).
  - Tracked config files: `appsettings.json`, `appsettings.Development.json`, `appsettings.example.json` (contains a dev JWT key string — see Section 11).

---

## 2. Build and Tests

### Backend (`VoyageHub-Backend/VoyageHub.slnx`)
- `dotnet build VoyageHub-Backend/VoyageHub.slnx -c Release`: **PASS** — `0 Warning(s), 0 Error(s)`.
- `dotnet test VoyageHub-Backend/VoyageHub.slnx --no-build -c Release`: **PASS** — **Total: 94, Passed: 94, Failed: 0, Skipped: 0** (3s).
- **Target Frameworks & Package Alignment:**
  - `VoyageHub.Api.csproj`: `net10.0` (`Microsoft.AspNetCore.Authentication.JwtBearer` `10.0.11`, `Microsoft.AspNetCore.OpenApi` `10.0.11`, `Microsoft.EntityFrameworkCore.Design` `10.0.11`, `Swashbuckle.AspNetCore` `10.3.1`).
  - `VoyageHub.Business.csproj`: `net10.0` (`BCrypt.Net-Next` `4.2.0`, `System.IdentityModel.Tokens.Jwt` `7.7.1`).
  - `VoyageHub.Data.csproj`: `net10.0` (`Microsoft.EntityFrameworkCore.SqlServer` `10.0.11`, `System.Security.Cryptography.Pkcs` `10.0.15`).
  - `VoyageHub.Tests.csproj`: `net10.0` (`xunit` `2.9.3`, `Moq` `4.20.72`, `Microsoft.EntityFrameworkCore.InMemory` `10.0.11`, `Microsoft.EntityFrameworkCore.Sqlite` `10.0.11`).
  - All projects target `net10.0` and all EF Core / ASP.NET Core packages match on `10.0.11`.

### Frontend (`VoyageHub-Frontend`)
- `npm ci --dry-run`: **PASS** — `package-lock.json` is consistent (exit code 0; 4 packages show npm `install-scripts` allowlist warnings).
- `npm test` (`ng test --no-watch --coverage --coverage-reporters=lcov`): **PASS** — **13 test files passed, 69 tests passed, 0 failed, 0 skipped**.
- `npm run build` (`ng build`): **PASS** with 2 budget warnings.
- `npm run build:static` (`ng build --configuration static`, `package.json:8`, `angular.json:61-84`): **PASS** with 2 budget warnings:
  - **WARNING:** `bundle initial exceeded maximum budget. Budget 500.00 kB was not met by 236.22 kB with a total of 736.22 kB` (`main-XKB3RDQV.js` 719.66 kB + `styles-MV57ICLU.css` 16.56 kB; estimated transfer size 157.72 kB).
  - **WARNING:** `src/app/requests/request-detail/request-detail.css exceeded maximum budget. Budget 12.00 kB was not met by 1.92 kB with a total of 13.92 kB`.

---

## 3. Configuration Inventory

| Key | Where Read | Local Source | Default Value | Required in Prod? | Secret? |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `ConnectionStrings:DefaultConnection` | `Program.cs:14` | `appsettings.Development.json:9` / `appsettings.json:10` | LocalDB `AngularAuthDb` in `appsettings.json:10` | Yes | Yes |
| `Jwt:Key` | `Program.cs:15`, `JwtTokenService.cs:23` | User Secrets (`5faf57db-...`) | Dev fallback in `Program.cs:39` & `JwtTokenService.cs:23` | Yes (>=32 chars) | Yes |
| `Jwt:Issuer` | `Program.cs:16`, `JwtTokenService.cs:24` | `appsettings.json:20` | `VoyageHub.Api` | Recommended | No |
| `Jwt:Audience` | `Program.cs:17`, `JwtTokenService.cs:25` | `appsettings.json:21` | `VoyageHub.Client` | Recommended | No |
| `Jwt:ExpiryMinutes` | `JwtTokenService.cs:26` | `appsettings.json:22` | `1440` | No | No |
| `Cors:AllowedOrigins` | `Program.cs:114-135` | `appsettings.json:12-18` | `localhost:4200` (stripped in Prod) | Yes | No |
| `Swagger:Enabled` | `Program.cs:183` | `appsettings.Development.json:12` (`true`), `appsettings.json:25` (`false`) | `IsDevelopment()` (`false` in Prod) | No | No |
| `FrontendUrl` | `AuthController.cs:111` | Not in `appsettings*.json` | `http://localhost:4200` (`AuthService.cs:19`) | Recommended (password reset link) | No |
| `Logging:LogLevel:*` | `WebApplication.CreateBuilder` | `appsettings.json:2-7` | `Information` / `Warning` | No | No |
| `AllowedHosts` | `WebApplication.CreateBuilder` | `appsettings.json:8` | `*` | No | No |

### Fail-Fast Verification (Production)
- **Missing `Jwt__Key`:** **PASS** — Throws `InvalidOperationException: Startup failed: JWT signing key ('Jwt:Key') is missing or not configured.` (`Program.cs:28`).
- **Short `Jwt__Key` (9 chars):** **PASS** — Throws `InvalidOperationException: Startup failed: JWT signing key ('Jwt:Key') must be at least 32 characters long (found 9).` (`Program.cs:33`).
- **Missing `ConnectionStrings__DefaultConnection` (run from `publish/`):** **BLOCKER (FAIL)** — Process **stayed alive** (`TEST3_FROM_PUBDIR_STAYED_ALIVE=True`) because `appsettings.json:10` supplies a LocalDB default, bypassing `string.IsNullOrWhiteSpace(connectionString)` at `Program.cs:21`.
- **Missing `Cors__AllowedOrigins__0` (run from `publish/`):** **BLOCKER (FAIL)** — Process **stayed alive** with an empty CORS origin list (`Program.cs:19-35` does not validate `corsOrigins.Length > 0`).

### Documentation Mismatches (`docs/CONFIGURATION.md` & `DEPLOYMENT.md`)
- Both docs state `ConnectionStrings:DefaultConnection` fails fast in Production, which is contradicted by `appsettings.json:10` providing a non-empty fallback when `publish/appsettings.json` is present.
- `docs/CONFIGURATION.md` & `DEPLOYMENT.md` do not list `FrontendUrl` (`AuthController.cs:111`), which defaults to `http://localhost:4200` in `AuthService.cs:19` if omitted.
- `DEPLOYMENT.md:53-68` documents Windows/mixed paths; for App Service Linux, zip entries must use forward slashes.
- No hardcoded `UseUrls` in `Program.cs`; `launchSettings.json` ports (`5062`, `7213`) apply only in Development.

### Azure App Service Application Settings Table

| Name | Example Value | Secret |
| :--- | :--- | :---: |
| `ASPNETCORE_ENVIRONMENT` | `Production` | No |
| `ConnectionStrings__DefaultConnection` | `Server=tcp:<server>.database.windows.net,1433;Database=voyagehub-db;User ID=<user>;Password=<pw>;Encrypt=True;TrustServerCertificate=False;` | Yes |
| `Jwt__Key` | `<48-byte-random-base64-string>` | Yes |
| `Jwt__Issuer` | `VoyageHub.Api` | No |
| `Jwt__Audience` | `VoyageHub.Client` | No |
| `Jwt__ExpiryMinutes` | `1440` | No |
| `Cors__AllowedOrigins__0` | `https://<your-static-app>.azurestaticapps.net` | No |
| `Swagger__Enabled` | `false` | No |
| `FrontendUrl` | `https://<your-static-app>.azurestaticapps.net` | No |

---

## 4. Security Review

### Authorization Matrix
- **AuthController (`api/auth`):**
  - `POST login` — `[AllowAnonymous]`
  - `GET personas` — `[AllowAnonymous]` (**BLOCKER:** leaks all user names, emails, roles, departments without auth, `AuthController.cs:57-63`)
  - `PUT profile`, `POST change-password` — `[Authorize]`
  - `POST forgot-password`, `POST verify-reset-token`, `POST reset-password` — `[AllowAnonymous]`
- **TravelRequestsController (`api/travelrequests`, `[Authorize]`):**
  - `GET`, `GET stats`, `GET {id}`, `POST {id}/close` — `Employee,Manager,Finance,Admin`
  - `POST` — `Employee,Manager` (**WARNING:** `TravelRequestsController.cs:94-97` only overrides `employeeId` if `User.IsInRole("Employee")`, allowing a Manager to pass `?employeeId=`)
  - `DELETE {id}`, `POST {id}/submit`, `POST {id}/segments`, `POST {id}/stays` — `Employee,Manager,Admin`
- **ApprovalsController (`api/approvals`):** `[Authorize(Roles = "Manager,Finance,Admin")]` on `GET queue`, `POST {stepId}/action`.
- **ExpensesController (`api/expenses`, `[Authorize]`):**
  - `GET pending`, `POST {id}/action`, `POST batch-payout`, `GET export-payout` — `Finance,Admin`
  - `GET my-expenses`, `GET my-reports`, `POST`, `POST {id}/resubmit`, `POST upload-proof`, `POST {id}/proof`, `GET proof/{fileName}` — `Employee,Manager,Finance,Admin` (**BLOCKER:** `GetProofFile` at `ExpensesController.cs:207-237` does not verify expense ownership; any authenticated user can download any receipt by filename).
- **EmployeesController (`api/employees`):** `[Authorize(Roles = "Admin")]` class-level, except `GET csv-template` has `[AllowAnonymous]` (**BLOCKER:** `EmployeesController.cs:102-104`).
- **PoliciesController (`api/policies`, `[Authorize]`):** `GET`, `GET my-policy` (`Employee,Manager,Finance,Admin`); `GET departments`, `PUT {id}`, `POST`, `DELETE {id}` (`Admin`).
- **AnalyticsController (`api/analytics`):** `[Authorize(Roles = "Manager,Finance,Admin")]` on `GET overview`, `GET employee-reports`, `GET cost-reports`.
- **AuditLogsController (`api/auditlogs`):** `[Authorize(Roles = "Admin")]` on `GET`.
- **SupportController (`api/support`, `[Authorize]`):**
  - `GET tickets`, `GET tickets/{id}`, `POST tickets` — `Employee,Manager,Finance,Admin` (**BLOCKER:** `SupportService.cs:28-106` does not scope `GetTicketsAsync` or `GetTicketByIdAsync` by `_currentUserService.UserId` for `Employee`, and `CreateTicketAsync` trusts `dto.UserId` instead of `_currentUserService.UserId`).
  - `POST tickets/{id}/status` — `Admin,Manager,Finance`.

### Identity, Authentication, Error Handling, CORS, Proxy, Validation, Portability
- **JWT Validation (`Program.cs:89-100`):** Validates Issuer, Audience, Lifetime, IssuerSigningKey; default clock skew (5m); token lifetime 1440m (`appsettings.json:22`); stored in `sessionStorage` (`auth.service.ts:157-160`).
- **Passwords (`AuthService.cs:67`):** BCrypt only (`BCrypt.Net.BCrypt.Verify`); no bypasses, no plaintext comparisons, no password/hash logged.
- **Error Handling (`WARNING`):** `AuthController.cs` (`52, 80, 100, 126`) and `SupportController.cs` (`43, 64, 85, 106`) catch `Exception` and return `$"Database error: {ex.Message}"` to clients in Production.
- **CORS (`Program.cs:137-149`):** `WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()` — does not combine `AllowAnyOrigin` with `AllowCredentials`.
- **Forwarded Headers / HTTPS (`WARNING`):** `Program.cs:202` calls `app.UseHttpsRedirection()` without `app.UseForwardedHeaders(...)` or HSTS; on App Service Linux (TLS terminated at reverse proxy), missing `ForwardedHeaders` can cause HTTPS redirect issues.
- **Swagger (`Program.cs:183-198`):** Disabled outside Development, including root `/` redirect (returns `200 text/plain`).
- **Input Validation:** `TravelRequestService.cs:239-257` validates `StartDate <= EndDate` and `EstimatedBudget > 0`; `ExpenseService.cs:291-294` validates `Amount > 0`; `EmployeesController.cs:92` checks `.csv` extension; `ExpensesController.cs:178-186` enforces 15 MB and `.pdf/.png/.jpg/.jpeg/.webp`.
- **`DateTime.Now`:** **PASS** — 0 occurrences in `VoyageHub-Backend/`.
- **Linux Paths / Case Sensitivity:** **PASS** — `ExpensesController.cs:189,196,217,218` uses `Path.Combine(_environment.ContentRootPath, "App_Data", "Receipts")`.

---

## 5. Production Dry Run (Windows & Linux)

### Windows Dry Run (`VoyageHub_PreDeploy` on `.\SQLEXPRESS06`, port `5199`)
- **Database Creation & User Seeding:** Created `VoyageHub_PreDeploy` from `VoyageHub_Schema.azure.sql`; seeded BCrypt hashes via `tools/HashPassword` for 5 users across all 4 roles.
- **Startup & First-Request Latency:** Started cleanly in `Production`; `GET /` returned `200` (`VoyageHub API is running.`) in **646 ms**.
- **Results (5d):**
  - Root `/` & `/swagger/index.html`: **PASS** (`/` -> `200`, `/swagger/index.html` -> `404`).
  - Anonymous protected endpoint (`GET /api/travelrequests`): **PASS** (`401`).
  - Login for all 4 roles (`Employee`, `Manager`, `Finance`, `Admin`): **PASS** (`200` with JWT).
  - Wrong role (`Employee` -> `GET /api/auditlogs`): **PASS** (`403`).
  - Employee IDOR (`GET /api/travelrequests?employeeId=...`, `GET /api/travelrequests/stats?employeeId=...`): **PASS** (`403`).
  - Employee IDOR detail (`GET /api/travelrequests/{id}`), Manager list (`GET /api/travelrequests`), Spoof create (`POST /api/travelrequests`), E2E flow (`POST /api/travelrequests`): **FAIL (HTTP 500)** — all failed with `SqlException: Invalid column name 'IsInternational'` (and `Expenses` queries fail on `ExchangeRate`, `OriginalAmount`, `OriginalCurrency`, `RejectionReason`) because `VoyageHub_Schema.azure.sql` is out of sync with EF Core entities.
  - CORS preflight (`http://localhost:4300` vs `http://evil.example`): **PASS** (`Access-Control-Allow-Origin: http://localhost:4300` vs omitted).
  - Invalid dates & negative budget (`POST /api/travelrequests`): **PASS** (`400`).
  - Missing `Jwt__Key` / short `Jwt__Key`: **PASS** (failed fast). Missing `ConnectionStrings__DefaultConnection` / `Cors__AllowedOrigins__0` from `publish/`: **FAIL** (stayed alive).

### Linux Container Dry Run (`mcr.microsoft.com/dotnet/aspnet:10.0`, port `8080`)
- **Status:** **NOT VERIFIED** — `docker` CLI is installed (`29.9.3`), but Docker Desktop Linux Engine daemon is not running (`open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified`).

---

## 6. Database Script (`VoyageHub_Schema.azure.sql`)

- **Azure SQL Compatibility:** **PASS** — No `CREATE DATABASE`, `USE`, `ALTER DATABASE`, `xp_cmdshell`, `sp_configure`, `FILESTREAM`, or cross-database references. (Note: lines 7-16 execute `DROP TABLE` if exists — **WARNING** against accidental re-run on live Prod).
- **Seed Data:** `@DefaultHash = 'DISABLED-SET-VIA-DEPLOYMENT-MD'` (not a valid BCrypt hash; no usable seeded password). However, script seeds sample travel requests, expenses, support tickets, and audit logs (`@UserAlex`, `@ReqBerlin`, etc.) beyond roles/departments/policies (**INFO**).
- **Entity Model Agreement:** **BLOCKER (FAIL)** — `VoyageHub_Schema.azure.sql` (and `database/VoyageHub_Schema.sql`) are missing `TRAVEL_REQUESTS.IsInternational` and `EXPENSES.RejectionReason`, `OriginalCurrency`, `OriginalAmount`, `ExchangeRate`.
- **Duplicate File:** Both `VoyageHub_Schema.azure.sql` (root) and `database/VoyageHub_Schema.azure.sql` exist and are identical.
- **`database/VoyageHub_Schema.sql`:** Unchanged (`git diff` empty).

---

## 7. Frontend Production Output

- **Build Output (`VoyageHub-Frontend/dist/AngularBasics/browser/`):**
  - `index.html` (21,255 B), `main-XKB3RDQV.js` (719,663 B), `styles-MV57ICLU.css` (16,556 B), `favicon.ico` (15,086 B), `staticwebapp.config.json` (176 B).
- **String Scan on `main-XKB3RDQV.js`:**
  - `localhost`: `False`
  - `127.0.0.1`: `False`
  - `VoyageDemo2026`: `False`
  - `http://`: Only W3C XML/SVG namespaces (`http://www.w3.org/2000/svg`, etc.) — no `http://` API URLs.
  - `rohan.sharma@voyagehub.in`: `True` (**WARNING:** `PRESET_PERSONAS` demo user list in `auth.service.ts:50-107` is bundled into production JS).
  - `REPLACE-ME`: `True` (from `environment.prod.ts`).
- **ACTION REQUIRED (`environment.prod.ts:3`):**
  - Currently `apiUrl: 'https://REPLACE-ME.azurewebsites.net/api'`. Must be updated to the deployed App Service URL (e.g., `https://<your-api-app>.azurewebsites.net/api`) before building for Static Web Apps. Confirmed swapped at build time via `angular.json:62-67` (`fileReplacements`).
- **`staticwebapp.config.json` & Deep Links:**
  - Valid JSON with `navigationFallback.rewrite: "/index.html"` excluding `/assets/*` and static asset extensions.
  - Verified locally via Node static server with SPA fallback on port `4300`: `GET /requests/123` returned `200` with `<app-root>`.

---

## 8. Code Quality

- **SonarQube Live Analysis:** **NOT VERIFIED** — Local SonarQube (`http://localhost:9000`) timed out / not running, and SonarQube MCP tools are not configured in this session.
- **Suppressions (`NOSONAR`, `#pragma warning disable`, `SuppressMessage`, `eslint-disable`):** **PASS** — 0 occurrences found across `VoyageHub-Backend/` and `VoyageHub-Frontend/src/`.
- **Exclusions (`VoyageHub-Frontend/sonar-project.properties:4`):** `**/node_modules/**,**/dist/**,**/.angular/**,**/coverage/**`.
- **Test Class Coverage Map:**
  - **Approval workflow:** `ApprovalServiceTests.cs`, `AcceptanceCriteriaTests.cs`
  - **Policy checks:** `PolicyServiceTests.cs`, `TravelRequestValidationTests.cs`, `new-request.spec.ts`, `policy-config.spec.ts`
  - **Expense validation:** `ExpenseServiceTests.cs`, `ExpensesControllerTests.cs`, `expense-review.spec.ts`, `my-reimbursements.spec.ts`
  - **Authorization behavior:** `AuthorizationTests.cs`, `RepositoryAndControllerCoverageTests.cs`, `role.guard.spec.ts`, `auth.interceptor.spec.ts`
  - **Gaps:** `SupportService` / `SupportController` ownership scoping and `ExpensesController.GetProofFile` ownership lack dedicated authorization tests.

---

## 9. Requirements Traceability

| Brief Requirement | Status | Evidence & Transaction Notes |
| :--- | :--- | :--- |
| Travel requests (draft/submit) | Implemented | `TravelRequestsController.cs`, `TravelRequestService.cs:232-469`. `CreateRequestAsync` & `SubmitDraftAsync` persist request + approval steps + audit log in a single EF Core `SaveChangesAsync()` implicit transaction. |
| Multi-level approval with comments | Implemented | `ApprovalsController.cs`, `ApprovalService.cs:118-190`. Single `SaveChangesAsync()` commits step update, request status, and audit log. |
| Auto-routing & configurable hierarchy | Implemented | `TravelRequestService.cs` (`BuildWorkflowStepsAsync`), `PolicyService.cs`, `PoliciesController.cs`. |
| Policy enforcement | Implemented | `PolicyEvaluator` in `TravelRequestService.cs:274,431` & `ApprovalService.cs:79`. |
| Itinerary segments & stays + timeline | Implemented | `TravelRequestsController.cs:122-138`, `request-detail` component. |
| Expenses with proof & reimbursement | Implemented | `ExpensesController.cs`, `ExpenseService.cs:284-508`. Single `SaveChangesAsync()` per action (`SubmitExpenseAsync`, `TakeActionAsync`, `BatchPayoutAsync`). |
| Reporting (cost, employee, dept spend) | Implemented | `AnalyticsController.cs`, `AnalyticsService.cs`. |
| Audit logs | Implemented | `AuditLogsController.cs`, `AuditService.cs`. |
| Role-based access control | Partial | Enforced on most endpoints; gaps on `SupportController`, `GetProofFile`, `personas`, `csv-template`. |
| Approval state machine | Implemented | `ApprovalService.cs:131-172` (Manager -> Finance Review -> Approved / Rejected). |
| Transactional data modeling | Partial | Uses single `SaveChangesAsync()` implicit transactions, but `VoyageHub_Schema.azure.sql` is out of sync with EF models. |
| No external integrations | Implemented | All data, auth, FX, and file storage are self-contained. |

---

## 10. Documentation

- **Relative Links:** All relative links in `README.md`, `DEPLOYMENT.md`, and `docs/CONFIGURATION.md` resolve to existing files.
- **Contradictions Found:**
  - `README.md:150` / `context.md:18` / `CHANGELOG.md:13,17` report 77 backend tests and 49 frontend tests; actual counts are **94 backend** and **69 frontend**.
  - `docs/CONFIGURATION.md:9,34` claims missing `ConnectionStrings:DefaultConnection` fails fast in Production, which `appsettings.json:10` bypasses.
- **Hosted-Instance Guidance & Demo Credentials:** `README.md:166` explicitly restricts demo credentials to local development and notes `VoyageHub_Schema.azure.sql` uses `'DISABLED-SET-VIA-DEPLOYMENT-MD'`. `DEPLOYMENT.md` uses placeholder `<password>` and `YourConfiguredPassword`.

---

## 11. Secrets & Git History

- **Git History Scan (`git rev-list --all`):** 0 matches for `squ_*`, `AKIA*`, `ghp_*`, or `-----BEGIN * PRIVATE KEY-----`.
- **Working Tree Findings:**
  - **WARNING:** Dev JWT signing key string present in `VoyageHub-Backend/VoyageHub.Api/Program.cs:39`, `VoyageHub-Backend/VoyageHub.Business/Services/JwtTokenService.cs:23`, and `VoyageHub-Backend/VoyageHub.Api/appsettings.example.json:13` (not a production secret, but should be removed/rotated if ever used outside local dev).
  - **INFO:** Sample template password in `VoyageHub-Backend/VoyageHub.Api/Controllers/EmployeesController.cs:108-110` and `DEMO_PASSWORD` constant in `VoyageHub-Frontend/src/app/services/auth.service.ts:39`.

---

## 12. Deployment Readiness Summary & Cleanup

### Before First Azure Deployment (Must Fix)
1. Update `VoyageHub_Schema.azure.sql` (and `database/VoyageHub_Schema.sql`) to include `TRAVEL_REQUESTS.IsInternational` and `EXPENSES.RejectionReason`, `OriginalCurrency`, `OriginalAmount`, `ExchangeRate`.
2. Remove the LocalDB `DefaultConnection` from `appsettings.json` (keep only in `appsettings.Development.json`) and add Production CORS origin validation in `Program.cs`.
3. Remove `[AllowAnonymous]` from `GET /api/auth/personas` and `GET /api/employees/csv-template`, and add ownership checks to `SupportService` and `ExpensesController.GetProofFile`.
4. Update `VoyageHub-Frontend/src/environments/environment.prod.ts` `apiUrl` to your real App Service URL, then run `npm run build:static`.

### Publish Commands (Linux App Service + Static Web Apps)
```powershell
# Backend (use forward-slash zip or az webapp deploy)
dotnet publish VoyageHub-Backend/VoyageHub.Api/VoyageHub.Api.csproj -c Release -o ./publish

# Frontend (after updating environment.prod.ts)
cd VoyageHub-Frontend && npm run build:static
```

### Post-Deploy Smoke Test Order
1. `GET https://<api>.azurewebsites.net/` -> `200` (`VoyageHub API is running.`), `GET .../swagger/index.html` -> `404`.
2. `POST .../api/auth/login` with an initialized Admin/Employee account -> `200` + JWT.
3. `GET .../api/travelrequests` (anonymous -> `401`; with JWT -> `200`).
4. Create + submit a request, approve as Manager & Finance, submit & reimburse an expense, verify `GET .../api/auditlogs`.
5. Open Static Web App URL and deep link `/requests/123` in browser; verify zero CORS/console errors.

### Audit Cleanup Confirmed
- Throwaway database `VoyageHub_PreDeploy` on `.\SQLEXPRESS06` was **dropped** (`DB_ID('VoyageHub_PreDeploy')` = `NULL`).
- Temporary directory `%TEMP%\voyagehub-predeploy` was **removed** (`TEMP_EXISTS=False`).

---

## Re-Audit (Post-Fix Verification)

**Branch:** `feature/pre-deploy-fixes` (commits `0992102` .. `912408a`)  
**Verdict:** **GO WITH FIXES** — All 5 blockers and all actionable hardening warnings are resolved and verified. One deploy-time action remains (`environment.prod.ts` `apiUrl`), plus two environment-dependent checks marked `NOT VERIFIED`.

### Status of Earlier Blockers & Warnings

| Item | Previous Status | Current Status | Verification Evidence |
| :--- | :---: | :---: | :--- |
| Schema mismatch (`IsInternational`, `RejectionReason`, `OriginalCurrency`, `OriginalAmount`, `ExchangeRate`) | BLOCKER | **PASS** | Added to `database/VoyageHub_Schema.azure.sql` & `database/VoyageHub_Schema.sql`; duplicate root script deleted; `SchemaScriptMatchesModelTests` passes (`107/107` backend tests); populated-DB `SET NOEXEC ON` guard verified in `sqlcmd`. |
| Connection string fail-fast bypassed (`appsettings.json`) | BLOCKER | **PASS** | Removed `ConnectionStrings` from `appsettings.json`; Production startup from `publish/` without `ConnectionStrings__DefaultConnection` fails fast (`InvalidOperationException`). |
| CORS fail-fast missing (`Program.cs`) | BLOCKER | **PASS** | Production startup without `Cors__AllowedOrigins` fails fast (`InvalidOperationException`). |
| Unauthenticated endpoints (`personas`, `csv-template`) | BLOCKER | **PASS** | `GET /api/auth/personas` returns `404` outside Development unless `Features:EnableDemoPersonas=true`; `PRESET_PERSONAS` stripped from prod JS bundle (`vikram.malhotra@voyagehub.in -> False`); `GET /api/employees/csv-template` returns `401` anonymous, `403` Employee, `200` Admin. |
| IDOR / missing ownership (`SupportService`, `GetProofFile`, Manager `Create`) | BLOCKER | **PASS** | `OwnershipAuthorizationTests` & `ExpensesControllerTests` pass; dry-run `GET /api/support/tickets/{otherId}` returns `403`, `POST /api/travelrequests` for Manager forces token `UserId`. |
| Forwarded headers & HSTS (`Program.cs`) | WARNING | **PASS** | `UseForwardedHeaders` (`XForwardedFor \| XForwardedProto`) + `UseHsts()` outside Development added. |
| Exception message leak in 500s (`AuthController`, `SupportController`) | WARNING | **PASS** | Replaced `$"Database error: {ex.Message}"` with generic message; exception still logged via `ILogger`. |
| Hardcoded JWT fallback key (`Program.cs`, `JwtTokenService`, `appsettings.example.json`) | WARNING | **PASS** | Removed fallback key; dev uses `dotnet user-secrets` (`Jwt:Key` present). |
| `.gitignore` gaps (`publish/`, `.env`, `.env.*`, `secrets.json`) | WARNING | **PASS** | Added to `.gitignore`. |
| Stale test counts in docs | WARNING | **PASS** | Updated `README.md`, `CHANGELOG.md`, `context.md`. |
| `environment.prod.ts` `apiUrl` placeholder | ACTION REQUIRED | **ACTION REQUIRED (at deploy)** | Must replace `https://REPLACE-ME.azurewebsites.net/api` with real App Service URL before final `npm run build:static`. |
| Frontend bundle budget warning (`736.30 kB` vs `500 kB`) | WARNING | **WARNING (non-blocking)** | Build succeeds; estimated transfer size `157.65 kB`. |
| Linux container dry run (`mcr.microsoft.com/dotnet/aspnet:10.0`) | NOT VERIFIED | **NOT VERIFIED** | Docker Desktop Linux Engine daemon not running (`open //./pipe/dockerDesktopLinuxEngine`). |
| SonarQube live scan | NOT VERIFIED | **NOT VERIFIED** | Local SonarQube (`localhost:9000`) not running. |

### Re-Verification Summary
- **Backend Build & Tests:** `dotnet build -c Release` (`0` warnings, `0` errors); `dotnet test` (**107 passed**, `0` failed, `0` skipped).
- **Frontend Tests & Static Build:** `npm test` (**69 passed**, `0` failed); `npm run build:static` succeeded (`736.30 kB` raw / `157.65 kB` transfer).
- **Production Dry Run (`VoyageHub_PreDeploy` on `.\SQLEXPRESS06`, port `5199`):**
  - `GET /` -> `200` (`651 ms`), `/swagger/index.html` -> `404`, `/api/auth/personas` -> `404`, `/api/employees/csv-template` -> `401` (anon) / `403` (Emp) / `200` (Adm).
  - Manager list (`200`, 7 items), Manager spoofed create (`201`), full E2E flow (`POST /api/travelrequests` `201` -> Manager approve `200` -> Finance approve `200` -> `POST /api/expenses` `200` -> Finance reimburse `200` -> `GET /api/auditlogs` recorded all 6 events).
- **Secrets Scan:** 0 secrets in tracked files.

### Final Azure App Service Settings Table

| Name | Example Value | Secret |
| :--- | :--- | :---: |
| `ASPNETCORE_ENVIRONMENT` | `Production` | No |
| `ConnectionStrings__DefaultConnection` | `Server=tcp:<server>.database.windows.net,1433;Database=voyagehub-db;User ID=<user>;Password=<pw>;Encrypt=True;TrustServerCertificate=False;` | Yes |
| `Jwt__Key` | `<48-byte-random-base64-string>` | Yes |
| `Jwt__Issuer` | `VoyageHub.Api` | No |
| `Jwt__Audience` | `VoyageHub.Client` | No |
| `Jwt__ExpiryMinutes` | `1440` | No |
| `Cors__AllowedOrigins__0` | `https://<your-static-app>.azurestaticapps.net` | No |
| `Swagger__Enabled` | `false` | No |
| `Features__EnableDemoPersonas` | `false` | No |
| `FrontendUrl` | `https://<your-static-app>.azurestaticapps.net` | No |

### Exact Deployment Commands

```powershell
# 1. Backend publish & forward-slash zip for Azure App Service Linux
dotnet publish VoyageHub-Backend/VoyageHub.Api/VoyageHub.Api.csproj -c Release -o ./publish
python -c "import os, zipfile; z=zipfile.ZipFile('api-deploy.zip','w',zipfile.ZIP_DEFLATED); [z.write(os.path.join(r,f), os.path.relpath(os.path.join(r,f),'publish').replace(os.sep,'/')) for r,_,fs in os.walk('publish') for f in fs]; z.close()"
az webapp deploy --resource-group rg-voyagehub-free --name <api-app-name> --src-path ./api-deploy.zip --type zip

# 2. Frontend static build (after updating environment.prod.ts apiUrl)
cd VoyageHub-Frontend
npm run build:static
# Deploy VoyageHub-Frontend/dist/AngularBasics/browser/ to Azure Static Web Apps
```

### Cleanup Confirmed
- Throwaway database `VoyageHub_PreDeploy` dropped (`DB_ID('VoyageHub_PreDeploy') = NULL`).
- Temporary directory `%TEMP%\voyagehub-predeploy` removed (`TEMP_EXISTS=False`).
