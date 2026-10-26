import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { RequestStatus, TravelRequest } from '../../../core/services/travel-request.service';

interface WorkflowNode {
  id: string;
  label: string;
  sublabel: string;
  roleType: 'employee' | 'decision' | 'approver' | 'terminal' | 'reject';
  icon: string;
}

@Component({
  selector: 'app-workflow-tracker',
  imports: [CommonModule, MatIconModule, MatChipsModule],
  templateUrl: './workflow-tracker.html',
  styleUrl: './workflow-tracker.scss',
})
export class WorkflowTracker {
  @Input({ required: true }) request!: TravelRequest;

  // Key nodes corresponding to the Mermaid workflow diagram
  readonly pipelineSteps: WorkflowNode[] = [
    {
      id: 'draft',
      label: '1. Draft',
      sublabel: 'Employee created',
      roleType: 'employee',
      icon: 'edit_note',
    },
    {
      id: 'policy',
      label: '2. Policy Check',
      sublabel: 'Auto-evaluated',
      roleType: 'decision',
      icon: 'verified_user',
    },
    {
      id: 'manager',
      label: '3. Manager Review',
      sublabel: 'Routing & approval',
      roleType: 'approver',
      icon: 'supervisor_account',
    },
    {
      id: 'finance',
      label: '4. Finance Review',
      sublabel: 'Budget authorization',
      roleType: 'approver',
      icon: 'account_balance',
    },
    {
      id: 'active',
      label: '5. Active & Itinerary',
      sublabel: 'Flights, stays & expenses',
      roleType: 'employee',
      icon: 'flight_takeoff',
    },
    {
      id: 'reimburse',
      label: '6. Expense Review',
      sublabel: 'Proof & reimbursement',
      roleType: 'approver',
      icon: 'receipt_long',
    },
    {
      id: 'closed',
      label: '7. Closed',
      sublabel: 'Trip completed',
      roleType: 'terminal',
      icon: 'task_alt',
    },
  ];

  isCurrent(stepId: string): boolean {
    if (this.request.status === 'Rejected') {
      return false;
    }
    switch (stepId) {
      case 'draft':
        return this.request.status === 'Draft';
      case 'policy':
      case 'manager':
        return this.request.status === 'Submitted';
      case 'finance':
        return this.request.status === 'Manager Approved';
      case 'active':
        return this.request.status === 'Finance Approved' && this.request.expenses.length === 0;
      case 'reimburse':
        return (
          this.request.status === 'Finance Approved' &&
          this.request.expenses.length > 0 &&
          this.request.expenses.some(e => e.status === 'Pending')
        );
      case 'closed':
        return this.request.status === 'Closed';
      default:
        return false;
    }
  }

  isCompleted(stepId: string): boolean {
    const status = this.request.status;
    if (status === 'Closed') return true;

    switch (stepId) {
      case 'draft':
        return status !== 'Draft';
      case 'policy':
        return status !== 'Draft';
      case 'manager':
        return (
          status === 'Manager Approved' ||
          status === 'Finance Approved' ||
          (status === 'Rejected' && this.request.rejectionStage === 'Finance')
        );
      case 'finance':
        return status === 'Finance Approved';
      case 'active':
        return (
          status === 'Finance Approved' &&
          (this.request.itinerary.length > 0 || this.request.expenses.length > 0)
        );
      case 'reimburse':
        return (
          status === 'Finance Approved' &&
          this.request.expenses.length > 0 &&
          this.request.expenses.every(e => e.status !== 'Pending')
        );
      case 'closed':
        return false;
      default:
        return false;
    }
  }

  get showRejectionBranch(): boolean {
    return this.request.status === 'Rejected';
  }
}
