# Changelog

All notable changes to the VoyageHub project implemented across branches are documented in this file. This project adheres to [Semantic Versioning](https://semver.org/) and [Keep a Changelog](https://keepachangelog.com/).

## [feature/sonar-quality-gate] - 2026-11-24

### Added & Improved
- **SonarQube Quality Gate Compliance**: Resolved all Quality Gate conditions for both `VoyageHub-Backend` (83.1% new code coverage, 0.0% duplication, 0 new violations) and `VoyageHub-Frontend` (100.0% new code coverage, 0.0% duplication, 0 new violations).
- **Backend Test Suite Expansion**:
  - Authored comprehensive unit tests in `VoyageHub.Tests/ExpenseServiceTests.cs` (10 tests) covering pending summaries, expense submission validations, single claim approval/rejection, and batch reimbursement payouts.
  - Authored comprehensive unit tests in `VoyageHub.Tests/EmployeeServiceTests.cs` (9 tests) covering employee creation validations, database uniqueness checks, updates, deletions, and CSV stream processing.
  - Authored comprehensive controller tests in `VoyageHub.Tests/EmployeesControllerTests.cs` (13 tests) and `VoyageHub.Tests/ExpensesControllerTests.cs` (17 tests) covering file validations, error handling, and proof document streaming.
  - Expanded backend test suite from 48 to 77 passing xUnit tests.
- **Frontend Test Suite & Coverage Reporting**:
  - Integrated `@vitest/coverage-v8` (`^4.1.12`) to enable V8 code coverage collection during `ng test`.
  - Configured LCOV coverage report generation (`coverage/AngularBasics/lcov.info`) and wired `sonar.javascript.lcov.reportPaths` in `sonar-project.properties`.
  - Authored unit test suites for `AuthService` and `EmployeeManagementComponent`, expanding frontend tests to 49 passing Vitest tests.

### Fixed & Refactored
- **ReDoS Vulnerability Prevention** (`7e80d4f`): Added explicit 250ms execution timeouts to regex evaluation in `EmployeeService.cs` (`csharpsquid:S6444`).
- **File Upload Parameter Binding** (`954cd3f`): Bound multipart file uploads directly to controller action parameters (`IFormFile? file`) rather than accessing raw `Request.Form.Files` dictionary (`csharpsquid:S6932`).
- **Eliminated Code Duplication** (`80041e4`): Extracted `SaveProofFileAsync` helper method in `ExpensesController.cs`, reducing new code duplication from 5.5% down to 0.0%.
- **Translatable EF Core Queries** (`55cdd7a`): Refactored case-insensitive database string lookups in `EmployeeService.cs` to use `EF.Functions.Like` instead of in-memory `.ToLower()` or unsupported `string.Equals(..., StringComparison)`.
- **String Sorting Comparison** (`64106d8`): Provided explicit string comparison function for array sorting in `employee-management.ts` (`typescript:S2871`).
- **Test Performance & Cleanup** (`4fd3105`, `5a42348`): Returned concrete `FormFile` in test helpers (`CA1859`) and registered `GC.SuppressFinalize` in disposable test fixture teardowns (`CA1816`).

---

## [Hotfixes & Bug Fixes] - 2026-11-23

### Fixed
- **Swagger JSON 500 Internal Server Error** (`0ea6721`): Removed conflicting `[FromForm]` attributes on `IFormFile` parameters in API controllers that caused Swashbuckle schema generation crashes.
- **Obsolete Vendor Prefix Cleanup** (`1894034`): Removed deprecated `-webkit-overflow-scrolling` property from dashboard CSS.
- **Template Reversion** (`6db17cb`): Restored request list template and styling state prior to regression.

---

## [feature/ui-consistency] - 2026-11-23

### Added & Refactored
- **Design Tokens & Primitives** (`3af9a72`): Established universal 4px spacing scale (`--space-0` through `--space-12`), radius tokens, typography scales, layout primitives (`.page`, `.page-header`, `.card`, `.form-grid`, `.table-wrap`, `.empty-state`), and unified status chip mapping in `src/styles/`.
- **Shared Standalone Components**: Created `app-page-header`, `app-empty-state`, and `app-status-chip` in `src/app/shared/`.
- **App Shell** (`342538d`): Refactored navigation bar, responsive sidebar, and main view container using spacing tokens and normalized $\ge 40\text{px}$ touch targets.
- **Login & Auth** (`f83f0fa`): Cleaned up authentication cards, normalized form field heights to 40px, and replaced hardcoded margins with spacing tokens.
- **Dashboard** (`eea2ee0`): Structured KPI cards with responsive grid, normalized activity table container, and consistent section spacing.
- **Requests Views** (`4c7e1e1`): Standardized request list, new request multi-step form, and request detail views with unified layout classes, responsive table containers, and consistent spacing.
- **Approvals Queue** (`55f1f96`): Applied `.page`, `.page-header`, 3-column KPI cards, unified action buttons, and responsive table wrapper.
- **Finance Review** (`cdf54f6`): Replaced custom containers with `.page`, `.page-header`, structured reimbursement action bar, and responsive expense item table.
- **Analytics & Reports** (`d071697`): Converted analytics views to `.page` and `.page-header`, refactored split charts grid and department budget table.
- **Admin Views** (`4fa0cfd`): Standardized policy config, audit logs, and employee management directory with `.page`, `.page-header`, `.table-wrap`, and min 40px controls.
- **Support Desk** (`4dfcdef`): Unified support emergency banner, ticket submission form grid, and ticket tracking table.
- **User Settings** (`a92188f`): Aligned user profile tabs, travel preferences, notification toggles, and security views with design tokens; preserved read-only corporate directory records.
- **Documentation**: Authored `docs/UI-GUIDELINES.md` detailing token reference, layout classes, component patterns, and do/don't checklist.

---

## [feature/deploy-config] - 2026-11-20

### Added
- **Configurable API URL** (`2a34de2`): Replaced hardcoded API URL (`http://localhost:5062/api`) in services with environment-driven `environment.apiUrl`. Added `environment.ts` and `environment.prod.ts` with Angular production file replacements.
- **Configurable CORS Origin Whitelist** (`d5246d0`): Added `Cors:AllowedOrigins` in `appsettings.json`. Enforced production origin isolation in `Program.cs` via `Cors__AllowedOrigins*` environment variables.
- **Database Connection String Overrides** (`37f3fa1`): Cleaned `appsettings.json` and ensured ASP.NET Core `ConnectionStrings__DefaultConnection` environment variable override works seamlessly.
- **JWT & Connection String Fail-Fast Validation** (`81b273c`): Initialized local JWT secret in .NET user-secrets. Enforced startup validation in `Program.cs` aborting execution if connection strings are missing or if JWT key is shorter than 32 characters in production.
- **Azure SQL Schema & Password Hasher Tool** (`4b71c6a`): Authored `database/VoyageHub_Schema.azure.sql` with disabled password placeholders (`'DISABLED-SET-VIA-DEPLOYMENT-MD'`). Implemented `tools/HashPassword` console utility for secure BCrypt hash generation.
- **Static Client Build** (`8dc10e0`): Configured `"static"` build target in `angular.json` (`npm run build:static`) and authored `staticwebapp.config.json` with client-side route fallback.
- **Token-Based Identity Scoping** (`f5df3d1`): Enforced caller identity extraction strictly from JWT claims via `ICurrentUserService`. Eliminated client-supplied identity parameters (`employeeId`, `approverId`, `role`) across controllers and Angular services.
- **Configuration-Driven Swagger UI** (`bfaf511`): Added `Swagger:Enabled` flag. Disabled Swagger by default in production and mapped root route `/` to a lightweight status check string.
- **EF Core Transient Resilience** (`5a04a0a`): Verified `EnableRetryOnFailure` on SQL Server DbContext configuration to survive Azure SQL auto-pausing.
- **Deployment Documentation & Dry Run**: Executed production Release build dry run. Authored `DEPLOYMENT.md` and `docs/CONFIGURATION.md`.
