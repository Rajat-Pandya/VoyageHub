# VoyageHub Deployment & Hosting Guide

This guide covers deployment architectures, configuration keys, free-tier hosting options, and pre-demo operational checklists for VoyageHub.

---

## 1. Evaluated Hosting Options & Free-Tier Architectures

VoyageHub has been evaluated across three distinct deployment models to support evaluator reviews and cost-free demonstrations:

| Hosting Option | Backend Target | Frontend Target | Database Target | Limits & Caveats | Best For |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Option A: Azure Free Tiers (Recommended Cloud)** | Azure App Service (F1 Free tier) | Azure Static Web Apps (Free tier) | Azure SQL Database (Free Offer) | • App Service F1 sleeps after 20m inactivity (requires warm-up request).<br>• 60 CPU minutes/day limit.<br>• Azure SQL free offer auto-pauses when free limit reached (causes transient latency on resume). | Multi-tier cloud production preview; separated client/API domains. |
| **Option B: MonsterASP.NET (Single-Site)** | Windows IIS Single Web Site (`net10.0`) | Bundled inside `wwwroot` (Static CSR) | Hosted Microsoft SQL Server (Free DB tier) | • Single deployment package.<br>• Free plan database limits (e.g., 25–100MB limit, renewal prompts).<br>• Requires setting `UseStaticFiles` and fallback routing in ASP.NET Core pipeline. | Simplified single-URL demo; zero CORS setup across different domains. |
| **Option C: Local Docker + Tunnel (Evaluation Fallback)** | Docker Linux container (`mcr.microsoft.com/dotnet/aspnet:10.0`) | Bundled Nginx or Local Angular build | Local Microsoft SQL Server Express or Docker SQL Server | • Requires host machine to stay awake and connected to internet.<br>• Tunnel latency depends on host connection.<br>• Free tunnels (ngrok/Cloudflare Tunnel) generate ephemeral or custom hostnames. | Immediate evaluation fallback if cloud resources sleep or encounter quota exhaustion. |

---

## 2. Option A: Step-by-Step Azure Free-Tier Deployment

```mermaid
flowchart TD
    User([User Browser])
    SWA["Azure Static Web Apps (Free)<br>Angular 21 Client (CSR)<br>staticwebapp.config.json"]
    API["Azure App Service (F1 Free)<br>ASP.NET Core 10 Web API<br>VoyageHub.Api"]
    DB[("Azure SQL Database (Free Offer)<br>32GB Storage / 100k vCore-sec/mo<br>Auto-pause enabled")]

    User -->|HTTPS| SWA
    SWA -->|REST / CORS Bearer| API
    API -->|TDS Encrypted (1433)| DB
```

### Step 1: Azure SQL Database Setup
1. In the Azure Portal, create a resource group (e.g. `rg-voyagehub-free`).
2. Create a new **SQL Database** (e.g. `voyagehub-db`) on server `sql-voyagehub-<unique>.database.windows.net`.
3. Under **Compute + Storage**, choose **Apply free offer** (100,000 vCore seconds/month, 32 GB storage).
4. In **Networking**, enable **Allow Azure services and resources to access this server**, and add your client IP address.
5. In SQL Server Management Studio (SSMS) or Azure Data Studio, connect to the database and execute [database/VoyageHub_Schema.azure.sql](database/VoyageHub_Schema.azure.sql).
6. **Set Seed User Passwords**: Use the `tools/HashPassword` utility to generate BCrypt hashes for production users:
   ```powershell
   echo "YourSecureAdminPassword2026!" | dotnet run --project tools/HashPassword
   ```
   Execute an `UPDATE` statement in SSMS to update the target user `PasswordHash` values from the `'DISABLED-SET-VIA-DEPLOYMENT-MD'` placeholder:
   ```sql
   UPDATE dbo.USERS
   SET PasswordHash = '$2a$11$YourGeneratedBCryptHash...'
   WHERE Email = 'admin@voyagehub.in';
   ```

### Step 2: Azure App Service (F1) Setup
1. Create a Web App:
   - Name: `api-voyagehub-<unique>` (`https://api-voyagehub-<unique>.azurewebsites.net`)
   - Runtime: `.NET 10`
   - Plan: **Free F1**
2. In **Configuration** $\rightarrow$ **Application settings**, configure:
   - `ConnectionStrings__DefaultConnection`: `Server=tcp:sql-voyagehub-<unique>.database.windows.net,1433;Database=voyagehub-db;User ID=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=False;`
   - `Jwt__Key`: `<Your-Secure-32-Character-Key>` (minimum 32 characters)
   - `Jwt__Issuer`: `VoyageHub.Api`
   - `Jwt__Audience`: `VoyageHub.Client`
   - `Swagger__Enabled`: `false` (or `true` if inspecting endpoints in staging)
   - `Cors__AllowedOrigins__0`: `https://<your-static-app>.azurestaticapps.net`
3. Publish and deploy:
   ```powershell
   cd VoyageHub-Backend\VoyageHub.Api
   dotnet publish -c Release -o ./publish
   cd publish
   Compress-Archive -Path * -DestinationPath ../api-deploy.zip
   az webapp deploy --resource-group rg-voyagehub-free --name api-voyagehub-<unique> --src-path ../api-deploy.zip --type zip
   ```

### Step 3: Azure Static Web Apps Setup
1. Update `VoyageHub-Frontend/src/environments/environment.prod.ts` with your App Service API URL:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://api-voyagehub-<unique>.azurewebsites.net/api'
   };
   ```
2. Build the client-side static bundle:
   ```powershell
   cd VoyageHub-Frontend
   npm run build:static
   ```
3. Deploy the contents of `dist/AngularBasics/browser/` (which includes `staticwebapp.config.json`) to Azure Static Web Apps using Azure CLI or GitHub Actions.

---

## 3. Option B: MonsterASP.NET Single-Site Deployment

MonsterASP.NET provides free Windows IIS hosting for ASP.NET Core applications with a single custom domain or test sub-domain.

### Packaging Procedure
1. Build the Angular static app:
   ```powershell
   cd VoyageHub-Frontend
   npm run build:static
   ```
2. Copy all generated files from `VoyageHub-Frontend/dist/AngularBasics/browser/` into `VoyageHub-Backend/VoyageHub.Api/wwwroot/`.
3. In `Program.cs`, ensure `app.UseDefaultFiles()`, `app.UseStaticFiles()`, and fallback routing `app.MapFallbackToFile("index.html")` are enabled.
4. Publish the combined project:
   ```powershell
   cd VoyageHub-Backend\VoyageHub.Api
   dotnet publish -c Release -o ./publish-single-site
   ```
5. Upload the `./publish-single-site` package via FTP to MonsterASP.NET `site/wwwroot/`.
6. Configure connection string and JWT key in MonsterASP.NET Control Panel or `web.config`.

---

## 4. Option C: Local Docker + Tunnel Deployment (Evaluator Fallback)

If cloud free-tier quotas are exhausted or databases are sleeping during an interview, a local containerized deployment exposed via Cloudflare Tunnel provides an instant, 100% reliable backup.

### Run via Docker
```powershell
# 1. Build backend image
docker build -t voyagehub-api:latest -f VoyageHub-Backend/VoyageHub.Api/Dockerfile .

# 2. Run container linking to host SQL Server
docker run -d -p 5062:8080 `
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal\\SQLEXPRESS06;Database=AngularAuthDb;Trusted_Connection=True;TrustServerCertificate=True;" `
  -e Jwt__Key="LocalFallbackTunnelEvaluationSecretKey2026!" `
  --name voyagehub-api voyagehub-api:latest

# 3. Expose via Cloudflare Tunnel (or ngrok)
cloudflared tunnel --url http://localhost:5062
```

---

## 5. Pre-Demo Verification Checklist

Follow this checklist 15 minutes before any presentation or interview:

- [ ] **1. Wake Up Hosted Services**:
  - Open `https://api-voyagehub-<unique>.azurewebsites.net/` in browser. Expect HTTP 200 text response: `"VoyageHub API is running."`
  - Allow 30–60 seconds if App Service F1 or Azure SQL is spinning up from cold sleep.
- [ ] **2. Test Authentication Endpoint**:
  - Execute a quick curl check to ensure token generation succeeds:
    ```powershell
    curl.exe -X POST "https://api-voyagehub-<unique>.azurewebsites.net/api/Auth/login" `
      -H "Content-Type: application/json" `
      -d "{\"email\":\"admin@voyagehub.in\",\"password\":\"YourConfiguredPassword\"}"
    ```
  - Verify that a valid JWT token is returned in `token`.
- [ ] **3. Verify Client Frontend**:
  - Open `https://<your-static-app>.azurestaticapps.net` in an incognito/private browser window.
  - Verify login page renders cleanly without CORS console errors.
- [ ] **4. Database State Reset**:
  - If test requests were accumulated during rehearsal, re-run `database/VoyageHub_Schema.sql` locally or reset demo records in Azure SQL so approval queues display clean numbers.
- [ ] **5. Local Fallback Running**:
  - Have local SQL Server Express and local `dotnet run` / `npm start` terminals ready in the background on your laptop as an instantaneous fallback.
