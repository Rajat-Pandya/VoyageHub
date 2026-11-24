# VoyageHub Code Quality & SonarQube Guide

This document details the local SonarQube setup, scan commands, token management, Clean as You Code Quality Gate criteria, and before/after metrics for VoyageHub.

---

## 1. Local SonarQube Setup via Docker

VoyageHub utilizes a local SonarQube Community Build instance to perform static analysis and code quality verification:

```powershell
# Run SonarQube Community Edition in Docker
docker run -d --name sonarqube -p 9000:9000 sonarqube:community
```

- **SonarQube Web Interface**: Accessible at `http://localhost:9000`.
- **Default Credentials**: `admin` / `admin` (prompts for password reset on first sign-in).

---

## 2. Token Configuration & Security

To avoid storing tokens in code or scanner properties files, generate a user analysis token in SonarQube (**User Profile** $\rightarrow$ **Security** $\rightarrow$ **Generate Token**) and store it in your Windows User environment:

```powershell
# 1. Save token securely in Windows User registry (do not print or commit token value)
[System.Environment]::SetEnvironmentVariable('SONARQUBE_TOKEN', '<your-token>', 'User')

# 2. In any active PowerShell terminal, load the token into the current session
$env:SONARQUBE_TOKEN = [System.Environment]::GetEnvironmentVariable('SONARQUBE_TOKEN', 'User')
```

---

## 3. Exact Scan Commands

> [!IMPORTANT]
> Sonar project keys are **case-sensitive**. Ensure you pass `VoyageHub-Backend` and `VoyageHub-Frontend` exactly as defined.

### VoyageHub-Backend (.NET 10 Web API)
```powershell
cd "VoyageHub-Backend"

# 1. Generate fresh coverage report
dotnet-coverage collect --output coverage.xml --output-format xml "dotnet test"

# 2. Run SonarScanner for .NET
dotnet sonarscanner begin `
  /k:"VoyageHub-Backend" `
  /d:sonar.host.url="http://localhost:9000" `
  /d:sonar.token="$($env:SONARQUBE_TOKEN)" `
  /d:sonar.cs.vscoveragexml.reportsPaths="coverage.xml"

dotnet build

dotnet sonarscanner end /d:sonar.token="$($env:SONARQUBE_TOKEN)"
```

### VoyageHub-Frontend (Angular 21 SPA)
```powershell
cd "VoyageHub-Frontend"

# 1. Run Vitest tests to emit LCOV report to coverage/AngularBasics/lcov.info
npm test

# 2. Run sonarqube-scanner via npx
$env:SONAR_HOST_URL = "http://localhost:9000"
$env:SONAR_TOKEN = $env:SONARQUBE_TOKEN

npx --yes sonarqube-scanner
```

---

## 4. Optional SonarQube MCP Server Configuration

If working with an AI coding assistant supporting the Model Context Protocol (MCP), configure the SonarQube MCP server in your local settings without embedding plaintext tokens:

```json
{
  "mcpServers": {
    "sonarqube": {
      "command": "npx",
      "args": ["-y", "@sonarsource/sonarqube-mcp-server"],
      "env": {
        "SONARQUBE_URL": "http://localhost:9000",
        "SONARQUBE_TOKEN": "${env:SONARQUBE_TOKEN}"
      }
    }
  }
}
```

---

## 5. Before & After Quality Gate Results Table

All findings were resolved on the `feature/sonar-quality-gate` branch without code suppressions (`NOSONAR`, `#pragma`, `[SuppressMessage]`, `eslint-disable`) or lowered thresholds:

### VoyageHub-Frontend (Angular 21)

| Metric / Dimension | Initial State (Phase 1) | Final State (Verified) | Quality Gate Threshold | Outcome |
| :--- | :---: | :---: | :---: | :---: |
| **Overall Quality Gate** | **ERROR** | **OK** | All conditions passing | **PASSED** |
| **Coverage on New Code** | `0.0%` | **`100.0%`** | $\ge 80.0\%$ | **PASSED** |
| **Duplications on New Code** | `0.13%` | **`0.0%`** | $\le 3.0\%$ | **PASSED** |
| **New Open Violations** | `1` | **`0`** | $= 0$ | **PASSED** |
| **Reliability Rating on New Code** | D | **A (`1.0`)** | A | **PASSED** |
| **Security Rating on New Code** | C | **A (`1.0`)** | A | **PASSED** |
| **Maintainability Rating on New Code** | A | **A (`1.0`)** | A | **PASSED** |
| **Hotspots to Review** | 0 | **0** | 0 to review | **PASSED** |

### VoyageHub-Backend (ASP.NET Core 10)

| Metric / Dimension | Initial State (Phase 1) | Final State (Verified) | Quality Gate Threshold | Outcome |
| :--- | :---: | :---: | :---: | :---: |
| **Overall Quality Gate** | **ERROR** | **OK** | All conditions passing | **PASSED** |
| **Coverage on New Code** | `0.0%` | **`83.1%`** | $\ge 80.0\%$ | **PASSED** |
| **Duplications on New Code** | `5.52%` | **`0.0%`** | $\le 3.0\%$ | **PASSED** |
| **New Open Violations** | `16` | **`0`** | $= 0$ | **PASSED** |
| **Reliability Rating on New Code** | A | **A (`1.0`)** | A | **PASSED** |
| **Security Rating on New Code** | A | **A (`1.0`)** | A | **PASSED** |
| **Maintainability Rating on New Code** | A | **A (`1.0`)** | A | **PASSED** |
| **Hotspots to Review** | 0 | **0** | 0 to review | **PASSED** |

---

## 6. Summary of Key Fixes

1. **ReDoS Timeout (`csharpsquid:S6444`)**: Specified explicit `TimeSpan.FromMilliseconds(250)` timeout on regular expressions in `EmployeeService.cs`.
2. **File Upload Model Binding (`csharpsquid:S6932`)**: Bound file uploads directly to action parameters (`IFormFile? file`) in `EmployeesController.cs` and `ExpensesController.cs`.
3. **Duplication Elimination**: Extracted `SaveProofFileAsync` helper in `ExpensesController.cs`, eliminating 36.2% code duplication.
4. **EF Core Translatable LINQ**: Replaced in-memory `.ToLower()` operations in `EmployeeService.cs` with `EF.Functions.Like`.
5. **Array Sort Comparison (`typescript:S2871`)**: Provided explicit string compare function in `employee-management.ts`.
6. **LCOV Report Generation**: Installed `@vitest/coverage-v8` in `VoyageHub-Frontend` and enabled `sonar.javascript.lcov.reportPaths`.
