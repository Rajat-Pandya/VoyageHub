# Contributing to VoyageHub

Thank you for contributing to VoyageHub. This document outlines development standards, branch naming conventions, commit formats, and pre-flight verification checks.

---

## 1. Local Developer Setup

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+ LTS](https://nodejs.org/) & npm 10+
- [Angular CLI 21](https://angular.dev/tools/cli) (`npm install -g @angular/cli`)
- [Microsoft SQL Server](https://www.microsoft.com/sql-server) or SQL Server Express

### Setup Steps
1. Clone the repository and navigate to root:
   ```bash
   git clone https://github.com/<your-username>/VoyageHub.git
   cd VoyageHub
   ```
2. Initialize local database schema and seed data:
   ```powershell
   .\database\run_migration.ps1 -Server ".\SQLEXPRESS06" -Database "AngularAuthDb"
   ```
3. Start the API backend:
   ```powershell
   cd VoyageHub-Backend\VoyageHub.Api
   dotnet restore
   dotnet run
   ```
   *(API runs at `http://localhost:5062`, Swagger at `http://localhost:5062/swagger`)*
4. In a second terminal, start the Angular frontend:
   ```powershell
   cd VoyageHub-Frontend
   npm install
   npm start
   ```
   *(Client runs at `http://localhost:4200`)*

---

## 2. Branching & Commit Conventions

### Branch Naming
- Feature branches: `feature/<descriptive-name>` (e.g. `feature/employee-csv-export`)
- Bugfix branches: `fix/<issue-name>` (e.g. `fix/receipt-mime-check`)

### Conventional Commits
All commit messages must follow the [Conventional Commits](https://www.conventionalcommits.org/) format:

```
<type>(<scope>): <short imperative description> [rule key if applicable]
```

- Allowed types: `feat`, `fix`, `docs`, `test`, `refactor`, `perf`, `style`.
- Example: `fix(api): bind file upload directly to model parameter (csharpsquid:S6932)`
- **Do not squash commits** when merging; preserve atomic history per fix or rule.

---

## 3. Pre-Pull Request Verification

Before opening a pull request, run and verify all checks locally:

### 1. Run All Tests
```powershell
# Run backend test suite (77 tests must pass)
dotnet test VoyageHub-Backend\VoyageHub.Tests\VoyageHub.Tests.csproj

# Run frontend test suite (49 tests must pass)
cd VoyageHub-Frontend
npm test

# Verify production build compilation
npm run build
```

### 2. Run SonarQube Clean as You Code Verification
Ensure both projects pass the local SonarQube Quality Gate:
- Coverage on new code must be $\ge 80.0\%$.
- Duplications on new code must be $\le 3.0\%$.
- 0 new open violations.
- Refer to [docs/CODE-QUALITY.md](docs/CODE-QUALITY.md) for local scanner execution steps.

---

## 4. Ground Rules & Code Standards

- **NEVER Add Suppressions**: No `NOSONAR`, no `#pragma warning disable`, no `[SuppressMessage]`, and no `eslint-disable`. Fix the root cause instead of suppressing warnings.
- **NEVER Lower Quality Gate Thresholds**: Do not modify rule sets, exclusions, or threshold settings.
- **NEVER Commit Secrets**: Do not stage real tokens (`squ_`), production connection strings, or cloud passwords. Keep secrets in .NET user-secrets or environment variables.
- **Token-Derived Identity**: Always resolve caller user ID and roles via `ICurrentUserService` rather than trusting client-supplied route or body IDs.
