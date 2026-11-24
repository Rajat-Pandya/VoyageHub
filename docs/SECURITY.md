# VoyageHub Security Architecture & Role Matrix

This document outlines the authentication, authorization, cryptography, secrets management, input validation, and security boundaries implemented in VoyageHub.

---

## 1. Authentication Architecture

### Password Hashing
- **Algorithm**: BCrypt (`BCrypt.Net-Next` v4.2.0).
- **Work Factor**: 11 (standard work factor providing robust brute-force resistance while keeping login latency $< 150\text{ms}$).
- **Salt Generation**: Cryptographically secure per-user salt generated automatically during hashing.
- **Verification**: Strict verification via `BCrypt.Net.BCrypt.Verify(password, storedHash)`. All legacy or testing backdoors have been permanently retired.

### JWT Bearer Token Specification
- **Algorithm**: HMAC-SHA256 (`SecurityAlgorithms.HmacSha256`).
- **Signature Key**: 256-bit+ symmetric key passed via `Jwt:Key` (enforced $\ge 32$ characters on production startup).
- **Token Claims**:
  - `sub`: Caller GUID (`User.Id`).
  - `email`: Caller email address.
  - `name`: Caller display name.
  - `role`: Role claim (`Employee`, `Manager`, `Finance`, `Admin`).
  - `departmentId`: Department GUID.
- **Expiration**: Configurable via `Jwt:ExpiryMinutes` (defaults to 1440 minutes / 24 hours).

---

## 2. Authorization & Role Matrix

VoyageHub employs strict Role-Based Access Control (RBAC) enforced on the server side using ASP.NET Core `[Authorize(Roles = "...")]` attributes.

| Feature Domain / Action | Anonymous | Employee | Line Manager | Finance Officer | System Admin |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Login & Password Reset** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Download CSV Template** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **View Own Dashboard & Stats** | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Create & Submit Travel Requests** | ❌ | ✅ | ✅ | ❌ | ✅ |
| **View Own Travel Requests** | ❌ | ✅ | ✅ | ✅ | ✅ |
| **View Team Requests (Direct Reports)**| ❌ | ❌ | ✅ | ✅ | ✅ |
| **Cancel Own Travel Request** | ❌ | ✅ | ✅ | ❌ | ✅ |
| **Manager Approval Queue** | ❌ | ❌ | ✅ | ❌ | ✅ |
| **Finance Approval Queue** | ❌ | ❌ | ❌ | ✅ | ✅ |
| **Submit Expense Reimbursement Claim** | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Upload Receipt Proof Document** | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Review & Approve Expense Claims** | ❌ | ❌ | ❌ | ✅ | ✅ |
| **Execute Batch Expense Payouts** | ❌ | ❌ | ❌ | ✅ | ✅ |
| **View Department Travel Policies** | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Create, Update, Delete Policies** | ❌ | ❌ | ❌ | ❌ | ✅ |
| **View Full System Audit Logs** | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Manage Employee Directory (CRUD/CSV)**| ❌ | ❌ | ❌ | ❌ | ✅ |
| **View High-Level Spend Analytics** | ❌ | ❌ | ✅ | ✅ | ✅ |
| **Open Support Desk Inquiries** | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Resolve & Update Support Tickets** | ❌ | ❌ | ✅ | ✅ | ✅ |

---

## 3. Token-Derived Identity Scoping (IDOR Prevention)

To prevent Insecure Direct Object Reference (IDOR) attacks, client-supplied user ID parameters were eliminated from query strings and route parameters across domain services:

- **Caller Identity Extraction**: `CurrentUserService` resolves the caller's identity directly from `HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)`.
- **Query Scoping**:
  - `TravelRequestRepository`: Queries executed by an `Employee` automatically append `WHERE EmployeeId = @callerId`.
  - Line Managers are scoped to their own requests plus employees whose `ManagerId` matches the caller's ID.
  - Approval queue queries evaluate `ApproverId = @callerId` or match the caller's active role stage.
- **Audit Attribution**: All changes logged in `AUDIT_LOGS` resolve `ActorId` and `ActorName` from the cryptographic token claims, ensuring tamper-proof audit trails.

---

## 4. CORS Policy & Environment Isolation

Cross-Origin Resource Sharing is strictly controlled in `Program.cs`:

- **Development**: Reads `Cors:AllowedOrigins` from `appsettings.json`, allowing `http://localhost:4200`, `http://127.0.0.1:4200`, and `https://localhost:4200` with credentials.
- **Production**: When `ASPNETCORE_ENVIRONMENT != Development`, the API reads origins strictly from environment variables (`Cors__AllowedOrigins__0`, etc.). If configured, localhost origins are excluded by default, preventing unauthorized local scripts from accessing cloud APIs.

---

## 5. Secrets Management & Fail-Fast Startup

VoyageHub enforces zero-secret exposure across the codebase:

1. **No Hardcoded Secrets**: Secrets and real connection strings are excluded from source code and `appsettings.json`.
2. **Local User Secrets**: Development signing keys and local credentials reside in .NET user-secrets (`dotnet user-secrets set "Jwt:Key" "..."`).
3. **Startup Fail-Fast Validation**: On application launch in non-Development environments, `Program.cs` validates:
   - `ConnectionStrings:DefaultConnection` must not be null or whitespace.
   - `Jwt:Key` must not be null or whitespace and must have a length of at least 32 characters (256 bits).
   - If validation fails, the API terminates immediately with a descriptive error message without printing or logging secret values.
4. **Cloud Seed Safety**: In `database/VoyageHub_Schema.azure.sql`, seeded user passwords are set to the disabled string `'DISABLED-SET-VIA-DEPLOYMENT-MD'`, requiring manual activation via `tools/HashPassword`.

---

## 6. Input Validation & Attack Mitigations

- **ReDoS Prevention**: Regex operations in `EmployeeService.cs` (e.g. employee email validation) specify an explicit execution timeout (`TimeSpan.FromMilliseconds(250)`) and compiled options to prevent catastrophic backtracking and Denial of Service.
- **File Upload Whitelisting**:
  - Bound directly to `IFormFile? file` parameters to prevent parameter tampering.
  - Whitelists only approved document and image MIME extensions: `.pdf`, `.jpg`, `.jpeg`, `.png`, `.webp`.
  - Enforces a strict 15MB file size limit (`15 * 1024 * 1024` bytes).
  - Sanitizes stored file names using unique GUID prefixes (`$"{Guid.NewGuid():N}_{cleanName}{ext}"`) to prevent path traversal.
- **SQL Injection Prevention**: All database interactions use Entity Framework Core 10 LINQ queries and parameterized commands. No raw dynamic string concatenation is used for T-SQL execution.

---

## 7. Verified Known Limitations

| Limitation | Impact | Recommended Production Mitigation |
| :--- | :--- | :--- |
| **Client Token Storage in `localStorage`** | Tokens stored in browser `localStorage` are vulnerable to Cross-Site Scripting (XSS) if malicious scripts execute in the client context. | Transition to `HttpOnly`, `SameSite=Strict`, `Secure` cookies with CSRF token exchange. |
| **No Token Revocation / Refresh Mechanism** | Once issued, a JWT is valid until expiration (`Jwt:ExpiryMinutes`). A compromised token cannot be selectively invalidated before expiry. | Implement a Redis-backed token revocation denylist and short-lived access tokens (15m) with rotating refresh tokens. |
| **Local Demo Seed Hashes** | `database/VoyageHub_Schema.sql` contains default BCrypt hashes for quick offline onboarding. | Use `database/VoyageHub_Schema.azure.sql` exclusively for hosted deployments and rotate all user credentials immediately. |
| **Local Disk Receipt Storage** | Receipt files are stored in the server's local file system (`App_Data/Receipts`). | Move to cloud object storage (Azure Blob Storage or AWS S3) with pre-signed URLs for scaled multi-instance deployments. |
