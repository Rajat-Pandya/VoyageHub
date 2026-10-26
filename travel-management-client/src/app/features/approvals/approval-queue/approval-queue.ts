import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import {
  TravelRequestService,
  TravelRequest,
  ExpenseItem,
} from '../../../core/services/travel-request.service';

interface FlatPendingExpense {
  requestId: number;
  employee: string;
  destination: string;
  expense: ExpenseItem;
}

@Component({
  selector: 'app-approval-queue',
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    MatTableModule,
    MatChipsModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './approval-queue.html',
  styleUrl: './approval-queue.scss',
})
export class ApprovalQueue {
  readonly travelService = inject(TravelRequestService);

  readonly stageFilter = signal<string>('all');

  // Pending travel requests needing either Manager or Finance approval
  readonly pendingRequests = computed(() => {
    const list = this.travelService.requests();
    const filter = this.stageFilter();

    const filtered = list.filter(r => r.status === 'Submitted' || r.status === 'Manager Approved');

    if (filter === 'manager') {
      return filtered.filter(r => r.status === 'Submitted');
    }
    if (filter === 'finance') {
      return filtered.filter(r => r.status === 'Manager Approved');
    }
    return filtered;
  });

  // Pending expenses across all travel requests needing Finance review
  readonly pendingExpenses = computed<FlatPendingExpense[]>(() => {
    const list = this.travelService.requests();
    const result: FlatPendingExpense[] = [];

    for (const req of list) {
      for (const exp of req.expenses) {
        if (exp.status === 'Pending') {
          result.push({
            requestId: req.id,
            employee: req.employee,
            destination: req.destination,
            expense: exp,
          });
        }
      }
    }
    return result;
  });

  displayedColumns = [
    'employee',
    'destination',
    'dates',
    'amount',
    'policy',
    'stage',
    'status',
    'actions',
  ];

  expenseDisplayedColumns = [
    'employee',
    'destination',
    'category',
    'amount',
    'proof',
    'actions',
  ];

  quickApprove(request: TravelRequest) {
    if (request.status === 'Submitted') {
      this.travelService.managerApprove(request.id);
    } else if (request.status === 'Manager Approved') {
      this.travelService.financeApprove(request.id);
    }
  }

  quickReject(request: TravelRequest) {
    const reason = prompt('Please provide a mandatory rejection reason:') || '';
    if (!reason.trim()) return;

    if (request.status === 'Submitted') {
      this.travelService.managerReject(request.id, reason);
    } else {
      this.travelService.financeReject(request.id, reason);
    }
  }

  approveExpense(item: FlatPendingExpense) {
    this.travelService.reviewExpense(item.requestId, item.expense.id, true);
  }

  rejectExpense(item: FlatPendingExpense) {
    const reason = prompt('Reason for rejecting this expense:') || 'Non-compliant receipt';
    this.travelService.reviewExpense(item.requestId, item.expense.id, false, reason);
  }
}
