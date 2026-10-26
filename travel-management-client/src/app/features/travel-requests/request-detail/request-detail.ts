import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Timeline } from '../../../shared/components/timeline/timeline';
import { WorkflowTracker } from '../../../shared/components/workflow-tracker/workflow-tracker';
import {
  TravelRequestService,
  TravelRequest,
  UserRole,
} from '../../../core/services/travel-request.service';

@Component({
  selector: 'app-request-detail',
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    MatCardModule,
    MatChipsModule,
    MatStepperModule,
    MatTabsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    Timeline,
    WorkflowTracker,
  ],
  templateUrl: './request-detail.html',
  styleUrl: './request-detail.scss',
})
export class RequestDetail {
  private route = inject(ActivatedRoute);
  readonly travelService = inject(TravelRequestService);

  readonly requestId = computed(() => {
    const param = this.route.snapshot.paramMap.get('id');
    return param ? Number(param) : 1;
  });

  // Current request reactive computation from service
  readonly request = computed<TravelRequest>(() => {
    const id = this.requestId();
    const found = this.travelService.getRequestById(id);
    if (found) return found;
    // Fallback if id not found: pick first request
    return this.travelService.requests()[0];
  });

  // Role simulation
  get currentUserRole(): UserRole {
    return this.travelService.currentUserRole();
  }

  setRole(role: UserRole) {
    this.travelService.setRole(role);
  }

  // Stepper representation
  readonly steps = ['Submitted', 'Manager Approved', 'Finance Approved', 'Closed'];

  get isRejected(): boolean {
    return this.request().status === 'Rejected';
  }

  get currentStepIndex(): number {
    if (this.isRejected) return -1;
    return this.steps.indexOf(this.request().status);
  }

  // Permissions based on workflow nodes
  get canSubmitDraft(): boolean {
    return this.request().status === 'Draft';
  }

  get canApprove(): boolean {
    const status = this.request().status;
    const role = this.currentUserRole;
    if (role === 'Manager' && status === 'Submitted') return true;
    if (role === 'Finance' && status === 'Manager Approved') return true;
    if (role === 'Admin') return status === 'Submitted' || status === 'Manager Approved';
    return false;
  }

  get isRequestActive(): boolean {
    const status = this.request().status;
    return status === 'Finance Approved' || status === 'Closed';
  }

  get canCloseRequest(): boolean {
    const req = this.request();
    if (req.status !== 'Finance Approved') return false;
    // Can close if at least one itinerary/expense and no expenses are still pending
    const hasPendingExpense = req.expenses.some(e => e.status === 'Pending');
    return !hasPendingExpense;
  }

  // Rejection state
  showRejectForm = false;
  rejectComment = '';

  // New Itinerary form state
  showAddItinerary = false;
  newItinerary = {
    type: 'Flight' as 'Flight' | 'Train' | 'Stay' | 'Cab',
    details: '',
    dates: '',
    confirmationCode: '',
  };

  // New Expense form state
  showAddExpense = false;
  newExpense = {
    category: 'Transport' as 'Transport' | 'Stay' | 'Meals' | 'Client Entertainment' | 'Incidental',
    amount: null as number | null,
    proofName: 'receipt_ticket.pdf',
    notes: '',
  };

  // Table columns
  itineraryColumns = ['type', 'details', 'dates', 'confirmationCode'];
  expenseColumns = ['category', 'amount', 'proof', 'status', 'actions'];

  // Workflow Action Handlers
  submitDraft() {
    this.travelService.submitDraft(this.request().id);
  }

  approve() {
    const req = this.request();
    if (this.currentUserRole === 'Manager' || (this.currentUserRole === 'Admin' && req.status === 'Submitted')) {
      this.travelService.managerApprove(req.id);
    } else if (this.currentUserRole === 'Finance' || (this.currentUserRole === 'Admin' && req.status === 'Manager Approved')) {
      this.travelService.financeApprove(req.id);
    }
    this.showRejectForm = false;
  }

  reject() {
    if (!this.rejectComment.trim()) return;
    const req = this.request();
    if (this.currentUserRole === 'Manager' || (this.currentUserRole === 'Admin' && req.status === 'Submitted')) {
      this.travelService.managerReject(req.id, this.rejectComment);
    } else {
      this.travelService.financeReject(req.id, this.rejectComment);
    }
    this.showRejectForm = false;
    this.rejectComment = '';
  }

  cancelReject() {
    this.showRejectForm = false;
    this.rejectComment = '';
  }

  addItinerarySegment() {
    if (!this.newItinerary.details.trim() || !this.newItinerary.dates.trim()) return;
    this.travelService.addItinerary(this.request().id, {
      type: this.newItinerary.type,
      details: this.newItinerary.details,
      dates: this.newItinerary.dates,
      confirmationCode: this.newItinerary.confirmationCode || undefined,
    });
    this.showAddItinerary = false;
    this.newItinerary = {
      type: 'Flight',
      details: '',
      dates: '',
      confirmationCode: '',
    };
  }

  submitExpenseItem() {
    if (!this.newExpense.amount || this.newExpense.amount <= 0) return;
    this.travelService.submitExpense(this.request().id, {
      category: this.newExpense.category,
      amount: this.newExpense.amount,
      proof: this.newExpense.proofName,
      notes: this.newExpense.notes,
    });
    this.showAddExpense = false;
    this.newExpense = {
      category: 'Transport',
      amount: null,
      proofName: 'receipt_ticket.pdf',
      notes: '',
    };
  }

  approveExpense(expenseId: number) {
    this.travelService.reviewExpense(this.request().id, expenseId, true);
  }

  rejectExpense(expenseId: number) {
    this.travelService.reviewExpense(this.request().id, expenseId, false, 'Does not comply with itemized meal cap');
  }

  closeTravelRequest() {
    this.travelService.closeRequest(this.request().id);
  }
}
