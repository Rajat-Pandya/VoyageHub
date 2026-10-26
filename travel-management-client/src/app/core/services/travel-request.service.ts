import { Injectable, inject, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { catchError, of, tap } from 'rxjs';

export type RequestStatus =
  | 'Draft'
  | 'Submitted'
  | 'Manager Approved'
  | 'Finance Approved'
  | 'Rejected'
  | 'Closed';

export type UserRole = 'Employee' | 'Manager' | 'Finance' | 'Admin';

export interface BackendTravelRequest {
  travelRequestId: number;
  employeeName: string;
  destination: string;
  startDate: string;
  endDate: string;
  tripDays?: number;
  purpose: string;
  estimatedBudget: number;
  status: string;
  createdOn: string;
}

export interface ItineraryItem {
  id: number;
  type: 'Flight' | 'Train' | 'Stay' | 'Cab';
  details: string;
  dates: string;
  confirmationCode?: string;
}

export interface ExpenseItem {
  id: number;
  category: 'Transport' | 'Stay' | 'Meals' | 'Client Entertainment' | 'Incidental';
  amount: string;
  numericAmount: number;
  proof: string;
  status: 'Pending' | 'Reimbursed' | 'Rejected';
  notes?: string;
  submittedAt: string;
}

export interface TimelineEvent {
  actor: string;
  action: string;
  timestamp: string;
  note?: string;
}

export interface PolicyCheckResult {
  passed: boolean;
  message: string;
  exceedsBudget: boolean;
}

export interface TravelRequest {
  id: number;
  employee: string;
  department: string;
  destination: string;
  dates: string;
  startDate?: string;
  endDate?: string;
  purpose: string;
  estimatedBudget: number;
  status: RequestStatus;
  rejectionComment?: string;
  rejectionStage?: 'Manager' | 'Finance';
  policyCheck: PolicyCheckResult;
  itinerary: ItineraryItem[];
  expenses: ExpenseItem[];
  history: TimelineEvent[];
}

@Injectable({
  providedIn: 'root',
})
export class TravelRequestService {
  private http = inject(HttpClient);

  // Backend API Base URL (.NET Web API running on port 5201)
  readonly apiBaseUrl = 'http://localhost:5201/api/TravelRequest';

  readonly budgetPolicyLimit = 50000;

  // Active user simulation for demoing the workflow
  readonly currentUserRole = signal<UserRole>('Employee');
  readonly currentUserName = signal<string>('Rajat Sharma');

  // Backend connection status
  readonly isBackendConnected = signal<boolean>(false);

  // Master list of requests across workflow stages
  private readonly _requests = signal<TravelRequest[]>([
    {
      id: 1,
      employee: 'Rajat Sharma',
      department: 'Corporate Travel',
      destination: 'Mumbai',
      dates: '12 Nov – 15 Nov',
      startDate: '2026-11-12',
      endDate: '2026-11-15',
      purpose: 'Client meeting — Q4 contract review',
      estimatedBudget: 45000,
      status: 'Manager Approved',
      policyCheck: {
        passed: true,
        message: 'Within department travel budget guidelines (under ₹50,000)',
        exceedsBudget: false,
      },
      itinerary: [
        { id: 1, type: 'Flight', details: 'Vadodara → Mumbai (6E 421)', dates: '12 Nov, 08:00', confirmationCode: '6E-VJ98' },
        { id: 2, type: 'Stay', details: 'Taj Lands End, Bandra West', dates: '12 Nov – 15 Nov', confirmationCode: 'TAJ-7721' },
      ],
      expenses: [
        {
          id: 1,
          category: 'Transport',
          amount: '₹1,200',
          numericAmount: 1200,
          proof: 'airport_uber_receipt.pdf',
          status: 'Pending',
          submittedAt: '12 Nov',
        },
        {
          id: 2,
          category: 'Stay',
          amount: '₹6,000',
          numericAmount: 6000,
          proof: 'hotel_invoice_final.pdf',
          status: 'Reimbursed',
          submittedAt: '15 Nov',
        },
      ],
      history: [
        { actor: 'Rajat Sharma (Employee)', action: 'Created draft travel request', timestamp: '04 Oct, 04:30 PM' },
        { actor: 'Rajat Sharma (Employee)', action: 'Submitted request for approval', timestamp: '05 Oct, 10:12 AM' },
        { actor: 'System (Policy Engine)', action: 'Policy check passed: Budget is within ₹50,000 threshold', timestamp: '05 Oct, 10:12 AM' },
        { actor: 'Priya Nair (Manager)', action: 'Approved travel request. Routed to Finance.', timestamp: '06 Oct, 09:40 AM' },
      ],
    },
    {
      id: 2,
      employee: 'Rajat Sharma',
      department: 'Corporate Travel',
      destination: 'Bangalore',
      dates: '20 Nov – 22 Nov',
      startDate: '2026-11-20',
      endDate: '2026-11-22',
      purpose: 'Annual Engineering & Tech Summit',
      estimatedBudget: 32000,
      status: 'Draft',
      policyCheck: {
        passed: true,
        message: 'Draft pending submission policy evaluation',
        exceedsBudget: false,
      },
      itinerary: [],
      expenses: [],
      history: [
        { actor: 'Rajat Sharma (Employee)', action: 'Saved new request as draft', timestamp: '10 Oct, 02:15 PM' },
      ],
    },
    {
      id: 3,
      employee: 'Neha Gupta',
      department: 'Sales',
      destination: 'Hyderabad',
      dates: '14 Nov – 16 Nov',
      startDate: '2026-11-14',
      endDate: '2026-11-16',
      purpose: 'Key Enterprise Client Demo & Pitch',
      estimatedBudget: 18500,
      status: 'Submitted',
      policyCheck: {
        passed: true,
        message: 'Within policy parameters',
        exceedsBudget: false,
      },
      itinerary: [
        { id: 1, type: 'Flight', details: 'BOM → HYD (AI 631)', dates: '14 Nov, 07:15', confirmationCode: 'AI-H882' }
      ],
      expenses: [],
      history: [
        { actor: 'Neha Gupta (Employee)', action: 'Submitted travel request', timestamp: '20 Oct, 09:30 AM' },
        { actor: 'System (Policy Engine)', action: 'Policy check passed', timestamp: '20 Oct, 09:30 AM' },
      ],
    },
    {
      id: 4,
      employee: 'Rohan Mehta',
      department: 'Marketing',
      destination: 'Singapore',
      dates: '22 Nov – 27 Nov',
      startDate: '2026-11-22',
      endDate: '2026-11-27',
      purpose: 'APAC Partner Summit keynote presentation',
      estimatedBudget: 84000,
      status: 'Finance Approved',
      policyCheck: {
        passed: false,
        message: 'International travel requires VP sign-off (exceeds standard domestic cap)',
        exceedsBudget: true,
      },
      itinerary: [
        { id: 1, type: 'Flight', details: 'BOM → SIN (SQ 423)', dates: '22 Nov, 23:45', confirmationCode: 'SQ-9941' },
        { id: 2, type: 'Stay', details: 'Marina Bay Sands', dates: '23 Nov – 27 Nov', confirmationCode: 'MBS-3091' }
      ],
      expenses: [
        {
          id: 10,
          category: 'Transport',
          amount: '₹3,400',
          numericAmount: 3400,
          proof: 'airport_cab_sg.pdf',
          status: 'Pending',
          submittedAt: '23 Nov',
        },
      ],
      history: [
        { actor: 'Rohan Mehta (Employee)', action: 'Submitted international travel request', timestamp: '12 Oct, 11:20 AM' },
        { actor: 'System (Policy Engine)', action: 'Policy check: Flagged for high-value international travel', timestamp: '12 Oct, 11:20 AM' },
        { actor: 'Kavita Rao (Manager)', action: 'Approved exception based on business need', timestamp: '13 Oct, 04:00 PM' },
        { actor: 'Anand Verma (Finance)', action: 'Approved budget allocation and authorized trip', timestamp: '14 Oct, 02:15 PM' },
      ],
    }
  ]);

  readonly requests = this._requests.asReadonly();

  private platformId = inject(PLATFORM_ID);

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.syncWithBackend();
    }
  }

  // --- Backend Sync Methods ---

  syncWithBackend() {
    this.http
      .get<BackendTravelRequest[]>(this.apiBaseUrl)
      .pipe(
        tap((backendList) => {
          this.isBackendConnected.set(true);
          if (backendList && backendList.length > 0) {
            const merged = this.mergeBackendWithLocal(backendList);
            this._requests.set(merged);
          }
        }),
        catchError((err) => {
          console.warn('Backend API at http://localhost:5201 is offline or unreachable. Using fallback local state.', err);
          this.isBackendConnected.set(false);
          return of(null);
        })
      )
      .subscribe();
  }

  private mergeBackendWithLocal(backendList: BackendTravelRequest[]): TravelRequest[] {
    const currentLocal = this._requests();

    return backendList.map((item) => {
      const existing = currentLocal.find((l) => l.id === item.travelRequestId);
      const policy = this.evaluatePolicy(item.estimatedBudget);

      const startDateFormatted = item.startDate ? new Date(item.startDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' }) : '';
      const endDateFormatted = item.endDate ? new Date(item.endDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' }) : '';
      const datesDisplay = startDateFormatted && endDateFormatted ? `${startDateFormatted} – ${endDateFormatted}` : 'Scheduled';

      return {
        id: item.travelRequestId,
        employee: item.employeeName || 'Rajat Sharma',
        department: existing ? existing.department : 'Corporate Travel',
        destination: item.destination,
        dates: datesDisplay,
        startDate: item.startDate,
        endDate: item.endDate,
        purpose: item.purpose,
        estimatedBudget: item.estimatedBudget,
        status: (item.status || 'Draft') as RequestStatus,
        policyCheck: policy,
        itinerary: existing ? existing.itinerary : [],
        expenses: existing ? existing.expenses : [],
        history: existing
          ? existing.history
          : [
              {
                actor: `${item.employeeName} (Employee)`,
                action: `Created record in database on ${new Date(item.createdOn).toLocaleDateString()}`,
                timestamp: this.getCurrentTimestamp(),
              },
            ],
      };
    });
  }

  getRequestById(id: number | string): TravelRequest | undefined {
    const numId = Number(id);
    return this._requests().find((r) => r.id === numId);
  }

  setRole(role: UserRole) {
    this.currentUserRole.set(role);
  }

  // --- Workflow Actions ---

  evaluatePolicy(estimatedBudget: number): PolicyCheckResult {
    const exceeds = estimatedBudget > this.budgetPolicyLimit;
    return {
      passed: !exceeds,
      exceedsBudget: exceeds,
      message: exceeds
        ? `Exceeds standard policy limit of ₹${this.budgetPolicyLimit.toLocaleString('en-IN')}. Flagged for secondary approver scrutiny.`
        : `Within standard departmental policy threshold (₹${this.budgetPolicyLimit.toLocaleString('en-IN')}).`,
    };
  }

  createRequest(data: {
    destination: string;
    startDate: string;
    endDate: string;
    purpose: string;
    estimatedBudget: number;
    asDraft: boolean;
  }): TravelRequest {
    const policy = this.evaluatePolicy(data.estimatedBudget);
    const newLocalId = Date.now();
    const formattedDates = `${data.startDate} – ${data.endDate}`;
    const status: RequestStatus = data.asDraft ? 'Draft' : 'Submitted';

    const history: TimelineEvent[] = [
      {
        actor: `${this.currentUserName()} (Employee)`,
        action: data.asDraft ? 'Created draft request' : 'Submitted travel request',
        timestamp: this.getCurrentTimestamp(),
      },
    ];

    if (!data.asDraft) {
      history.push({
        actor: 'System (Policy Engine)',
        action: policy.passed
          ? 'Policy check passed'
          : `Policy check: Flagged (budget exceeds ₹${this.budgetPolicyLimit.toLocaleString('en-IN')})`,
        timestamp: this.getCurrentTimestamp(),
      });
    }

    const newReq: TravelRequest = {
      id: newLocalId,
      employee: this.currentUserName(),
      department: 'Corporate Travel',
      destination: data.destination,
      dates: formattedDates,
      startDate: data.startDate,
      endDate: data.endDate,
      purpose: data.purpose,
      estimatedBudget: data.estimatedBudget,
      status,
      policyCheck: policy,
      itinerary: [],
      expenses: [],
      history,
    };

    // Update local state immediately
    this._requests.update((list) => [newReq, ...list]);

    // Send to .NET Web API
    const nowIso = new Date().toISOString();
    const futureIso = new Date(Date.now() + 5 * 86400000).toISOString();

    const payload = {
      employeeName: this.currentUserName(),
      destination: data.destination,
      startDate: nowIso,
      endDate: futureIso,
      purpose: data.purpose,
      estimatedBudget: data.estimatedBudget,
    };

    this.http
      .post<BackendTravelRequest>(this.apiBaseUrl, payload)
      .pipe(
        tap((saved) => {
          this.isBackendConnected.set(true);
          if (saved && saved.travelRequestId) {
            // If submitted directly, also sync status to backend
            if (!data.asDraft) {
              this.syncStatusToBackend(saved.travelRequestId, 'Submitted');
            }
            this._requests.update((list) =>
              list.map((r) =>
                r.id === newLocalId ? { ...r, id: saved.travelRequestId } : r
              )
            );
          }
        }),
        catchError((err) => {
          console.warn('Backend POST failed. Local state retained.', err);
          return of(null);
        })
      )
      .subscribe();

    return newReq;
  }

  submitDraft(id: number) {
    this._requests.update((list) =>
      list.map((req) => {
        if (req.id !== id) return req;
        const policy = this.evaluatePolicy(req.estimatedBudget);
        return {
          ...req,
          status: 'Submitted',
          policyCheck: policy,
          history: [
            ...req.history,
            {
              actor: `${this.currentUserName()} (Employee)`,
              action: 'Submitted draft request for approval',
              timestamp: this.getCurrentTimestamp(),
            },
            {
              actor: 'System (Policy Engine)',
              action: policy.passed
                ? 'Policy check passed'
                : `Policy check: Flagged (budget ₹${req.estimatedBudget.toLocaleString('en-IN')} exceeds threshold)`,
              timestamp: this.getCurrentTimestamp(),
            },
          ],
        };
      })
    );
    this.syncStatusToBackend(id, 'Submitted');
  }

  managerApprove(id: number) {
    this._requests.update((list) =>
      list.map((req) => {
        if (req.id !== id) return req;
        return {
          ...req,
          status: 'Manager Approved',
          history: [
            ...req.history,
            {
              actor: 'Priya Nair (Manager)',
              action: 'Approved travel request. Routed to Finance for budget review.',
              timestamp: this.getCurrentTimestamp(),
            },
          ],
        };
      })
    );
    this.syncStatusToBackend(id, 'Manager Approved');
  }

  managerReject(id: number, comment: string) {
    this._requests.update((list) =>
      list.map((req) => {
        if (req.id !== id) return req;
        return {
          ...req,
          status: 'Rejected',
          rejectionStage: 'Manager',
          rejectionComment: comment,
          history: [
            ...req.history,
            {
              actor: 'Priya Nair (Manager)',
              action: `Rejected request. Reason: ${comment}`,
              timestamp: this.getCurrentTimestamp(),
            },
          ],
        };
      })
    );
    this.syncStatusToBackend(id, 'Rejected');
  }

  financeApprove(id: number) {
    this._requests.update((list) =>
      list.map((req) => {
        if (req.id !== id) return req;
        return {
          ...req,
          status: 'Finance Approved',
          history: [
            ...req.history,
            {
              actor: 'Anand Verma (Finance)',
              action: 'Approved travel request. Request is now Active for booking and expenses.',
              timestamp: this.getCurrentTimestamp(),
            },
          ],
        };
      })
    );
    this.syncStatusToBackend(id, 'Finance Approved');
  }

  financeReject(id: number, comment: string) {
    this._requests.update((list) =>
      list.map((req) => {
        if (req.id !== id) return req;
        return {
          ...req,
          status: 'Rejected',
          rejectionStage: 'Finance',
          rejectionComment: comment,
          history: [
            ...req.history,
            {
              actor: 'Anand Verma (Finance)',
              action: `Rejected request. Reason: ${comment}`,
              timestamp: this.getCurrentTimestamp(),
            },
          ],
        };
      })
    );
    this.syncStatusToBackend(id, 'Rejected');
  }

  private syncStatusToBackend(id: number, status: string) {
    this.http
      .patch(`${this.apiBaseUrl}/${id}/status`, { status })
      .pipe(
        tap(() => this.isBackendConnected.set(true)),
        catchError((err) => {
          console.warn(`Failed to sync status ${status} to backend for req #${id}`, err);
          return of(null);
        })
      )
      .subscribe();
  }

  addItinerary(
    requestId: number,
    item: { type: 'Flight' | 'Train' | 'Stay' | 'Cab'; details: string; dates: string; confirmationCode?: string }
  ) {
    this._requests.update((list) =>
      list.map((req) => {
        if (req.id !== requestId) return req;
        const newItem: ItineraryItem = {
          id: Date.now(),
          ...item,
        };
        return {
          ...req,
          itinerary: [...req.itinerary, newItem],
          history: [
            ...req.history,
            {
              actor: `${req.employee} (Employee)`,
              action: `Added ${item.type} itinerary segment: ${item.details}`,
              timestamp: this.getCurrentTimestamp(),
            },
          ],
        };
      })
    );
  }

  submitExpense(
    requestId: number,
    expense: {
      category: 'Transport' | 'Stay' | 'Meals' | 'Client Entertainment' | 'Incidental';
      amount: number;
      proof: string;
      notes?: string;
    }
  ) {
    this._requests.update((list) =>
      list.map((req) => {
        if (req.id !== requestId) return req;
        const newExpense: ExpenseItem = {
          id: Date.now(),
          category: expense.category,
          numericAmount: expense.amount,
          amount: `₹${expense.amount.toLocaleString('en-IN')}`,
          proof: expense.proof || 'receipt_upload.pdf',
          status: 'Pending',
          notes: expense.notes,
          submittedAt: this.getCurrentTimestamp(),
        };
        return {
          ...req,
          expenses: [...req.expenses, newExpense],
          history: [
            ...req.history,
            {
              actor: `${req.employee} (Employee)`,
              action: `Submitted expense of ₹${expense.amount.toLocaleString('en-IN')} (${expense.category}) with proof`,
              timestamp: this.getCurrentTimestamp(),
            },
          ],
        };
      })
    );
  }

  reviewExpense(requestId: number, expenseId: number, approved: boolean, notes?: string) {
    this._requests.update((list) =>
      list.map((req) => {
        if (req.id !== requestId) return req;
        const updatedExpenses = req.expenses.map((e) => {
          if (e.id !== expenseId) return e;
          return {
            ...e,
            status: (approved ? 'Reimbursed' : 'Rejected') as 'Reimbursed' | 'Rejected',
            notes: notes || e.notes,
          };
        });

        const targetExpense = req.expenses.find((e) => e.id === expenseId);
        const amountStr = targetExpense ? targetExpense.amount : '';

        return {
          ...req,
          expenses: updatedExpenses,
          history: [
            ...req.history,
            {
              actor: 'Anand Verma (Finance)',
              action: approved
                ? `Approved and reimbursed expense of ${amountStr}`
                : `Rejected expense of ${amountStr}${notes ? ': ' + notes : ''}`,
              timestamp: this.getCurrentTimestamp(),
            },
          ],
        };
      })
    );
  }

  closeRequest(id: number) {
    this._requests.update((list) =>
      list.map((req) => {
        if (req.id !== id) return req;
        return {
          ...req,
          status: 'Closed',
          history: [
            ...req.history,
            {
              actor: 'System / Finance',
              action: 'Travel completed and all approved expenses settled. Request closed.',
              timestamp: this.getCurrentTimestamp(),
            },
          ],
        };
      })
    );
    this.syncStatusToBackend(id, 'Closed');
  }

  private getCurrentTimestamp(): string {
    const now = new Date();
    return (
      now.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' }) +
      ', ' +
      now.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true })
    );
  }
}
