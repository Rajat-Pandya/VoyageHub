import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VoyageService, ApprovalQueueItem } from '../../services/voyage.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-approval-queue',
  imports: [CommonModule, CurrencyPipe, FormsModule],
  templateUrl: './approval-queue.html',
  styleUrl: './approval-queue.css'
})
export class ApprovalQueue implements OnInit {
  private readonly voyageService = inject(VoyageService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly items = signal<ApprovalQueueItem[]>([]);
  readonly pendingTotal = signal<number>(0);
  readonly avgResponseTime = signal<number>(0);
  readonly policyCompliance = signal<number>(100);
  readonly isLoading = signal(true);

  // Modal action
  readonly selectedItem = signal<ApprovalQueueItem | null>(null);
  readonly actionType = signal<'Approve' | 'Reject'>('Approve');
  actionComment = '';

  ngOnInit(): void {
    this.loadQueue();
  }

  loadQueue(): void {
    this.isLoading.set(true);
    this.voyageService.getApprovalQueue().subscribe({
      next: (res) => {
        this.items.set(res?.items || []);
        this.pendingTotal.set(res?.pendingTotal || 0);
        this.avgResponseTime.set(res?.avgResponseTimeHours || 0);
        this.policyCompliance.set(res?.policyCompliancePercent ?? 100);
        this.isLoading.set(false);
      },
      error: () => {
        this.items.set([]);
        this.pendingTotal.set(0);
        this.avgResponseTime.set(0);
        this.policyCompliance.set(100);
        this.isLoading.set(false);
      }
    });
  }

  viewDetail(item: ApprovalQueueItem): void {
    void this.router.navigate(['/requests', item.requestId]);
  }

  openActionModal(item: ApprovalQueueItem, action: 'Approve' | 'Reject'): void {
    this.selectedItem.set(item);
    this.actionType.set(action);
    this.actionComment = action === 'Approve' ? 'Approved for business needs.' : 'Budget exceeds threshold.';
  }

  closeModal(): void {
    this.selectedItem.set(null);
  }

  confirmAction(): void {
    const item = this.selectedItem();
    if (!item) return;

    this.voyageService.takeApprovalAction(item.stepId, this.actionType(), this.actionComment).subscribe({
      next: () => {
        this.closeModal();
        this.loadQueue();
      },
      error: () => {
        this.closeModal();
        this.loadQueue();
      }
    });
  }
}
