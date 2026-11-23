import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TravelRequestItem {
  id: string;
  requestCode: string;
  destination: string;
  purpose: string;
  datesFormatted: string;
  startDate: string;
  endDate: string;
  submittedDate: string;
  estimatedBudget: number;
  approverName: string;
  status: string;
  policyCompliance: string;
  travelClass: string;
  employeeName: string;
  departmentName: string;
}

export interface DashboardStats {
  draftsCount: number;
  pendingCount: number;
  upcomingTripsCount: number;
  totalReimbursement: number;
}

export interface ItinerarySegment {
  id: string;
  mode: string;
  carrier?: string;
  origin: string;
  destination: string;
  departureTime: string;
  arrivalTime?: string;
  confirmationNumber?: string;
  cost: number;
}

export interface StayItem {
  id: string;
  hotelName: string;
  checkIn: string;
  checkOut: string;
  nights: number;
  confirmationNumber?: string;
  cost: number;
}

export interface ExpenseItem {
  id: string;
  requestId: string;
  requestCode: string;
  employeeName: string;
  departmentName: string;
  tripPurpose: string;
  category: string;
  amount: number;
  proofPath?: string;
  status: string;
  submittedAt: string;
}

export interface ApprovalHistoryItem {
  id: string;
  level: string;
  approverName: string;
  status: string;
  comment?: string;
  decidedAt?: string;
}

export interface TravelRequestDetail {
  id: string;
  requestCode: string;
  employeeId: string;
  employeeName: string;
  employeeEmail: string;
  departmentName: string;
  destination: string;
  purpose: string;
  startDate: string;
  endDate: string;
  tripDurationDays: number;
  tripDurationNights: number;
  estimatedBudget: number;
  travelClass: string;
  justification?: string;
  status: string;
  policyCompliance: string;
  submittedDate: string;
  primaryContact: string;
  segments: ItinerarySegment[];
  stays: StayItem[];
  expenses: ExpenseItem[];
  approvalHistory: ApprovalHistoryItem[];
}

export interface ApprovalQueueSummary {
  pendingTotal: number;
  avgResponseTimeHours: number;
  policyCompliancePercent: number;
  items: ApprovalQueueItem[];
}

export interface ApprovalQueueItem {
  stepId: string;
  requestId: string;
  requestCode: string;
  employeeName: string;
  departmentName: string;
  employeeAvatar?: string;
  destination: string;
  purpose: string;
  travelDates: string;
  budget: number;
  policyStatus: string;
  status: string;
  level?: string; // Manager, Finance
}

export interface ExpenseReviewSummary {
  totalPendingAmount: number;
  pendingClaims: ExpenseItem[];
}

export interface AnalyticsOverview {
  totalSpent: number;
  vsLastPeriodPercent: number;
  monthlySpend: { month: string; amount: number }[];
  departmentSpend: { department: string; trips: number; avgCost: number; total: number }[];
  categorySpend: { category: string; percentage: number; amount: number; color: string }[];
}

export interface PolicyConfig {
  id: string;
  departmentId: string;
  departmentName: string;
  intlBudgetLimit: number;
  domesticLimit: number;
  allowedClass: string;
  hardEnforcement: boolean;
}

export interface AuditLogItem {
  id: string;
  timestamp: string;
  entityType: string;
  action: string;
  actor: string;
  actorAvatar?: string;
  changeDetail: string;
  severity: string;
}

@Injectable({
  providedIn: 'root'
})
export class VoyageService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  // Requests
  getRequests(status?: string, search?: string, limit?: number): Observable<TravelRequestItem[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (search) params = params.set('search', search);
    if (limit) params = params.set('limit', limit.toString());

    return this.http.get<TravelRequestItem[]>(`${this.baseUrl}/travelrequests`, { params });
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/travelrequests/stats`);
  }

  getRequestDetail(id: string): Observable<TravelRequestDetail> {
    return this.http.get<TravelRequestDetail>(`${this.baseUrl}/travelrequests/${id}`);
  }

  createRequest(payload: any): Observable<TravelRequestDetail> {
    return this.http.post<TravelRequestDetail>(`${this.baseUrl}/travelrequests`, payload);
  }

  cancelRequest(id: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/travelrequests/${id}`);
  }

  submitDraft(id: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/travelrequests/${id}/submit`, {});
  }

  addSegment(id: string, segment: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/travelrequests/${id}/segments`, segment);
  }

  addStay(id: string, stay: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/travelrequests/${id}/stays`, stay);
  }

  closeRequest(id: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/travelrequests/${id}/close`, {});
  }

  // Approvals (Manager / Finance)
  getApprovalQueue(): Observable<ApprovalQueueSummary> {
    return this.http.get<ApprovalQueueSummary>(`${this.baseUrl}/approvals/queue`);
  }

  takeApprovalAction(stepId: string, action: 'Approve' | 'Reject', comment?: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/approvals/${stepId}/action`, { action, comment });
  }

  // Expenses (Employee submission + Finance review)
  submitExpense(payload: { requestId: string; category: string; amount: number; description?: string; proofPath?: string }): Observable<any> {
    return this.http.post(`${this.baseUrl}/expenses`, payload);
  }

  getPendingExpenses(category?: string, highPriorityOnly?: boolean): Observable<ExpenseReviewSummary> {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    if (highPriorityOnly !== undefined) params = params.set('highPriorityOnly', highPriorityOnly.toString());
    return this.http.get<ExpenseReviewSummary>(`${this.baseUrl}/expenses/pending`, { params });
  }

  takeExpenseAction(id: string, action: 'Approve' | 'Reject'): Observable<any> {
    return this.http.post(`${this.baseUrl}/expenses/${id}/action`, { action });
  }

  batchPayout(expenseIds?: string[]): Observable<any> {
    return this.http.post(`${this.baseUrl}/expenses/batch-payout`, { expenseIds });
  }

  // Policies (Admin)
  getPolicies(): Observable<PolicyConfig[]> {
    return this.http.get<PolicyConfig[]>(`${this.baseUrl}/policies`);
  }

  createPolicy(payload: Partial<PolicyConfig>): Observable<PolicyConfig> {
    return this.http.post<PolicyConfig>(`${this.baseUrl}/policies`, payload);
  }

  updatePolicy(id: string, payload: Partial<PolicyConfig>): Observable<any> {
    return this.http.put(`${this.baseUrl}/policies/${id}`, payload);
  }

  deletePolicy(id: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/policies/${id}`);
  }

  // Employees (Admin Directory & Batch Upload)
  getEmployees(search?: string, department?: string, role?: string): Observable<EmployeeItem[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (department && department !== 'All') params = params.set('department', department);
    if (role && role !== 'All') params = params.set('role', role);
    return this.http.get<EmployeeItem[]>(`${this.baseUrl}/employees`, { params });
  }

  getEmployee(id: string): Observable<EmployeeItem> {
    return this.http.get<EmployeeItem>(`${this.baseUrl}/employees/${id}`);
  }

  createEmployee(payload: CreateEmployeePayload): Observable<EmployeeItem> {
    return this.http.post<EmployeeItem>(`${this.baseUrl}/employees`, payload);
  }

  updateEmployee(id: string, payload: UpdateEmployeePayload): Observable<EmployeeItem> {
    return this.http.put<EmployeeItem>(`${this.baseUrl}/employees/${id}`, payload);
  }

  deleteEmployee(id: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/employees/${id}`);
  }

  batchUploadEmployees(file: File): Observable<BatchUploadResult> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<BatchUploadResult>(`${this.baseUrl}/employees/batch-upload`, formData);
  }

  downloadEmployeeCsvTemplate(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/employees/csv-template`, { responseType: 'blob' });
  }

  // Analytics
  getAnalytics(): Observable<AnalyticsOverview> {
    return this.http.get<AnalyticsOverview>(`${this.baseUrl}/analytics/overview`);
  }

  // Audit Logs
  getAuditLogs(severity?: string, entityType?: string): Observable<AuditLogItem[]> {
    let params = new HttpParams();
    if (severity) params = params.set('severity', severity);
    if (entityType) params = params.set('entityType', entityType);
    return this.http.get<AuditLogItem[]>(`${this.baseUrl}/auditlogs`, { params });
  }

  // Support Tickets
  getSupportTickets(status?: string, userId?: string, search?: string): Observable<SupportTicketItem[]> {
    let params = new HttpParams();
    if (status && status !== 'All') params = params.set('status', status);
    if (userId) params = params.set('userId', userId);
    if (search) params = params.set('search', search);
    return this.http.get<SupportTicketItem[]>(`${this.baseUrl}/support/tickets`, { params });
  }

  createSupportTicket(payload: CreateSupportTicketPayload): Observable<SupportTicketItem> {
    return this.http.post<SupportTicketItem>(`${this.baseUrl}/support/tickets`, payload);
  }
}

export interface SupportTicketItem {
  id: string;
  ticketCode: string;
  category: string;
  priority: 'Low' | 'Medium' | 'High' | 'Urgent';
  requestCode?: string;
  requestId?: string;
  subject: string;
  description: string;
  callbackNumber?: string;
  status: 'Open' | 'In Progress' | 'Resolved';
  assignedAgent?: string;
  resolutionNotes?: string;
  userId?: string;
  userName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSupportTicketPayload {
  category: string;
  priority: string;
  requestCode?: string;
  subject: string;
  description: string;
  callbackNumber?: string;
  userId?: string;
}

export interface EmployeeItem {
  id: string;
  name: string;
  email: string;
  role: string;
  departmentId?: string;
  departmentName?: string;
  managerId?: string;
  managerName?: string;
  avatarUrl?: string;
  createdAt: string;
}

export interface CreateEmployeePayload {
  name: string;
  email: string;
  password?: string;
  role: string;
  departmentId?: string;
  departmentName?: string;
  managerId?: string;
  avatarUrl?: string;
}

export interface UpdateEmployeePayload {
  name: string;
  email: string;
  password?: string;
  role: string;
  departmentId?: string;
  departmentName?: string;
  managerId?: string;
  avatarUrl?: string;
}

export interface BatchUploadResult {
  totalProcessed: number;
  successCount: number;
  failureCount: number;
  errors: string[];
  createdEmployees: EmployeeItem[];
}
