# VoyageHub Requirements Traceability Matrix

This document maps every requirement from the original project specification to its implementing source files, automated test coverage, and verification status.

---

## Requirements Verification Matrix

| Requirement / Capability | Status | Implementing Source Files | Covering Automated Tests | Verification Notes |
| :--- | :---: | :--- | :--- | :--- |
| **1. Travel Requests (Draft & Submit)** | **Implemented** | • `TravelRequestsController.cs`<br>• `TravelRequestService.cs`<br>• `TravelRequestRepository.cs`<br>• `new-request.ts`<br>• `request-list.ts`<br>• `request-detail.ts` | • `AcceptanceCriteriaTests.cs`<br>• `TravelRequestValidationTests.cs` | Requests can be saved as `Draft` or auto-submitted. Submitting transitions status to `Manager Review` and instantiates sequential approval steps. |
| **2. Multi-Level Approvals with Comments & Auto-Routing** | **Implemented** | • `ApprovalsController.cs`<br>• `ApprovalService.cs`<br>• `ApprovalStepRepository.cs`<br>• `approval-queue.ts` | • `AcceptanceCriteriaTests.cs`<br>• `ApprovalServiceTests.cs`<br>• `AuthorizationTests.cs` | Manager approval automatically advances request to Finance Review with comment logging. Rejection by either party cascades cancellation to all remaining steps. |
| **3. Policy Enforcement & Budget Limits** | **Implemented** | • `PoliciesController.cs`<br>• `PolicyService.cs`<br>• `TravelRequestService.cs`<br>• `policy-config.ts` | • `PolicyServiceTests.cs`<br>• `TravelRequestValidationTests.cs`<br>• `AcceptanceCriteriaTests.cs` | Compares estimated trip budget against department domestic and international limits. Automatically sets `PolicyCompliance = 'Flagged'` when limits are exceeded. |
| **4. Itinerary Segments & Hotel Stays with Timeline** | **Implemented** | • `TravelRequestsController.cs`<br>• `TravelRequestService.cs`<br>• `request-detail.ts`<br>• `request-detail.html` | • `AcceptanceCriteriaTests.cs` | Supports multi-modal segments (flight, train, cab) and hotel stay bookings rendered as a sequential visual timeline on the request detail view. |
| **5. Expenses with Proof Upload & Reimbursement Tracking** | **Implemented** | • `ExpensesController.cs`<br>• `ExpenseService.cs`<br>• `ExpenseRepository.cs`<br>• `expense-review.ts` | • `ExpensesControllerTests.cs`<br>• `ExpenseServiceTests.cs` | Supports receipt file uploads (.pdf, .png, .jpg $\le 15$MB), proof streaming, single claim approval/rejection, and bulk batch reimbursement payouts. |
| **6. Reporting & Analytics (Spend, History, Department)** | **Implemented** | • `AnalyticsController.cs`<br>• `AnalyticsService.cs`<br>• `analytics-reports.ts`<br>• `analytics-reports.html` | Verified via API & Controller mapping | Computes total monthly spend trends, category breakdowns (Airfare, Lodging, Meals), employee spend histories, and departmental budget utilization. |
| **7. System Audit Logs** | **Implemented** | • `AuditLogsController.cs`<br>• `AuditService.cs`<br>• `AuditLogRepository.cs`<br>• `audit-logs.ts` | • `AcceptanceCriteriaTests.cs`<br>• `ExpenseServiceTests.cs`<br>• `EmployeeServiceTests.cs` | Automatically writes immutable audit records to `AUDIT_LOGS` on request creation, approvals, rejections, policy updates, payouts, and employee edits. |
| **8. Role-Based Access Control (RBAC)** | **Implemented** | • `CurrentUserService.cs`<br>• `role.guard.ts`<br>• `auth.interceptor.ts`<br>• All API Controllers | • `AuthorizationTests.cs`<br>• `role.guard.spec.ts` (20 tests) | 4 persona roles (Employee, Manager, Finance, Admin) enforced via server-side `[Authorize(Roles = "...")]` attributes and client `roleGuard`. |
| **9. Workflow State Machine** | **Implemented** | • `TravelRequestService.cs`<br>• `ApprovalService.cs` | • `AcceptanceCriteriaTests.cs`<br>• `ApprovalServiceTests.cs` | Strict state transitions: `Draft` $\rightarrow$ `Manager Review` $\rightarrow$ `Finance Review` $\rightarrow$ `Approved` / `Rejected` $\rightarrow$ `Completed`. |
| **10. Transactional Data Modeling** | **Implemented** | • `AppDbContext.cs`<br>• `database/VoyageHub_Schema.sql`<br>• All Repository implementations | • `RetryPolicyTests.cs`<br>• `EmployeeServiceTests.cs` | Relational SQL schema with foreign key cascades, unique constraints, and atomic `SaveChangesAsync` execution across aggregates. |
| **11. No External Integrations (Self-Contained)** | **Implemented** | • `TravelRequestService.cs`<br>• `ExpenseService.cs` | • Code inspection | Intentionally zero dependencies on third-party live GDS booking systems, paid SMS gateways, or banking rails; all mock behaviors are self-contained. |

---

## Additional Capabilities Implemented in Codebase

| Capability | Status | Implementing Source Files | Covering Automated Tests | Notes |
| :--- | :---: | :--- | :--- | :--- |
| **Admin Employee Directory & CSV Upload** | **Implemented** | • `EmployeesController.cs`<br>• `EmployeeService.cs`<br>• `employee-management.ts` | • `EmployeesControllerTests.cs`<br>• `EmployeeServiceTests.cs`<br>• `employee-management.spec.ts` | Full CRUD directory, email ReDoS regex protection, CSV batch upload stream parsing, and downloadable CSV template. |
| **Corporate Travel Desk Ticketing** | **Implemented** | • `SupportController.cs`<br>• `SupportService.cs`<br>• `support.ts` | Verified via Controller mapping | Concierge ticketing system (`#TICK-xxxx`), linking travel requests, status tracking, and notes. |
| **Design Token UI System** | **Implemented** | • `src/styles/_tokens.css`<br>• `src/styles/_layout.css`<br>• `src/styles/_components.css` | Verified via visual inspection & build | 4px-base spacing scale, responsive layout classes (`.page`, `.form-grid`, `.card-grid`), and unified status chips. |
