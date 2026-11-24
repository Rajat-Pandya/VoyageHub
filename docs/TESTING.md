# VoyageHub Testing Guide & Test Catalog

This document details how to execute the automated test suites, explains test coverage collection, catalogs the test classes, and outlines tested vs. untested behaviors.

---

## 1. Running Tests Locally

### Backend Test Suite (.NET / xUnit)
Run all backend unit and integration tests from the repository root:

```powershell
dotnet test "VoyageHub-Backend\VoyageHub.Tests\VoyageHub.Tests.csproj"
```
*(Executes all **77 passing xUnit tests** in ~2.5 seconds using in-memory and SQLite test databases.)*

### Frontend Test Suite (Angular / Vitest)
Run all frontend client unit tests from the frontend directory:

```powershell
cd "VoyageHub-Frontend"
npm test
```
*(Executes all **49 passing Vitest tests** in ~4 seconds and generates an LCOV coverage report.)*

---

## 2. Test Coverage Metrics & Report Generation

VoyageHub measures test coverage locally and imports it into SonarQube Clean as You Code Quality Gates:

| Project | Test Framework | Total Tests | Coverage Tool | Report Output Path | Coverage on New Code |
| :--- | :--- | :---: | :--- | :--- | :---: |
| **`VoyageHub-Backend`** | xUnit 2.9.3 | **77** | `dotnet-coverage` v18.12 | `VoyageHub-Backend/coverage.xml` | **83.1%** |
| **`VoyageHub-Frontend`** | Vitest 4.0.8 | **49** | `@vitest/coverage-v8` v4.1.12 | `VoyageHub-Frontend/coverage/AngularBasics/lcov.info` | **100.0%** |

### How to Regenerate Backend Coverage Report
```powershell
cd VoyageHub-Backend
dotnet-coverage collect --output coverage.xml --output-format xml "dotnet test"
```

### How to Regenerate Frontend Coverage Report
```powershell
cd VoyageHub-Frontend
# Configured in package.json as "test": "ng test --no-watch --coverage --coverage-reporters=lcov"
npm test
```

---

## 3. Backend Test Suite Catalog (`VoyageHub.Tests`)

| Test File | Test Count | Key Behaviors Verified |
| :--- | :---: | :--- |
| **`AcceptanceCriteriaTests.cs`** | 10 | End-to-end acceptance scenarios: draft creation, budget compliance validation against department policy caps, submission transition to `Manager Review`, manager approval advancing to `Finance Review`, manager rejection cascading cancellation to remaining steps, and audit log persistence. |
| **`ApprovalServiceTests.cs`** | 5 | Sequential approval routing logic: manager approval advancing step status, finance review activation, rejection handling, and queue retrieval filtering. |
| **`AuthorizationTests.cs`** | 3 | Role-based endpoint accessibility: verifies employees cannot access manager queues, line managers are restricted to direct reports, and finance/admin roles possess elevated approval authority. |
| **`AuthServiceTests.cs`** | 5 | Authentication and credential verification: BCrypt password verification, bad password rejection, JWT claims payload construction, and profile update persistence. |
| **`EmployeesControllerTests.cs`** | 13 | Admin employee management endpoints: list retrieval, profile lookups, creation validation failures, update actions, deletion results, CSV template downloads, and CSV batch upload parsing. |
| **`EmployeeServiceTests.cs`** | 9 | Employee business service: full name and email validation, duplicate email conflict handling, department assignment, manager association, and CSV stream processing using SQLite in-memory database. |
| **`ExpensesControllerTests.cs`** | 17 | Expense endpoints: receipt proof document upload, file size validation (>15MB rejection), file extension whitelisting (.exe rejection), receipt streaming (`GetProofFile`), single claim approval/rejection, and batch payout execution. |
| **`ExpenseServiceTests.cs`** | 10 | Expense business logic: pending summary calculations, zero/negative amount rejection, non-existent travel request validation, audit trail attribution, and batch reimbursement status changes. |
| **`PolicyServiceTests.cs`** | 5 | Policy management: department travel policy retrieval, budget ceiling updates, allowed flight class modifications, and duplicate policy creation prevention per department. |
| **`RetryPolicyTests.cs`** | 1 | EF Core execution strategy resilience: verifies that `EnableRetryOnFailure` is configured on the SQL Server DbContext options. |
| **`TravelRequestValidationTests.cs`**| 4 | Date constraints (`StartDate <= EndDate`), budget estimation thresholds, and request code formatting. |

---

## 4. Frontend Test Suite Catalog (`VoyageHub-Frontend`)

| Test File | Test Count | Key Behaviors Verified |
| :--- | :---: | :--- |
| **`app.spec.ts`** | 2 | Root application component bootstrapping, title binding, and navigation layout rendering. |
| **`auth.service.spec.ts`** | 6 | Authentication state management via Angular Signals: initial unauthenticated state, login success setting token in `localStorage`, logout clearing signals, persona switching, and display name resolution. |
| **`dashboard.spec.ts`** | 4 | Dashboard component initialization: KPI metrics rendering, pending actions display, and responsive activity list layout. |
| **`employee-management.spec.ts`** | 10 | Admin employee management view: directory table sorting with custom compare functions, department filtering, search input binding, modal form toggling, and CSV upload actions. |
| **`login.spec.ts`** | 7 | Authentication view: email/password form validation, persona quick-select buttons, submission loading state, and error message banners. |
| **`role.guard.spec.ts`** | 20 | Client navigation security: route access evaluation across all 4 personas (Employee, Manager, Finance, Admin) for protected routes, redirecting unauthorized users to `/dashboard` or `/login`. |

---

## 5. What Is Tested vs. Not Tested

### Thoroughly Tested Behaviors
- Multi-tier approval state machine transitions (`Draft` $\rightarrow$ `Manager Review` $\rightarrow$ `Finance Review` $\rightarrow$ `Approved` / `Rejected`).
- Budget cap checking and policy enforcement flags (`Flagged` vs. `Compliant`).
- Token-derived identity scoping and role-based permissions.
- Out-of-pocket expense reimbursement calculations and batch payouts.
- File upload validations (size caps, MIME extensions) and receipt streaming.
- Employee directory CSV batch processing and uniqueness constraints.

### Behaviors Intentionally Not Tested (Out of Scope)
- **Live GDS Flight / Hotel Booking Engines**: Third-party airline and hotel booking APIs are simulated; no live external booking calls exist.
- **Real SMTP Email Dispatch**: Email notifications are simulated in-app via the support desk and audit log records.
- **Express 5 SSR Server Engine (`server.mjs`)**: Server-Side Rendering bundling is verified via build compilation (`npm run build`), but Node SSR serving is not exercised in unit tests.
