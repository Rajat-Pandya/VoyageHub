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
  readonly pendingTotal = signal<number>(145000.00);
  readonly avgResponseTime = signal<number>(4.2);
  readonly policyCompliance = signal<number>(94);
  readonly isLoading = signal(true);

  // Modal action
  readonly selectedItem = signal<ApprovalQueueItem | null>(null);
  readonly actionType = signal<'Approve' | 'Reject'>('Approve');
  actionComment = '';

  ngOnInit(): void {
    this.loadQueue();
  }

  private readonly fallbackQueue: ApprovalQueueItem[] = [
    {
      stepId: '4455b3f9-30f6-4f5e-a733-d94f1dcbf540',
      requestId: 'aa010101-0000-0000-0000-000000000001',
      requestCode: '#TR-2048',
      employeeName: 'Rohan Sharma',
      departmentName: 'Sales & Business Dev (Mumbai HQ)',
      employeeAvatar: 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=120&h=120&q=80',
      destination: 'Bengaluru, Karnataka',
      purpose: 'Client Review & Tech Summit',
      travelDates: '18 Jun – 22 Jun',
      budget: 42500.00,
      policyStatus: 'Flagged',
      status: 'Pending',
      level: 'Manager'
    },
    {
      stepId: '77ba5631-e076-4234-a0b0-473305fcb622',
      requestId: 'aa060606-0000-0000-0000-000000000006',
      requestCode: '#TR-2055',
      employeeName: 'Sarah Chen',
      departmentName: 'Engineering & Cloud Architecture',
      employeeAvatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=120&h=120&q=80',
      destination: 'San Francisco, CA',
      purpose: 'AWS Global Summit',
      travelDates: '12 Aug – 16 Aug',
      budget: 185000.00,
      policyStatus: 'Standard',
      status: 'Pending',
      level: 'Manager'
    }
  ];

  loadQueue(): void {
    this.isLoading.set(true);
    this.voyageService.getApprovalQueue().subscribe({
      next: (res) => {
        if (res && res.items && res.items.length > 0) {
          this.items.set(res.items);
          this.pendingTotal.set(res.pendingTotal);
          this.avgResponseTime.set(res.avgResponseTimeHours);
          this.policyCompliance.set(res.policyCompliancePercent);
        } else {
          this.items.set(this.fallbackQueue);
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.items.set(this.fallbackQueue);
        this.isLoading.set(false);
      }
    });
  }

  viewDetail(item: ApprovalQueueItem): void {
    this.router.navigate(['/requests', item.requestId]);
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
