# VoyageHub Git Workflow & Version Control Standards

This document establishes the branching model, commit conventions, release tagging, and verification procedures for VoyageHub.

---

## 1. Branching Strategy

VoyageHub follows a disciplined feature-branch workflow based on `master`:

```
master (stable, deployable)
  │
  ├── feature/deploy-config       ──> merged
  ├── feature/ui-consistency      ──> merged
  ├── feature/sonar-quality-gate  ──> verified & merged
  └── feature/documentation       ──> current active branch
```

### Branch Roles
- **`master`**: Production-ready, stable codebase. All commits on `master` must pass unit tests and SonarQube quality gates.
- **`feature/<topic>`**: Dedicated branches for discrete feature milestones (e.g. `feature/deploy-config`, `feature/ui-consistency`, `feature/sonar-quality-gate`, `feature/documentation`).
- **`fix/<issue>`**: Targeted branches for addressing isolated bugs or quality smells.

---

## 2. Conventional Commits Standard

VoyageHub enforces the [Conventional Commits](https://www.conventionalcommits.org/) specification across all git history:

```
<type>(<scope>): <description> [optional issue/rule key]
```

### Allowed Types
- **`feat`**: A new user-facing feature or API capability.
- **`fix`**: A bug fix or defect remediation.
- **`docs`**: Documentation updates or additions only.
- **`test`**: Adding missing tests, expanding test fixtures, or updating coverage data.
- **`refactor`**: Code restructuring without altering external behavior.
- **`perf`**: A code change that improves execution performance.
- **`style`**: Changes that do not affect code logic (whitespace, formatting, CSS tokens).

### Scopes Used in VoyageHub
- `api`, `auth`, `admin`, `expenses`, `employees`, `policies`, `requests`, `approvals`, `support`, `analytics`, `tests`, `deploy`, `config`.

### Examples from Repository History
- `fix(employees): specify regex execution timeout to prevent ReDoS (csharpsquid:S6444)`
- `refactor(expenses): extract SaveProofFileAsync to eliminate duplicated upload logic`
- `fix(admin): provide compare function for string array sorting (typescript:S2871)`
- `test(backend): add unit tests for EmployeeService, EmployeesController, and ExpensesController`
- `docs(readme): update README with verified tech stack, ports, quickstart, and metrics`

---

## 3. Tags & Milestones

Release tags are created at key stability checkpoints:
- **`pre-release-prep`**: Stable checkpoint capturing the initial deployment readiness dry-run prior to UI standardization and Quality Gate hardening.

To create a new annotated milestone tag:
```powershell
git tag -a v1.0.0-rc1 -m "Release Candidate 1 with passing Sonar Quality Gates"
```

---

## 4. Pre-Pull Request Verification Checklist

Before creating a pull request or merging into `master`, verify the following gates:

- [ ] **1. Clean Working Tree**: `git status` reports nothing unstaged or untracked.
- [ ] **2. Backend Tests Pass**:
  ```powershell
  dotnet test VoyageHub-Backend\VoyageHub.Tests\VoyageHub.Tests.csproj
  ```
  *(All 77 tests must pass with 0 failures.)*
- [ ] **3. Frontend Tests & Build Pass**:
  ```powershell
  cd VoyageHub-Frontend
  npm test
  npm run build
  ```
  *(All 49 Vitest tests pass and production build compiles with zero TypeScript errors.)*
- [ ] **4. SonarQube Clean as You Code Quality Gate**:
  - Code coverage on new code $\ge 80.0\%$.
  - Duplicated lines on new code $\le 3.0\%$.
  - 0 new open violations (Security, Reliability, Maintainability).
- [ ] **5. No Suppressions**: No `NOSONAR`, `#pragma warning disable`, `[SuppressMessage]`, or `eslint-disable` added to bypass checks.
- [ ] **6. No Plaintext Secrets**: Ensure no API keys, tokens (`squ_`), or real connection strings are staged.
