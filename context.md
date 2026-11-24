# VoyageHub System Context & Specification

## 1. Overview

VoyageHub is an enterprise corporate travel and expense management application designed to coordinate employee business travel requests, multi-level approvals, expense reimbursements, travel policies, employee directories, and corporate support desk workflows. The system serves four primary corporate personas: Traveling Employees, Line Managers, Finance Officers, and System Administrators. **[Verified]**

- **Project Type**: Full-Stack Web Application consisting of an ASP.NET Core layered REST API service, an Angular Single Page Application with Server-Side Rendering (SSR), and a relational Microsoft SQL Server database. **[Verified: Program.cs, package.json, database/VoyageHub_Schema.sql]**
- **Technology Stack & Versions**:
  - Runtime & Language: .NET 10 (`net10.0`), C# 13. **[Verified: VoyageHub.Api.csproj]**
  - Web API Framework: ASP.NET Core 10 (`Microsoft.NET.Sdk.Web`). **[Verified: VoyageHub.Api.csproj]**
  - ORM & Database Access: Microsoft Entity Framework Core 10 with SQL Server provider (`Microsoft.EntityFrameworkCore.SqlServer` v10.0.11, `Microsoft.EntityFrameworkCore.Design` v10.0.11). **[Verified: VoyageHub.Data.csproj, VoyageHub.Api.csproj]**
  - Cryptography / Password Hashing: BCrypt.Net-Next v4.2.0. **[Verified: VoyageHub.Business.csproj]**
  - JWT Authentication: `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.11, `System.IdentityModel.Tokens.Jwt` v7.7.1. **[Verified: VoyageHub.Api.csproj, VoyageHub.Business.csproj]**
  - API Documentation: Swashbuckle / OpenAPI (`Microsoft.AspNetCore.OpenApi` v10.0.11, `Swashbuckle.AspNetCore` v10.3.1). **[Verified: VoyageHub.Api.csproj]**
  - Frontend Client: Angular 21 (`@angular/core` v21.2.0), Angular SSR (`@angular/ssr` v21.2.21), Express 5 (`express` v5.1.0), RxJS v7.8.0. **[Verified: VoyageHub-Frontend/package.json]**
  - Frontend Build & Test: Angular CLI v21.2.21, TypeScript v5.9.2, Vitest v4.0.8, `@vitest/coverage-v8` v4.1.12. **[Verified: VoyageHub-Frontend/package.json]**
  - Database Engine: Microsoft SQL Server (`AngularAuthDb`). **[Verified: database/VoyageHub_Schema.sql]**
- **Current Maturity**: **Production-Ready Core Full-Stack Application**. Complete end-to-end CRUD, transactional database schema, role-based JWT Bearer authentication, and workflows exist and compile without errors. Backend is backed by 77 automated xUnit tests (83.1% coverage on new code); frontend is backed by 49 Vitest unit tests (100.0% coverage on new code); both projects pass the local SonarQube Clean as You Code Quality Gate with 0 open violations and 0.0% code duplication. **[Verified: dotnet test, npm test, SonarQube API]**

---

## 2. Constitution (Rules the Project Follows)

| Rule | Specification & Convention | Evidence / Source File | Status |
| :--- | :--- | :--- | :--- |
| **Backend Layered Architecture** | Strict 3-tier layering: Presentation (`VoyageHub.Api`), Business logic & DTOs (`VoyageHub.Business`), Data access & EF Core models (`VoyageHub.Data`). | `VoyageHub.slnx` | Verified |
| **Backend Dependency Direction** | `Api` references `Business` and `Data`; `Business` references `Data`; `Data` has no dependencies on other solution projects. | `VoyageHub.Api.csproj`, `VoyageHub.Business.csproj` | Verified |
| **Data Layer Abstraction** | Generic repository interface `IRepository<T>` extended by specialized repository interfaces (`ITravelRequestRepository`, `IApprovalStepRepository`, `IExpenseRepository`, `IUserRepository`, etc.) backed by `AppDbContext`. | `IRepository.cs`, `Repository.cs` | Verified |
| **DI Registration Lifetime** | Repositories and business services are registered with `Scoped` lifetime, sharing one `AppDbContext` instance per HTTP request. | `Program.cs` | Verified |
| **Frontend State & Reactivity** | Authentication and session state are managed via Angular Signals (`signal`, `computed`) in `AuthService`. | `auth.service.ts` | Verified |
| **Frontend Component Structure** | Standalone components organized by feature module (`admin/`, `analytics/`, `approvals/`, `dashboard/`, `finance/`, `requests/`, `support/`, `settings/`). | `app.routes.ts` | Verified |
| **Route Protection** | Role-based authorization guard `roleGuard(['RoleName'])` guards protected routes on client side. | `role.guard.ts` | Verified |
| **JSON Casing & Serialization** | API responses serialize JSON property names using `CamelCase`. Client TypeScript models strictly mirror backend camelCase DTOs. | `Program.cs`, `voyage.service.ts` | Verified |
| **Database Table Naming** | Relational tables and constraints strictly follow uppercase naming conventions (`USERS`, `DEPARTMENTS`, `TRAVEL_REQUESTS`, `APPROVAL_STEPS`, `ITINERARY_SEGMENTS`, `STAYS`, `EXPENSES`, `TRAVEL_POLICIES`, `AUDIT_LOGS`, `SUPPORT_TICKETS`). | `AppDbContext.cs`, `database/VoyageHub_Schema.sql` | Verified |
| **Global Usings Pattern** | Common namespace imports are centralized in `GlobalUsings.cs` in each backend project. | `GlobalUsings.cs` | Verified |
| **Async & Cancellation** | Asynchronous service and repository methods accept an optional `CancellationToken` defaulting to `default`. | `ITravelRequestService.cs`, `IExpenseService.cs` | Verified |
| **Audit Logging Pattern** | State-altering actions trigger audit log records inserted into `AUDIT_LOGS` table via `IAuditService.LogAsync()`. | `TravelRequestService.cs`, `ApprovalService.cs`, `ExpenseService.cs`, `EmployeeService.cs` | Verified |
| **Intentional Backdoor Shortcut (RETIRED)** | Formerly permitted bypass passwords in AuthService; retired in favor of strict BCrypt verification. | `AuthService.cs` | Retired |
| **Dynamic Database Aggregations** | Analytics and dashboard metrics query real database entities via EF Core with fallback values only for cold-start demo resiliency. | `TravelRequestRepository.cs`, `ExpenseRepository.cs`, `AnalyticsService.cs` | Verified |
| **Caller Identity from JWT Token** | User identity and roles are derived directly from the authenticated JWT token claims via `ICurrentUserService` rather than trusting client parameters. | `CurrentUserService.cs`, `TravelRequestsController.cs`, `ApprovalsController.cs` | Verified |

---

## 3. Functional Requirements (As Built)

### Authentication & User Profiles

| ID | Requirement Statement | Status | Source Files | Confidence |
| :--- | :--- | :--- | :--- | :--- |
| **FR-AUTH-01** | The system shall authenticate users with email and password, returning profile information and a signed JWT token upon success. | Implemented | `AuthController.cs`, `AuthService.cs`, `auth.service.ts` | Verified |
| **FR-AUTH-02** | The system shall permit bypass passwords during credential verification for testing. | **Retired** | `AuthService.cs` (Bypasses removed in favor of strict BCrypt verification) | Verified |
| **FR-AUTH-03** | The system shall list pre-seeded user personas for instant demo persona switching in UI. | Implemented | `AuthController.cs`, `auth.service.ts` | Verified |
| **FR-AUTH-04** | The system shall update a user's display name and preferences while persisting changes to the database. | Implemented | `AuthController.cs`, `AuthService.cs` | Verified |
| **FR-AUTH-05** | The system shall allow users to change passwords by validating current credentials and writing new BCrypt hashes. | Implemented | `AuthController.cs`, `AuthService.cs` | Verified |
| **FR-AUTH-06** | The system shall support self-service password reset requests via password reset tokens. | Implemented | `AuthController.cs`, `AuthService.cs` | Verified |

### Travel Requests & Workflows

| ID | Requirement Statement | Status | Source Files | Confidence |
| :--- | :--- | :--- | :--- | :--- |
| **FR-TRV-01** | The system shall list travel requests filtered by status, employee ID, and search keyword, scoped by token role. | Implemented | `TravelRequestsController.cs`, `TravelRequestRepository.cs`, `request-list.ts` | Verified |
| **FR-TRV-02** | The system shall calculate dashboard KPI metrics (drafts, pending, upcoming trips, pending reimbursement totals). | Implemented | `TravelRequestsController.cs`, `TravelRequestService.cs`, `dashboard.ts` | Verified |
| **FR-TRV-03** | The system shall retrieve full travel request details including itineraries, stays, expenses, and approval steps. | Implemented | `TravelRequestsController.cs`, `TravelRequestService.cs`, `request-detail.ts` | Verified |
| **FR-TRV-04** | The system shall create travel requests as Draft or In Review, auto-checking budget limits against department policies. | Implemented | `TravelRequestsController.cs`, `TravelRequestService.cs`, `new-request.ts` | Verified |
| **FR-TRV-05** | The system shall submit draft travel requests into "Manager Review" and instantiate sequential approval steps. | Implemented | `TravelRequestsController.cs`, `TravelRequestService.cs` | Verified |
| **FR-TRV-06** | The system shall permit cancelling active travel requests and record an audit log event. | Implemented | `TravelRequestsController.cs`, `TravelRequestService.cs` | Verified |
| **FR-TRV-07** | The system shall add itinerary segments and hotel stay reservations to existing travel requests. | Implemented | `TravelRequestsController.cs`, `TravelRequestService.cs` | Verified |
| **FR-TRV-08** | The system shall close completed travel requests, transitioning status to "Completed". | Implemented | `TravelRequestsController.cs`, `TravelRequestService.cs` | Verified |

### Approvals & Escalations

| ID | Requirement Statement | Status | Source Files | Confidence |
| :--- | :--- | :--- | :--- | :--- |
| **FR-APPR-01** | The system shall display approval queue items filtered by token caller identity and role (Manager / Finance). | Implemented | `ApprovalsController.cs`, `ApprovalService.cs`, `approval-queue.ts` | Verified |
| **FR-APPR-02** | The system shall process approval decisions: advancing Manager approval to Finance Review, finalizing Finance approval to Approved, or rejecting requests and cancelling remaining steps. | Implemented | `ApprovalsController.cs`, `ApprovalService.cs` | Verified |

### Expenses & Reimbursements

| ID | Requirement Statement | Status | Source Files | Confidence |
| :--- | :--- | :--- | :--- | :--- |
| **FR-EXP-01** | The system shall retrieve pending expense claims filtered by category and high-priority amounts ($\ge 10,000$ INR). | Implemented | `ExpensesController.cs`, `ExpenseService.cs`, `expense-review.ts` | Verified |
| **FR-EXP-02** | The system shall submit expense claims attached to a travel request, validating non-negative amounts. | Implemented | `ExpensesController.cs`, `ExpenseService.cs` | Verified |
| **FR-EXP-03** | The system shall approve ("Reimbursed") or reject ("Rejected") individual expense claims with audit trail attribution. | Implemented | `ExpensesController.cs`, `ExpenseService.cs` | Verified |
| **FR-EXP-04** | The system shall process batch reimbursement payouts for multiple selected pending expense claims. | Implemented | `ExpensesController.cs`, `ExpenseService.cs` | Verified |
| **FR-EXP-05** | The system shall support uploading and streaming receipt proof files (.pdf, .jpg, .jpeg, .png, .webp) up to 15MB. | Implemented | `ExpensesController.cs`, `ExpenseService.cs` | Verified |

### Travel Policies & Administration

| ID | Requirement Statement | Status | Source Files | Confidence |
| :--- | :--- | :--- | :--- | :--- |
| **FR-POL-01** | The system shall retrieve all department travel policies with budget limits and allowed travel classes. | Implemented | `PoliciesController.cs`, `PolicyService.cs`, `policy-config.ts` | Verified |
| **FR-POL-02** | The system shall update policy limits, flight classes, and enforcement parameters for an existing policy ID. | Implemented | `PoliciesController.cs`, `PolicyService.cs` | Verified |
| **FR-POL-03** | The system shall create travel policies for departments, preventing duplicate policies per department. | Implemented | `PoliciesController.cs`, `PolicyService.cs` | Verified |
| **FR-POL-04** | The system shall delete a department travel policy by ID while preserving the department record. | Implemented | `PoliciesController.cs`, `PolicyService.cs` | Verified |

### Employee Directory Management

| ID | Requirement Statement | Status | Source Files | Confidence |
| :--- | :--- | :--- | :--- | :--- |
| **FR-EMP-01** | The system shall list all employees with search and department filtering for Admin users. | Implemented | `EmployeesController.cs`, `EmployeeService.cs`, `employee-management.ts` | Verified |
| **FR-EMP-02** | The system shall create, update, and delete employee profiles with department association and manager assignment. | Implemented | `EmployeesController.cs`, `EmployeeService.cs` | Verified |
| **FR-EMP-03** | The system shall process batch employee CSV uploads with email regex validation, error reporting, and audit logging. | Implemented | `EmployeesController.cs`, `EmployeeService.cs` | Verified |
| **FR-EMP-04** | The system shall provide a standard downloadable CSV employee template. | Implemented | `EmployeesController.cs` | Verified |

### Support Desk & Audit Logging

| ID | Requirement Statement | Status | Source Files | Confidence |
| :--- | :--- | :--- | :--- | :--- |
| **FR-SUP-01** | The system shall list support tickets filtered by status, user ID, or search query. | Implemented | `SupportController.cs`, `SupportService.cs`, `support.ts` | Verified |
| **FR-SUP-02** | The system shall create support tickets with generated codes (`#TICK-xxxx`), linking optional travel requests. | Implemented | `SupportController.cs`, `SupportService.cs` | Verified |
| **FR-SUP-03** | The system shall update ticket resolution status, notes, and assigned agents. | Implemented | `SupportController.cs`, `SupportService.cs` | Verified |
| **FR-AUD-01** | The system shall retrieve audit trail logs filtered by severity and entity type. | Implemented | `AuditLogsController.cs`, `AuditService.cs`, `audit-logs.ts` | Verified |
| **FR-ANL-01** | The system shall provide spend analytics, category breakdown, cost reports, and employee history. | Implemented | `AnalyticsController.cs`, `AnalyticsService.cs`, `analytics-reports.ts` | Verified |

---

## 4. Setup and Verification

### Prerequisites
- .NET 10 SDK (`net10.0`). **[Verified]**
- Node.js 20+ LTS and npm 10+. **[Verified]**
- Microsoft SQL Server / Azure SQL database. **[Verified]**

### Build and Run Commands
```powershell
# 1. Apply database schema
.\database\run_migration.ps1 -Server ".\SQLEXPRESS06" -Database "AngularAuthDb"

# 2. Run backend API
cd VoyageHub-Backend\VoyageHub.Api
dotnet run

# 3. Run frontend client
cd VoyageHub-Frontend
npm install
npm start
```

### Test Commands
```powershell
# Run backend tests (77 tests)
dotnet test VoyageHub-Backend\VoyageHub.Tests\VoyageHub.Tests.csproj

# Run frontend tests (49 tests)
cd VoyageHub-Frontend
npm test
```
