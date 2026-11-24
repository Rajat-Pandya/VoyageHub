# ADR-001: JWT Authentication with BCrypt Password Hashing

## Status
Accepted

## Context
VoyageHub requires a secure, stateless authentication and authorization architecture to support cross-origin communication between the Angular Single Page Application and the ASP.NET Core 10 Web API across separate domains (e.g. Azure Static Web Apps and Azure App Service). Initial early prototypes explored persona switching and in-memory mock identities, which could not support real role enforcement or production deployment.

## Decision
1. **Password Hashing**: Adopt BCrypt (`BCrypt.Net-Next`) with a work factor of 11 for password hashing and verification. Retire all bypass passwords and backdoor shortcuts.
2. **Stateless JWT Tokens**: Issue HMAC-SHA256 signed JSON Web Tokens upon successful authentication (`/api/Auth/login`). The token encapsulates verified claims: `sub` (User GUID), `email`, `name`, `role`, and `departmentId`.
3. **Claims-Based Context**: Implement `ICurrentUserService` in the API to resolve caller identity and roles strictly from verified token claims on every HTTP request, preventing Insecure Direct Object Reference (IDOR) attacks.

## Consequences
- **Positive**: Stateless authentication scales horizontally across serverless or containerized instances without sticky sessions or centralized session stores. Role-based guards (`[Authorize(Roles = "...")]`) can be evaluated without database lookups.
- **Negative / Trade-offs**: Tokens stored in browser `localStorage` are vulnerable to XSS if client execution is compromised. In standard JWT architectures, issued tokens cannot be revoked prior to expiration (`Jwt:ExpiryMinutes`) without maintaining a distributed denylist (e.g., in Redis).
