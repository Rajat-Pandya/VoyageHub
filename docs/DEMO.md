# VoyageHub Guided Demonstration Script & Runbook

This runbook provides an evaluator, reviewer, or interviewer with a step-by-step walkthrough across all four corporate personas, demonstrating end-to-end travel request, multi-tier approval, expense reimbursement, and administration workflows.

---

## 1. Pre-Demo Verification Checklist

Complete these steps 10–15 minutes before beginning the demonstration:

- [ ] **1. Warm-Up Cloud Services (if using hosted Azure deployment)**:
  - Open `https://api-voyagehub-<unique>.azurewebsites.net/` in browser. Expect `"VoyageHub API is running."`
  - Allow 30–60 seconds if App Service F1 or Azure SQL is spinning up from cold sleep.
- [ ] **2. Local Fallback Verification (Recommended)**:
  - Open a PowerShell terminal and verify local backend starts cleanly:
    ```powershell
    cd VoyageHub-Backend\VoyageHub.Api
    dotnet run
    ```
    *(Accessible at `http://localhost:5062`)*
  - Open a second terminal and verify local Angular frontend is running:
    ```powershell
    cd VoyageHub-Frontend
    npm start
    ```
    *(Accessible at `http://localhost:4200`)*
- [ ] **3. Reset Demo Data**:
  - Reset database records to a pristine initial state by executing:
    ```powershell
    .\database\run_migration.ps1 -Server ".\SQLEXPRESS06" -Database "AngularAuthDb"
    ```
- [ ] **4. Incognito Window**: Open the frontend in an Incognito / Private browser window to ensure clean local storage state.

---

## 2. Persona 1: Traveling Employee (`Aarav Patel`)

**Role**: Senior Software Engineer in Engineering Department.  
**Goal**: Create a business travel request, add itineraries and hotel stays, submit for approval, upload expense receipts, and open a support inquiry.

### Step-by-Step Click Path
1. **Sign In**:
   - Navigate to `http://localhost:4200/login`.
   - Click the **Employee (Aarav Patel)** quick-select persona chip (or enter `aarav.patel@voyagehub.in` / `VoyageHub@2026`).
   - Click **Sign In**. Redirects to `/dashboard`.
2. **Dashboard Overview**:
   - Observe personal KPI metrics (Active Trips, Draft Requests, Pending Approvals, Pending Expenses).
3. **Create New Travel Request**:
   - In sidebar, click **New Request** (`/requests/new`).
   - Fill form:
     - **Destination**: `Bangalore, India`
     - **Purpose**: `Q4 Architecture Review & Client Onboarding`
     - **Departure Date**: Select date 7 days in future.
     - **Return Date**: Select date 10 days in future.
     - **Estimated Budget**: `₹45,000` *(Within Engineering policy limit of ₹80,000)*.
     - **Travel Class**: `Economy`.
   - Click **Add Flight / Train Segment**:
     - Mode: `Flight` | From: `Mumbai (BOM)` | To: `Bangalore (BLR)` | Cost: `₹12,000`.
   - Click **Add Hotel Stay**:
     - Hotel: `The Leela Palace` | Location: `Old Airport Rd` | Cost: `₹24,000`.
   - Click **Save as Draft**.
4. **Inspect Draft & Submit**:
   - Navigate to **My Requests** (`/requests`).
   - Click on the newly created request `#TR-xxxx` to open **Request Detail** (`/requests/{id}`).
   - Review the visual itinerary timeline and policy compliance status (**Compliant**, green badge).
   - Click **Submit for Approval** button.
   - Status updates in real-time to **Manager Review**.
5. **Submit Out-of-Pocket Expense Claim**:
   - In Request Detail view, scroll to **Expenses** section.
   - Click **Add Expense Claim**:
     - Category: `Meals & Incidentals`
     - Amount: `₹3,500`
     - Upload sample receipt document (`sample.pdf` or `.png`).
     - Click **Submit Claim**.
6. **File Travel Desk Concierge Inquiry**:
   - In sidebar, click **Support Desk** (`/support`).
   - Click **Submit Inquiry**:
     - Subject: `Airport cab pickup coordination for Bangalore trip`
     - Priority: `Medium`
     - Associated Request: Select `#TR-xxxx`.
     - Description: `Please arrange airport pickup at BLR airport on arrival day.`
   - Click **Submit Ticket**. Ticket code `#TICK-xxxx` is generated.
7. **Sign Out**: Click user initials badge in sidebar footer and select **Logout**.

---

## 3. Persona 2: Line Manager (`Priya Nair`)

**Role**: Product & Engineering Line Manager.  
**Goal**: Review pending team travel requests, inspect budget compliance against policy limits, and approve request to advance it to Finance.

### Step-by-Step Click Path
1. **Sign In**:
   - Navigate to `/login`. Click **Manager (Priya Nair)** persona chip and click **Sign In**.
2. **Review Approvals Queue**:
   - In sidebar, click **Approvals** (`/approvals`).
   - In the **Manager Approvals** tab, locate Aarav Patel's submitted request `#TR-xxxx` (`Bangalore, India - ₹45,000`).
   - Notice policy indicator: **Compliant** (estimated ₹45,000 vs. ₹80,000 cap).
3. **Approve Request**:
   - Click **View Details / Action**.
   - Review trip itinerary legs and hotel stay reservations.
   - In comments box, enter: `"Approved. Project budget is allocated for Q4 architecture review."`
   - Click **Approve Request**.
   - Request status transitions immediately to **Finance Review**, removing it from Priya's pending queue and placing it in the Finance queue.
4. **Sign Out**: Click user initials badge $\rightarrow$ **Logout**.

---

## 4. Persona 3: Finance Officer (`Vikram Mehta`)

**Role**: Corporate Finance & Accounts Officer.  
**Goal**: Execute second-level financial approval, inspect expense reimbursement claims, and process batch payouts.

### Step-by-Step Click Path
1. **Sign In**:
   - Navigate to `/login`. Click **Finance (Vikram Mehta)** persona chip and click **Sign In**.
2. **Finalize Travel Request Approval**:
   - Navigate to **Approvals** (`/approvals`).
   - Locate Aarav's request in the **Finance Approvals** queue.
   - In comments box, enter: `"Budget verified against cost-center code ENG-402."`
   - Click **Approve**. Request status transitions to **Approved**.
3. **Review Expense Claims & Proof Documents**:
   - In sidebar, click **Expense Review** (`/finance/expenses`).
   - Review the pending claims table. Locate Aarav Patel's ₹3,500 meal claim.
   - Click **View Proof Receipt** to stream the uploaded receipt document in a new tab.
4. **Execute Batch Reimbursement Payout**:
   - Select the checkbox next to Aarav's claim (and any other pending claims).
   - Notice the total payout counter update at the top of the table.
   - Click **Process Batch Reimbursement**.
   - Claims transition to **Reimbursed**, and an audit record is generated recording the total disbursement.
5. **Sign Out**: Click user initials badge $\rightarrow$ **Logout**.

---

## 5. Persona 4: System Administrator (`Ananya Verma`)

**Role**: Enterprise Corporate System Administrator.  
**Goal**: Manage departmental travel policies, audit log oversight, employee directory operations, and support desk resolution.

### Step-by-Step Click Path
1. **Sign In**:
   - Navigate to `/login`. Click **Admin (Ananya Verma)** persona chip and click **Sign In**.
2. **Inspect System Audit Logs**:
   - In sidebar, click **Audit Logs** (`/admin/audit-logs`).
   - Observe the live chronological audit trail:
     - Notice events for `Submit Travel Request`, `Approve Step` (Manager), `Approve Step` (Finance), `Submit Expense`, and `Batch Payout`.
     - Filter by Severity (`LOW`, `MEDIUM`, `HIGH`) and Entity Type (`EXPENSE`, `APPROVAL`, `TRAVEL_REQUEST`).
3. **Manage Travel Policies**:
   - In sidebar, click **Policy Config** (`/admin/policies`).
   - Locate the **Engineering** department policy.
   - Click **Edit**:
     - Update Domestic Budget Limit from `₹80,000` to `₹90,000`.
     - Toggle Hard Enforcement on.
     - Click **Save Policy**.
4. **Employee Directory & CSV Template Download**:
   - In sidebar, click **Employee Directory** (`/admin/employees`).
   - Search for `"Aarav"` or filter by Department `"Engineering"`.
   - Click **Download CSV Template** to download `employees_template.csv`.
5. **Resolve Corporate Travel Desk Ticket**:
   - In sidebar, click **Support Directory** (`/support`).
   - Locate Aarav's inquiry ticket `#TICK-xxxx`.
   - Click **Open Ticket**:
     - Update status to `Resolved`.
     - Add resolution notes: `"Confirmed airport cab pickup with Bangalore corporate travel fleet."`
     - Click **Update Status**.

---

## 6. Contingency & Troubleshooting Plan

| Issue Encountered | Root Cause | Immediate Remediation |
| :--- | :--- | :--- |
| **Hosted API returns 500 or times out** | Azure SQL free tier or App Service F1 is sleeping from inactivity. | Send a warm-up GET request to `https://api-voyagehub-<unique>.azurewebsites.net/` and wait 45 seconds for cold start. |
| **CORS error in browser console** | Frontend origin does not match `Cors:AllowedOrigins`. | Switch immediately to local development setup (`http://localhost:4200` $\rightarrow$ `http://localhost:5062`). |
| **User persona shows "Invalid credentials"** | Cloud database seed used disabled passwords. | Use the local SQL Server setup where pre-computed BCrypt hashes match `VoyageHub@2026`. |
