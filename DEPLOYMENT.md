# VoyageHub Azure Deployment Guide (Free Tier Route)

This step-by-step guide covers how to deploy the entire VoyageHub application (API, Angular client, and SQL Database) to Azure's free service tiers without incurring charges.

---

## Architecture Overview on Azure Free Tiers

```
┌─────────────────────────────────────────────────────────┐
│              Azure Static Web Apps (Free)               │
│   Client-side Angular 21 CSR Application (Single Host)  │
│   Routing via staticwebapp.config.json                  │
└────────────────────────────┬────────────────────────────┘
                             │ HTTPS / CORS Preflight
                             ▼
┌─────────────────────────────────────────────────────────┐
│               Azure App Service (F1 Free)               │
│       ASP.NET Core 10 Web API (VoyageHub.Api)           │
│       Settings configured via App Service App Settings  │
└────────────────────────────┬────────────────────────────┘
                             │ Encrypted SQL Connection (TDS 1433)
                             ▼
┌─────────────────────────────────────────────────────────┐
│           Azure SQL Database (Free Offer Tier)          │
│       100,000 vCore seconds / 32 GB storage / mo.       │
│       Auto-pause enabled to prevent overage charges     │
└─────────────────────────────────────────────────────────┘
```

---

## Step 1: Create Free Azure SQL Database

1. **Sign in to Azure Portal**:
   Navigate to [portal.azure.com](https://portal.azure.com).

2. **Create SQL Database**:
   - In search bar, search for **SQL databases** and click **Create**.
   - Resource Group: Create new, e.g. `rg-voyagehub-free`.
   - Database name: `voyagehub-db`.
   - Server: Create new server, e.g. `sql-voyagehub-<yourname>`. Choose SQL Authentication and record your server admin credentials securely.
   - **Workload environment**: Select **Development**.
   - **Compute + storage**: Select **Apply free offer** (free 100,000 vCore-seconds per month with 32 GB storage).
   - Ensure **Auto-pause database when free limit reached** is enabled.

3. **Configure Firewall**:
   - Go to the SQL Server resource -> **Security** -> **Networking**.
   - Enable **Allow Azure services and resources to access this server** (required for App Service connection).
   - Under **Firewall rules**, add your current client IP address so you can connect via SQL Server Management Studio (SSMS) or Azure Data Studio.

---

## Step 2: Initialize Database Schema & Seed Data

1. **Connect to Azure SQL via SSMS or Azure Data Studio**:
   - Server name: `sql-voyagehub-<yourname>.database.windows.net`
   - Authentication: SQL Server Authentication
   - Login / Password: Server administrator credentials
   - Database: `voyagehub-db`

2. **Execute Schema Script**:
   - Open and execute [database/VoyageHub_Schema.azure.sql](database/VoyageHub_Schema.azure.sql).
   - This script creates all tables, relationships, constraints, and demo data without `CREATE DATABASE` or `USE` statements.
   - All seeded users have their `PasswordHash` set to the disabled placeholder `'DISABLED-SET-VIA-DEPLOYMENT-MD'`.

3. **Generate Secure Password Hashes Using `tools/HashPassword`**:
   On your local machine, run the password hashing console tool:
   ```powershell
   # Run the tool and enter your desired password via standard input
   echo "YourSecureAdminPassword2026!" | dotnet run --project tools/HashPassword
   ```
   The tool outputs only the computed 60-character BCrypt hash (work factor 11, matching `AuthService`).

4. **Update User Passwords in Azure SQL**:
   Execute the following UPDATE pattern in SSMS to set real credentials:
   ```sql
   -- Update Admin (Ananya Verma)
   UPDATE dbo.USERS
   SET PasswordHash = '$2a$11$ComputedHashFromToolGoesHere......................'
   WHERE Email = 'admin@voyagehub.in';

   -- Update Manager (Priya Nair)
   UPDATE dbo.USERS
   SET PasswordHash = '$2a$11$ComputedHashFromToolGoesHere......................'
   WHERE Email = 'priya.nair@voyagehub.in';

   -- Update Finance (Vikram Malhotra)
   UPDATE dbo.USERS
   SET PasswordHash = '$2a$11$ComputedHashFromToolGoesHere......................'
   WHERE Email = 'vikram.malhotra@voyagehub.in';

   -- Update Employee (Rohan Sharma)
   UPDATE dbo.USERS
   SET PasswordHash = '$2a$11$ComputedHashFromToolGoesHere......................'
   WHERE Email = 'rohan.sharma@voyagehub.in';
   ```

---

## Step 3: Create Azure App Service (F1 Tier)

1. **Create Web App**:
   - Name: `api-voyagehub-<unique>` (URL: `https://api-voyagehub-<unique>.azurewebsites.net`).
   - Publish: **Code**.
   - Runtime stack: **.NET 10** (or .NET 8 LTS if standard region image).
   - Operating System: **Linux** or **Windows**.
   - Pricing Plan: **Free F1** (1 GB memory, 60 CPU minutes/day).

2. **Configure Application Settings**:
   Navigate to **Configuration** -> **Application settings** (or **Environment variables**) and add:
   - `ConnectionStrings__DefaultConnection`: `Server=tcp:sql-voyagehub-<yourname>.database.windows.net,1433;Database=voyagehub-db;User ID=<admin>;Password=<pass>;Encrypt=True;TrustServerCertificate=False;`
   - `Jwt__Key`: `<Generated-32-character-or-longer-secret-key>`
   - `Jwt__Issuer`: `VoyageHub.Api`
   - `Jwt__Audience`: `VoyageHub.Client`
   - `Swagger__Enabled`: `false` (or `true` if you wish to verify Swagger UI in testing)
   - `Cors__AllowedOrigins__0`: `https://lemon-island-012345678.azurestaticapps.net` (Update after creating the frontend Static Web App in Step 5)

---

## Step 4: Publish & Deploy Backend API

1. **Publish Locally**:
   ```powershell
   cd VoyageHub-Backend\VoyageHub.Api
   dotnet publish -c Release -o ./publish
   ```

2. **Deploy to App Service**:
   Deploy using the Azure CLI, Visual Studio / VS Code Azure Tools, or zip deploy:
   ```powershell
   cd publish
   Compress-Archive -Path * -DestinationPath ../api-deploy.zip
   az webapp deploy --resource-group rg-voyagehub-free --name api-voyagehub-<unique> --src-path ../api-deploy.zip --type zip
   ```

---

## Step 5: Deploy Frontend to Azure Static Web Apps

1. **Configure Production API URL in Angular**:
   Update `VoyageHub-Frontend/src/environments/environment.prod.ts` with your actual App Service API URL:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://api-voyagehub-<unique>.azurewebsites.net/api'
   };
   ```

2. **Build Client-Side Static Bundle**:
   ```powershell
   cd VoyageHub-Frontend
   npm run build:static
   ```
   The build output is located at `dist/AngularBasics/browser`, containing `index.html` and `staticwebapp.config.json`.

3. **Deploy to Azure Static Web Apps**:
   Create a Static Web App resource in the Azure Portal or via Azure CLI / GitHub Actions:
   - Plan type: **Free**.
   - App location: `VoyageHub-Frontend/dist/AngularBasics/browser` (or deploy via Azure SWA CLI: `swa deploy ./dist/AngularBasics/browser --env production`).

4. **Synchronize CORS on API**:
   Once Azure Static Web Apps provides your URL (e.g. `https://lemon-island-012345678.azurestaticapps.net`):
   - Update `Cors__AllowedOrigins__0` in Azure App Service Configuration to match this exact origin.
   - Restart the App Service.

---

## Step 6: Post-Deployment Smoke Test

Perform the following verification checks against the live deployment:
1. **Health Check**: Request `GET https://api-voyagehub-<unique>.azurewebsites.net/`.
   - Verify HTTP 200 with text `"VoyageHub API is running."`.
2. **Swagger Status**: Request `GET https://api-voyagehub-<unique>.azurewebsites.net/swagger`.
   - When `Swagger__Enabled` is `false`, verify HTTP 404.
3. **Authentication**: In the Static Web App UI (`https://<swa-domain>.azurestaticapps.net`), sign in with one of the seeded user emails and the password set in Step 2.
   - Verify JWT token is received and user is redirected to `/dashboard`.
4. **Deep Linking**: Refresh a deep URL (e.g. `https://<swa-domain>.azurestaticapps.net/requests`).
   - Verify `staticwebapp.config.json` rewrites to `/index.html` and the page renders without a 404 error.
5. **Authorization Verification**:
   - Verify Employee only sees their own requests and stats.
   - Verify Manager sees direct reports in approval queue.

---

## Step 7: Presentation Warm-Up Checklist

Because free tiers incorporate auto-pause and resource limits, perform this warm-up 5–10 minutes before any demo:

- [ ] **Wake Up Azure SQL**: Run a simple `SELECT COUNT(*) FROM dbo.USERS;` query in SSMS or visit the dashboard. The serverless tier may take 30–60 seconds to resume from pause on the first request.
- [ ] **Warm Up App Service**: F1 free instances idle down after periods of inactivity. Ping `https://api-voyagehub-<unique>.azurewebsites.net/` in your browser until it responds with HTTP 200.
- [ ] **Monitor F1 CPU Quota**: App Service F1 tier allows 60 CPU minutes per day. Avoid leaving long polling or automated test loops running against the deployed instance.
- [ ] **Verify Persona Logins**: Log in once with an Employee (`rohan.sharma@voyagehub.in`) and a Manager (`priya.nair@voyagehub.in`) to ensure token issuance and cached views are pre-warmed.

---

## Step 8: Clean-Up Steps to Avoid Any Charges

When the project demonstration is complete, clean up all cloud resources:
```powershell
az group delete --name rg-voyagehub-free --yes --no-wait
```
Deleting the resource group removes the SQL Server, SQL Database, App Service Plan, Web App, and Static Web App simultaneously.
