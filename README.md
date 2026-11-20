# VoyageHub — Corporate Travel & Expense Management System

[![CI](https://github.com/Rajat-Pandya/VoyageHub/actions/workflows/ci.yml/badge.svg)](https://github.com/Rajat-Pandya/VoyageHub/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular 21](https://img.shields.io/badge/Angular-21-DD0031?logo=angular)](https://angular.dev/)
[![EF Core 10](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC292B?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)

**VoyageHub** is an enterprise-grade corporate travel and expense management application designed to coordinate employee business travel requests, multi-tier approvals, expense claim reimbursements, departmental policy compliance, and internal corporate travel desk workflows.

> [!WARNING]
> ### Security Notice
> Authentication in this repository is designed for demo and evaluation purposes. While password hashing uses BCrypt (work factor 12) and JWT token generation is supported, complete token authentication and role authorization across all endpoints are part of the production roadmap (GAP-002). Persona switching and seed endpoints are accessible without multi-factor auth. **Do NOT expose this application directly to the public internet or load real corporate financial or personal data.**

---

<!-- 
### Application Preview
![VoyageHub Dashboard](Assets/dashboard-preview.png)
-->

## Architecture Overview

VoyageHub follows a clean, layered architectural design separating the Angular Single Page Application (with SSR capabilities) from the ASP.NET Core backend REST services and SQL Server database.

```mermaid
flowchart TD
    Client["Angular 21 Client (SPA / SSR)"]

    subgraph Backend ["VoyageHub.Api (ASP.NET Core 10)"]
        Controllers["Controllers (Auth, TravelRequests, Approvals, Expenses, Policies, Support, Analytics)"]
        Swagger["OpenAPI / Swagger UI"]
    end

    subgraph Core ["VoyageHub.Business"]
        Services["Business Services and Workflow Logic"]
        Audit["Audit Trail Logging Engine"]
        DTOs["Data Transfer Objects (DTOs)"]
    end

    subgraph Data ["VoyageHub.Data (EF Core 10)"]
        AppDb["AppDbContext"]
        Repos["Generic and Custom Repositories"]
    end

    Database[("Microsoft SQL Server (AngularAuthDb)")]

    Client -->|REST / JSON (Port 5062)| Controllers
    Controllers --> Services
    Services --> Audit
    Services --> Repos
    Repos --> AppDb
    AppDb -->|T-SQL| Database
```

---

## User Personas and Workflows

| Persona | Primary Capabilities |
| :--- | :--- |
| **Employee** | Create and submit travel requests, link itinerary segments and hotel stays, claim out-of-pocket expenses, submit receipts, and open support tickets. |
| **Line Manager** | Review pending team travel requests, inspect itinerary details and policy compliance flags, approve or reject with comments. |
| **Finance Officer** | Second-level approval on manager-approved requests, review itemized expense claims, and execute single or batch reimbursements. |
| **System Admin** | Set department domestic and international budget limits, flight class rules, enforcement flags, view complete system audit logs, and manage accounts. |

---

## Tech Stack

- **Backend**:
  - ASP.NET Core 10 Web API (`net10.0`)
  - Entity Framework Core 10.0 (`Microsoft.EntityFrameworkCore.SqlServer` 10.0.11)
  - Password Hashing: `BCrypt.Net-Next` (work factor 12)
  - API Documentation: OpenAPI / Swagger (`Swashbuckle.AspNetCore`)
  - Testing: xUnit, Moq, and `Microsoft.EntityFrameworkCore.Sqlite` in-memory provider
- **Frontend**:
  - Angular 21 (`@angular/core` ^21.2.0)
  - Server-Side Rendering (SSR) via Express 5
  - Role-based Route Guards and JWT HTTP Interceptors
  - Testing: Vitest with Angular TestBed
- **Database**:
  - Microsoft SQL Server (LocalDB, Express, or Azure SQL Database)

---

## Repository Structure

```
.
├── .github/                   # GitHub Actions CI workflow
├── Assets/                    # Architecture diagrams, ER models, SRS documentation
├── database/                  # Database creation and seed data scripts
│   ├── VoyageHub_Schema.sql   # SQL Server schema, table constraints, and demo seed data
│   └── run_migration.ps1      # Parameterized setup script for local SQL Server
├── VoyageHub-Backend/         # .NET 10 Backend Solution
│   ├── VoyageHub.Api/         # REST Controllers, Middleware, Configuration
│   ├── VoyageHub.Business/    # Business services, validation, workflow logic, DTOs
│   ├── VoyageHub.Data/        # EF Core DbContext, entity models, repositories
│   ├── VoyageHub.Tests/       # Unit tests (xUnit + SQLite in-memory)
│   └── VoyageHub.slnx         # Solution file
├── VoyageHub-Frontend/        # Angular 21 Client Application
│   ├── src/app/admin/         # Admin policy management and audit logs
│   ├── src/app/approvals/     # Manager and Finance approval queues
│   ├── src/app/dashboard/     # Unified role-based dashboard
│   ├── src/app/finance/       # Expense claims review and batch payout
│   ├── src/app/requests/      # Travel request creation and itineraries
│   ├── src/app/support/       # Corporate desk support ticketing
│   └── src/app/services/      # Authentication, HTTP interceptors, Voyage API service
├── LICENSE                    # MIT License
└── README.md                  # Project documentation
```

---

## Quick Start Guide

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (v10.0.x or v11.0 preview)
- [Node.js 20+ LTS](https://nodejs.org/) & npm 10+
- [Microsoft SQL Server](https://www.microsoft.com/sql-server) (LocalDB, Express, or Developer edition)

---

### Step 1: Database Setup

1. Open SQL Server Management Studio (SSMS), Azure Data Studio, or PowerShell.
2. Execute the schema script:
   ```powershell
   .\database\run_migration.ps1 -Server "(localdb)\MSSQLLocalDB" -Database "AngularAuthDb"
   ```
   Or execute `database/VoyageHub_Schema.sql` directly in your SQL editor. This will:
   - Create the `AngularAuthDb` database.
   - Create all 9 domain tables (`USERS`, `DEPARTMENTS`, `TRAVEL_POLICIES`, `TRAVEL_REQUESTS`, `APPROVAL_STEPS`, `ITINERARY_SEGMENTS`, `STAYS`, `EXPENSES`, `SUPPORT_TICKETS`, `AUDIT_LOGS`).
   - Seed baseline departments, travel policies, and demo persona accounts.

---

### Step 2: Backend Configuration and Startup

1. Navigate to the API project directory:
   ```bash
   cd VoyageHub-Backend/VoyageHub.Api
   ```
2. Configure the database connection string using `dotnet user-secrets` (recommended) or set the `ConnectionStrings__DefaultConnection` environment variable:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=AngularAuthDb;Trusted_Connection=True;TrustServerCertificate=True;"
   ```
3. Restore dependencies and run the API:
   ```bash
   dotnet restore
   dotnet run
   ```
4. The API runs on `http://localhost:5062`. Access Swagger UI documentation at:
   ```
   http://localhost:5062/swagger
   ```

---

### Step 3: Frontend Startup

1. Open a new terminal and navigate to the frontend directory:
   ```bash
   cd VoyageHub-Frontend
   ```
2. Install dependencies:
   ```bash
   npm ci
   ```
3. Start the Angular development server:
   ```bash
   npm start
   ```
4. Open your browser and navigate to `http://localhost:4200`.

---

### Demo Accounts

The database seed provides ready-to-use persona accounts. All seeded demo accounts use the single demo password below:

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `vikram@voyagehub.internal` | `VoyageDemo2026!` |
| **Manager** | `priya@voyagehub.internal` | `VoyageDemo2026!` |
| **Finance** | `rohit@voyagehub.internal` | `VoyageDemo2026!` |
| **Employee** | `ananya@voyagehub.internal` | `VoyageDemo2026!` |

---

## Testing

### Backend Unit Tests (.NET 10 / xUnit)
The backend test suite runs with an in-memory SQLite provider (`Microsoft.EntityFrameworkCore.Sqlite`) requiring no external database:
```bash
dotnet test VoyageHub-Backend/VoyageHub.slnx
```
Covers authentication security (BCrypt enforcement, plaintext rejection), approval state transitions, policy duplication checks, travel request auto-flagging, expense reimbursements, and ticket code formatting.

### Frontend Unit Tests (Angular 21 / Vitest)
The frontend test suite runs via Vitest in headless, non-watch mode:
```bash
cd VoyageHub-Frontend
npm test -- --no-watch
```
Covers role route guards, application routing, dashboard metrics rendering, and authentication component workflows.

---

## Known Limitations and Roadmap

- **Token Authentication & RBAC (GAP-002)**: Complete JWT validation and role authorization attributes across all endpoints.
- **Audit Actor Resolution (GAP-007)**: Dynamic extraction of actor identities from authentication context.
- **Reporting Analytics (GAP-005, GAP-006)**: Complete live aggregation queries for empty-state reporting instead of fallback figures.
- **User Preferences (GAP-008)**: Database persistence for user notification settings and travel preferences.
- **Input Validation (GAP-009)**: Comprehensive FluentValidation rules for currency bounds, date limits, and character lengths.
- **Global Error Handling**: Centralized ASP.NET Core `ProblemDetails` exception handling middleware.
- **Pagination**: Server-side cursor/page-based querying for large request and audit histories.
- **Receipt Blob Storage**: Integration with Azure Blob Storage or AWS S3 for binary expense receipt uploads.
- **Accessibility (a11y)**: ARIA live regions and automated axe-core compliance checks.

---

## Author

- **Rajat Pandya** ([@Rajat-Pandya](https://github.com/Rajat-Pandya))

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
