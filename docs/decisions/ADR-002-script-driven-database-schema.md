# ADR-002: Script-Driven Database Schema vs. EF Core Code-First Migrations

## Status
Accepted

## Context
VoyageHub requires consistent database schema definitions across local developer environments (Microsoft SQL Server / SQL Server Express) and cloud hosted environments (Azure SQL Database). Two primary approaches were considered:
1. Automatically generated EF Core code-first migrations (`dotnet ef migrations add`).
2. Declarative SQL DDL scripts (`database/VoyageHub_Schema.sql` and `database/VoyageHub_Schema.azure.sql`) managed via PowerShell scripts and `sqlcmd`.

## Decision
Adopt **declarative T-SQL schema scripts** as the primary source of truth for database schema creation and seed data:
- `database/VoyageHub_Schema.sql`: Includes `CREATE DATABASE [AngularAuthDb]` and seeds active development credentials for local SQL Server.
- `database/VoyageHub_Schema.azure.sql`: Omits database creation and `USE` statements (unsupported in Azure SQL managed pools), and seeds disabled password placeholders (`DISABLED-SET-VIA-DEPLOYMENT-MD`) to guarantee production credential isolation.
- Entity configurations in `VoyageHub.Data` (`AppDbContext.cs`) are maintained in strict synchronization with the SQL schema using Fluent API annotations and uppercase table names.

## Consequences
- **Positive**: Complete control over table constraints, unique indexes, cascading delete rules, and seed data without opaque migration tables (`__EFMigrationsHistory`). Enables DBAs to inspect and review DDL before execution.
- **Negative / Trade-offs**: Schema evolution requires manual synchronization between the SQL scripts and C# entity models in `VoyageHub.Data`.
