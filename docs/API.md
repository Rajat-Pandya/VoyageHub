# VoyageHub API Endpoint Reference

This document provides a comprehensive catalog of all REST endpoints exposed by the VoyageHub ASP.NET Core 10 Web API (`VoyageHub.Api`).

> [!NOTE]
> **OpenAPI / Swagger UI Availability**: Swagger UI is enabled by default in the `Development` environment at `http://localhost:5062/swagger`. In `Production`, `Swagger:Enabled` defaults to `false`, unmapping Swagger endpoints and configuring the root route `/` to return a simple HTTP 200 text status (`"VoyageHub API is running."`).

---

## 1. Authentication Endpoints (`/api/Auth`)

| Method | Path | Required Role | Description | Request Body | Response (Success) | Error Codes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/Auth/login` | Anonymous | Authenticates with email and password, returning user profile and signed JWT token. | `{ "email": string, "password": string }` | `200 OK` `{ "success": true, "token": string, "user": UserDto }` | `400`, `401` |
| `GET` | `/api/Auth/personas` | Anonymous | Lists pre-seeded personas for development persona switcher. | None | `200 OK` `UserDto[]` | None |
| `PUT` | `/api/Auth/profile` | Any Authenticated | Updates the caller's display name and avatar URL. | `{ "fullName": string, "avatarUrl": string }` | `200 OK` `UserDto` | `400`, `401` |
| `POST` | `/api/Auth/change-password` | Any Authenticated | Validates existing password and hashes new password via BCrypt. | `{ "currentPassword": string, "newPassword": string }` | `200 OK` `{ "success": true, "message": string }` | `400`, `401` |
| `POST` | `/api/Auth/forgot-password` | Anonymous | Generates password reset token for registered email. | `{ "email": string }` | `200 OK` `{ "success": true, "message": string }` | `400`, `404` |
| `POST` | `/api/Auth/verify-reset-token`| Anonymous | Checks validity of password reset token. | `{ "email": string, "token": string }` | `200 OK` `{ "success": true, "message": string }` | `400` |
| `POST` | `/api/Auth/reset-password` | Anonymous | Resets password using valid reset token. | `{ "email": string, "token": string, "newPassword": string }` | `200 OK` `{ "success": true, "message": string }` | `400` |

---

## 2. Travel Requests Endpoints (`/api/TravelRequests`)

| Method | Path | Required Role | Description | Request Body | Response (Success) | Error Codes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/TravelRequests` | Employee, Manager, Finance, Admin | Retrieves travel requests. Scoped strictly to caller's own requests for Employees; scoped to direct reports for Managers; full access for Finance/Admin. | Query params: `status`, `search` | `200 OK` `TravelRequestDto[]` | `401`, `403` |
| `GET` | `/api/TravelRequests/stats` | Employee, Manager, Finance, Admin | Computes dashboard KPI aggregates (drafts, pending, upcoming trips, pending reimbursement amount). | None | `200 OK` `DashboardStatsDto` | `401`, `403` |
| `GET` | `/api/TravelRequests/{id}` | Employee, Manager, Finance, Admin | Retrieves request details including itineraries, stays, expenses, and approval steps. | None | `200 OK` `TravelRequestDetailDto` | `401`, `403`, `404` |
| `POST` | `/api/TravelRequests` | Employee, Manager, Admin | Creates a new travel request. Evaluates department budget policy; auto-routes to In Review if `saveAsDraft = false`. | `CreateTravelRequestDto` | `201 Created` `TravelRequestDto` | `400`, `401`, `403` |
| `DELETE`| `/api/TravelRequests/{id}` | Employee, Manager, Admin | Cancels an active travel request. | None | `200 OK` `{ "success": true, "message": string }` | `400`, `401`, `403`, `404` |
| `POST` | `/api/TravelRequests/{id}/submit` | Employee, Manager, Admin | Submits a Draft request into Manager Review and instantiates approval steps. | None | `200 OK` `{ "success": true, "message": string }` | `400`, `401`, `403`, `404` |
| `POST` | `/api/TravelRequests/{id}/segments` | Employee, Manager, Admin | Adds an itinerary segment (flight, train, etc.) to request. | `CreateSegmentDto` | `200 OK` `ItinerarySegmentDto` | `400`, `401`, `403`, `404` |
| `POST` | `/api/TravelRequests/{id}/stays` | Employee, Manager, Admin | Adds a hotel stay reservation to request. | `CreateStayDto` | `200 OK` `StayDto` | `400`, `401`, `403`, `404` |
| `POST` | `/api/TravelRequests/{id}/close` | Employee, Manager, Finance, Admin | Closes an Approved travel request upon trip completion. | None | `200 OK` `{ "success": true, "message": string }` | `400`, `401`, `403`, `404` |

---

## 3. Approvals Endpoints (`/api/Approvals`)

| Method | Path | Required Role | Description | Request Body | Response (Success) | Error Codes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Approvals/queue` | Manager, Finance, Admin | Retrieves pending approval steps filtered by caller's role (Manager / Finance) and ID. | None | `200 OK` `ApprovalQueueItemDto[]` | `401`, `403` |
| `POST` | `/api/Approvals/{stepId}/action` | Manager, Finance, Admin | Approves or rejects an approval step. Advances Manager $\rightarrow$ Finance, finalizes Finance $\rightarrow$ Approved, or cascades cancellation on rejection. | `{ "action": "Approve" \| "Reject", "comment": string }` | `200 OK` `{ "success": true, "message": string }` | `400`, `401`, `403`, `404` |

---

## 4. Expenses & Reimbursements Endpoints (`/api/Expenses`)

| Method | Path | Required Role | Description | Request Body | Response (Success) | Error Codes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Expenses/pending` | Finance, Admin | Retrieves pending expense claims summary with totals and claim items. | Query params: `category`, `highPriorityOnly` | `200 OK` `ExpenseReviewSummaryDto` | `401`, `403` |
| `POST` | `/api/Expenses` | Employee, Manager, Finance, Admin | Submits an itemized expense claim attached to a travel request. | `SubmitExpenseDto` | `200 OK` `ExpenseDto` | `400`, `401`, `403` |
| `POST` | `/api/Expenses/{id}/action` | Finance, Admin | Approves (`Reimbursed`) or rejects (`Rejected`) an individual expense claim. | `{ "action": "Approve" \| "Reject" }` | `200 OK` `{ "success": true, "message": string }` | `400`, `401`, `403`, `404` |
| `POST` | `/api/Expenses/batch-payout` | Finance, Admin | Reimburses multiple selected pending expense claims in a single transaction. | `{ "expenseIds": Guid[] }` | `200 OK` `{ "success": true, "message": string }` | `400`, `401`, `403` |
| `POST` | `/api/Expenses/upload-proof` | Employee, Manager, Finance, Admin | Uploads a receipt document (.pdf, .jpg, .jpeg, .png, .webp $\le 15$MB). | `multipart/form-data` (`file`) | `200 OK` `{ "success": true, "proofPath": string, "fileName": string, "fileUrl": string }` | `400`, `401`, `403` |
| `POST` | `/api/Expenses/{id}/proof` | Employee, Manager, Finance, Admin | Uploads and directly attaches a receipt proof to an existing expense claim. | `multipart/form-data` (`file`) | `200 OK` `{ "success": true, "proofPath": string, "fileName": string, "fileUrl": string }` | `400`, `401`, `403`, `404` |
| `GET` | `/api/Expenses/proof/{fileName}` | Employee, Manager, Finance, Admin | Streams the uploaded receipt document with proper MIME type and range support. | None | `200 OK` `FileStreamResult` | `400`, `401`, `403`, `404` |

---

## 5. Employee Directory Management (`/api/Employees`)

| Method | Path | Required Role | Description | Request Body | Response (Success) | Error Codes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Employees` | Admin | Retrieves employee directory records with search and filtering. | Query params: `search`, `department`, `role` | `200 OK` `EmployeeDto[]` | `401`, `403` |
| `GET` | `/api/Employees/{id}` | Admin | Retrieves a single employee profile by ID. | None | `200 OK` `EmployeeDto` | `401`, `403`, `404` |
| `POST` | `/api/Employees` | Admin | Creates a new employee with department and optional manager. | `CreateEmployeeDto` | `201 Created` `EmployeeDto` | `400`, `401`, `403` |
| `PUT` | `/api/Employees/{id}` | Admin | Updates employee profile information. | `UpdateEmployeeDto` | `200 OK` `EmployeeDto` | `400`, `401`, `403`, `404` |
| `DELETE`| `/api/Employees/{id}` | Admin | Deletes an employee record. | None | `200 OK` `{ "success": true, "message": string }` | `400`, `401`, `403`, `404` |
| `POST` | `/api/Employees/batch-upload` | Admin | Parses a CSV stream and batch creates employee records. | `multipart/form-data` (`file`) | `200 OK` `BatchUploadResultDto` | `400`, `401`, `403` |
| `GET` | `/api/Employees/csv-template` | Anonymous | Downloads a standard CSV employee template file (`employees_template.csv`). | None | `200 OK` `FileContentResult` (`text/csv`) | None |

---

## 6. Travel Policies Endpoints (`/api/Policies`)

| Method | Path | Required Role | Description | Request Body | Response (Success) | Error Codes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Policies` | Employee, Manager, Finance, Admin | Retrieves all department travel policies. | None | `200 OK` `TravelPolicyDto[]` | `401`, `403` |
| `POST` | `/api/Policies` | Admin | Creates a new department travel policy (enforces 1 policy per department). | `CreatePolicyDto` | `200 OK` `TravelPolicyDto` | `400`, `401`, `403` |
| `PUT` | `/api/Policies/{id}` | Admin | Updates travel budget limits, allowed flight classes, and enforcement. | `UpdatePolicyDto` | `200 OK` `TravelPolicyDto` | `400`, `401`, `403`, `404` |
| `DELETE`| `/api/Policies/{id}` | Admin | Deletes a travel policy while preserving the department record. | None | `200 OK` `{ "success": true, "message": string }` | `400`, `401`, `403`, `404` |

---

## 7. Support Desk Endpoints (`/api/Support`)

| Method | Path | Required Role | Description | Request Body | Response (Success) | Error Codes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/Support/tickets` | Employee, Manager, Finance, Admin | Lists support tickets filtered by user, status, or search query. | Query params: `status`, `search` | `200 OK` `SupportTicketDto[]` | `401`, `403` |
| `GET` | `/api/Support/tickets/{id}` | Employee, Manager, Finance, Admin | Retrieves ticket details by ID. | None | `200 OK` `SupportTicketDto` | `401`, `403`, `404` |
| `POST` | `/api/Support/tickets` | Employee, Manager, Finance, Admin | Opens a new support inquiry ticket (`#TICK-xxxx`). | `CreateSupportTicketDto` | `200 OK` `SupportTicketDto` | `400`, `401`, `403` |
| `POST` | `/api/Support/tickets/{id}/status` | Manager, Finance, Admin | Updates resolution status and notes for a ticket. | `UpdateTicketStatusDto` | `200 OK` `SupportTicketDto` | `400`, `401`, `403`, `404` |

---

## 8. Audit Logs & Analytics Endpoints (`/api/AuditLogs`, `/api/Analytics`)

| Method | Path | Required Role | Description | Request Body | Response (Success) | Error Codes |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/AuditLogs` | Admin | Retrieves system audit logs filtered by entity type and severity. | Query params: `entityType`, `severity` | `200 OK` `AuditLogDto[]` | `401`, `403` |
| `GET` | `/api/Analytics/overview` | Manager, Finance, Admin | Retrieves aggregate spend metrics, monthly trends, and category breakdowns. | None | `200 OK` `AnalyticsOverviewDto` | `401`, `403` |
| `GET` | `/api/Analytics/employee-reports` | Manager, Finance, Admin | Retrieves historical employee travel and expense totals. | None | `200 OK` `EmployeeSpendReportDto[]` | `401`, `403` |
| `GET` | `/api/Analytics/cost-reports` | Manager, Finance, Admin | Retrieves departmental budget utilization and cost breakdown. | None | `200 OK` `DepartmentCostReportDto[]` | `401`, `403` |
