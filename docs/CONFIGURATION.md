# VoyageHub Configuration Matrix

This document defines the configuration parameters for VoyageHub, their environment variable representations for cloud hosting, local sources, production sources, requirements, and safe example values.

## Configuration Settings Reference

| Setting Key | Environment Variable Form | Local Source | Production Source (Azure) | Required? | Safe Example Value | Description |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | `appsettings.Development.json` / `appsettings.json` | Azure App Service App Settings (`ConnectionStrings__DefaultConnection`) or Connection Strings (`DefaultConnection`) | **Yes** (Startup fail-fast in Production) | `Server=tcp:sql-voyagehub-free.database.windows.net,1433;Database=voyagehub-db;Authentication=Active Directory Default;TrustServerCertificate=False;` | Database connection string for SQL Server / Azure SQL. Overrides local default. |
| `Jwt:Key` | `Jwt__Key` | .NET User-Secrets (`dotnet user-secrets set "Jwt:Key" "..."`) | Azure App Service App Settings (`Jwt__Key`) | **Yes** (Startup fail-fast: must be >= 32 characters in Production) | `YourStrongRandom32PlusCharacterSecretKeyGoesHere!` | HMAC-SHA256 symmetric signing key for JWT authentication tokens. |
| `Jwt:Issuer` | `Jwt__Issuer` | `appsettings.json` (`VoyageHub.Api`) | Azure App Service App Settings (`Jwt__Issuer`) | Optional (Defaults to `VoyageHub.Api`) | `https://api-voyagehub.azurewebsites.net` | Token issuer validated during JWT token verification. |
| `Jwt:Audience` | `Jwt__Audience` | `appsettings.json` (`VoyageHub.Client`) | Azure App Service App Settings (`Jwt__Audience`) | Optional (Defaults to `VoyageHub.Client`) | `https://lemon-island-012345678.azurestaticapps.net` | Token audience validated during JWT token verification. |
| `Cors:AllowedOrigins` | `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1` | `appsettings.json` (`["http://localhost:4200", "http://127.0.0.1:4200", "https://localhost:4200"]`) | Azure App Service App Settings (`Cors__AllowedOrigins__0`) | **Yes** in Production | `https://lemon-island-012345678.azurestaticapps.net` | List of allowed CORS origins. In Production, default localhost origins are excluded unless explicitly specified. |
| `Swagger:Enabled` | `Swagger__Enabled` | `appsettings.Development.json` (`true`) | Azure App Service App Settings (`Swagger__Enabled`, defaults to `false`) | Optional (Defaults to `false` in Production) | `false` | Controls whether Swagger OpenAPI documentation and Swagger UI are enabled at `/swagger` and root `/` redirects. |
| `environment.apiUrl` (Frontend) | N/A (Build-time Angular token) | `src/environments/environment.ts` (`http://localhost:5062/api`) | `src/environments/environment.prod.ts` wired through `angular.json` `fileReplacements` | **Yes** for production static build | `https://api-voyagehub.azurewebsites.net/api` | Base API URL used by Angular HTTP services. Replaced at build time. |

---

## Notes on Configuration Resolution

1. **Hierarchy & Precedence**:
   In ASP.NET Core, configuration sources are resolved in the following priority order:
   1. Environment Variables (e.g. `ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Cors__AllowedOrigins__0`)
   2. User Secrets (Development environment only)
   3. `appsettings.{Environment}.json`
   4. `appsettings.json`

2. **CORS Isolation in Production**:
   When `ASPNETCORE_ENVIRONMENT != Development`, the API explicitly checks if any `Cors__AllowedOrigins*` environment variables are defined. If present, only the specified origins are allowed. Default localhost origins are excluded in Production unless explicitly listed.

3. **Startup Validation**:
   In non-Development environments, VoyageHub performs startup validation checks before the application starts accepting requests:
   - Database connection string must not be null or whitespace.
   - JWT key must not be null or whitespace and must contain at least 32 characters (256 bits for HMAC-SHA256).
   - If either check fails, the application terminates immediately with a clear error message without logging sensitive values.
