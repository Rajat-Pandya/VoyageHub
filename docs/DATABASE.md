# VoyageHub Database Architecture & Schema Reference

This document provides the entity-relationship model, schema definitions, constraints, seeding mechanisms, and migration procedures for the VoyageHub database (`AngularAuthDb`).

---

## 1. Entity-Relationship Diagram

```mermaid
erDiagram
    DEPARTMENTS ||--o| TRAVEL_POLICIES : "defines policy (1:1)"
    DEPARTMENTS ||--o{ USERS : "employs"
    USERS ||--o{ USERS : "manages"
    USERS ||--o{ TRAVEL_REQUESTS : "submits"
    USERS ||--o{ APPROVAL_STEPS : "approves"
    USERS ||--o{ SUPPORT_TICKETS : "files"
    USERS ||--o{ AUDIT_LOGS : "performs"
    TRAVEL_REQUESTS ||--o{ APPROVAL_STEPS : "undergoes"
    TRAVEL_REQUESTS ||--o{ ITINERARY_SEGMENTS : "contains"
    TRAVEL_REQUESTS ||--o{ STAYS : "contains"
    TRAVEL_REQUESTS ||--o{ EXPENSES : "accrues"
    TRAVEL_REQUESTS ||--o{ SUPPORT_TICKETS : "references"

    DEPARTMENTS {
        guid Id PK
        string Name UK "nvarchar(150)"
        datetime2 CreatedAt
    }

    USERS {
        guid Id PK
        string Name "nvarchar(150)"
        string Email UK "nvarchar(255)"
        string PasswordHash "nvarchar(500)"
        string Role "nvarchar(50)"
        guid DepartmentId FK "nullable"
        guid ManagerId FK "nullable"
        string AvatarUrl "nvarchar(500) nullable"
        string PasswordResetToken "nvarchar(200) nullable"
        datetime2 PasswordResetTokenExpires "nullable"
        datetime2 CreatedAt
    }

    TRAVEL_POLICIES {
        guid Id PK
        guid DepartmentId FK,UK
        decimal MaxBudget "decimal(18,2)"
        decimal IntlBudgetLimit "decimal(18,2)"
        decimal DomesticLimit "decimal(18,2)"
        string AllowedClass "nvarchar(50)"
        bool HardEnforcement "bit"
        datetime2 CreatedAt
        datetime2 UpdatedAt
    }

    TRAVEL_REQUESTS {
        guid Id PK
        string RequestCode UK "nvarchar(50)"
        guid EmployeeId FK
        string Destination "nvarchar(200)"
        string Purpose "nvarchar(500)"
        date StartDate "date"
        date EndDate "date"
        decimal EstimatedBudget "decimal(18,2)"
        string TravelClass "nvarchar(50)"
        string Justification "nvarchar(max) nullable"
        string Status "nvarchar(50)"
        string PolicyCompliance "nvarchar(50)"
        datetime2 CreatedAt
        datetime2 UpdatedAt
    }

    APPROVAL_STEPS {
        guid Id PK
        guid RequestId FK
        guid ApproverId FK "nullable"
        string Level "nvarchar(50)"
        string Status "nvarchar(50)"
        string Comment "nvarchar(max) nullable"
        datetime2 DecidedAt "nullable"
    }

    ITINERARY_SEGMENTS {
        guid Id PK
        guid RequestId FK
        string Mode "nvarchar(50)"
        string FromLocation "nvarchar(150)"
        string ToLocation "nvarchar(150)"
        datetime2 DepartureTime
        datetime2 ArrivalTime
        string BookingRef "nvarchar(100) nullable"
        decimal Cost "decimal(18,2)"
    }

    STAYS {
        guid Id PK
        guid RequestId FK
        string HotelName "nvarchar(200)"
        string Location "nvarchar(150)"
        date CheckIn "date"
        date CheckOut "date"
        string BookingRef "nvarchar(100) nullable"
        decimal Cost "decimal(18,2)"
    }

    EXPENSES {
        guid Id PK
        guid RequestId FK
        string Category "nvarchar(100)"
        decimal Amount "decimal(18,2)"
        string ProofPath "nvarchar(500) nullable"
        string Status "nvarchar(50)"
        datetime2 SubmittedAt
    }

    SUPPORT_TICKETS {
        guid Id PK
        string TicketCode UK "nvarchar(50)"
        guid UserId FK
        guid RequestId FK "nullable"
        string Subject "nvarchar(250)"
        string Category "nvarchar(100)"
        string Priority "nvarchar(50)"
        string Status "nvarchar(50)"
        string AssignedTo "nvarchar(150) nullable"
        string Description "nvarchar(max)"
        string ResolutionNotes "nvarchar(max) nullable"
        datetime2 CreatedAt
        datetime2 UpdatedAt
    }

    AUDIT_LOGS {
        guid Id PK
        string EntityType "nvarchar(100)"
        guid EntityId "nullable"
        string Action "nvarchar(150)"
        guid ActorId "nullable"
        string ActorName "nvarchar(150)"
        string ChangeDetail "nvarchar(max)"
        string Severity "nvarchar(50)"
        datetime2 Timestamp
    }
```

---

## 2. Table Catalog & Key Constraints

| Table Name | Primary Purpose | Foreign Keys & References | Unique Constraints | Cascade Delete Behavior |
| :--- | :--- | :--- | :--- | :--- |
| **`DEPARTMENTS`** | Organizational hierarchy units (Engineering, Product, Finance, etc.). | None | `UK_DEPARTMENTS_Name` | Restricts deletion if users or policies reference it. |
| **`USERS`** | System actors across all four persona roles. | `DepartmentId` $\rightarrow$ `DEPARTMENTS(Id)`, `ManagerId` $\rightarrow$ `USERS(Id)` | `UK_USERS_Email` | `ON DELETE SET NULL` on Department; `ON DELETE RESTRICT` on Manager. |
| **`TRAVEL_POLICIES`** | Budget caps, class rules, and enforcement flags per department. | `DepartmentId` $\rightarrow$ `DEPARTMENTS(Id)` | `UK_TRAVEL_POLICIES_Dept` (1:1) | `ON DELETE CASCADE` when parent department is deleted. |
| **`TRAVEL_REQUESTS`** | Core travel requisition lifecycle entity. | `EmployeeId` $\rightarrow$ `USERS(Id)` | `UK_TRAVEL_REQUESTS_Code` | `ON DELETE CASCADE` when parent user is deleted. |
| **`APPROVAL_STEPS`** | Sequential approval nodes (Manager & Finance) for travel requests. | `RequestId` $\rightarrow$ `TRAVEL_REQUESTS(Id)`, `ApproverId` $\rightarrow$ `USERS(Id)` | Composite index on `(RequestId, Level)` | `ON DELETE CASCADE` when parent request is deleted. |
| **`ITINERARY_SEGMENTS`** | Multi-modal travel legs (flight, train, taxi). | `RequestId` $\rightarrow$ `TRAVEL_REQUESTS(Id)` | None | `ON DELETE CASCADE` when parent request is deleted. |
| **`STAYS`** | Hotel and lodging reservations associated with a trip. | `RequestId` $\rightarrow$ `TRAVEL_REQUESTS(Id)` | None | `ON DELETE CASCADE` when parent request is deleted. |
| **`EXPENSES`** | Itemized out-of-pocket expenses and reimbursement claims. | `RequestId` $\rightarrow$ `TRAVEL_REQUESTS(Id)` | None | `ON DELETE CASCADE` when parent request is deleted. |
| **`SUPPORT_TICKETS`** | Internal corporate travel desk concierge inquiries. | `UserId` $\rightarrow$ `USERS(Id)`, `RequestId` $\rightarrow$ `TRAVEL_REQUESTS(Id)` | `UK_SUPPORT_TICKETS_Code` | `ON DELETE CASCADE` on User; `ON DELETE SET NULL` on Request. |
| **`AUDIT_LOGS`** | Append-only security and operational audit trail. | None (Loose actor attribution for historical durability) | None | Non-cascading append-only records. |

---

## 3. Seed Data Mechanics

The schema script automatically provisions standard corporate foundation data:
1. **Departments**: Engineering, Product Management, Finance & Accounts, Sales & Operations, Human Resources, Administration.
2. **Travel Policies**: Standard baseline limits (e.g., Engineering: ₹80,000 domestic, ₹2,50,000 international; Economy class only; hard enforcement enabled).
3. **Demo Personas**:
   - `admin@voyagehub.in` (System Admin)
   - `priya.nair@voyagehub.in` (Manager)
   - `vikram.mehta@voyagehub.in` (Finance Officer)
   - `aarav.patel@voyagehub.in` (Employee)

---

## 4. Difference: Local vs. Azure SQL Schema Scripts

VoyageHub maintains two schema scripts with distinct cloud-safety characteristics:

| Dimension | `database/VoyageHub_Schema.sql` (Local Development) | `database/VoyageHub_Schema.azure.sql` (Hosted / Azure SQL) |
| :--- | :--- | :--- |
| **Database Creation** | Includes `CREATE DATABASE [AngularAuthDb]` and `USE [AngularAuthDb]` statements. | Omits database creation and `USE` statements (unsupported in Azure SQL managed pools). |
| **Seeded User Passwords** | Seeds active BCrypt password hashes (`VoyageHub@2026`) for immediate offline development. | Seeds disabled placeholder `'DISABLED-SET-VIA-DEPLOYMENT-MD'` to prevent accidental default credentials in cloud environments. |
| **Usage Context** | Local SQL Server Express instances or Docker SQL Server containers. | Azure SQL Database (Free Offer) or hosted production databases. |

---

## 5. How to Reset the Database

To reset the database back to clean baseline seed data:

### Automated Reset via PowerShell
```powershell
.\database\run_migration.ps1 -Server ".\SQLEXPRESS06" -Database "AngularAuthDb"
```

### Manual Reset via SQL Server Command Line (`sqlcmd`)
```powershell
sqlcmd -S ".\SQLEXPRESS06" -d master -E -i "database\VoyageHub_Schema.sql"
```
*(This script drops and recreates tables if they exist, resetting all test travel requests, approval queues, and expense records to default state.)*
