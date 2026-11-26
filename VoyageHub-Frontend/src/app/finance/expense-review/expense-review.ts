import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { VoyageService, ExpenseItem } from '../../services/voyage.service';
import { getUserInitials } from '../../services/auth.service';

@Component({
  selector: 'app-expense-review',
  imports: [CommonModule, CurrencyPipe, DatePipe, RouterLink, FormsModule],
  templateUrl: './expense-review.html',
  styleUrl: './expense-review.css'
})
export class ExpenseReview implements OnInit {
  private readonly voyageService = inject(VoyageService);
  private readonly sanitizer = inject(DomSanitizer);
  readonly getUserInitials = getUserInitials;

  readonly claims = signal<ExpenseItem[]>([]);
  readonly totalPending = signal<number>(0);
  readonly isLoading = signal(true);

  selectedCategory = 'All Categories';
  selectedStatus = 'Pending';
  highPriorityOnly = false;

  // Receipt preview modal
  readonly previewClaim = signal<ExpenseItem | null>(null);
  // Reject reason modal
  readonly rejectTarget = signal<ExpenseItem | null>(null);
  rejectionReason = '';

  ngOnInit(): void {
    this.loadExpenses();
  }

  loadExpenses(): void {
    this.isLoading.set(true);
    this.voyageService.getPendingExpenses(this.selectedCategory, this.highPriorityOnly, this.selectedStatus).subscribe({
      next: (res) => {
        this.claims.set(res?.pendingClaims || []);
        this.totalPending.set(res?.totalPendingAmount || 0);
        this.isLoading.set(false);
      },
      error: () => {
        this.claims.set([]);
        this.totalPending.set(0);
        this.isLoading.set(false);
      }
    });
  }

  takeAction(claim: ExpenseItem, action: 'Approve' | 'Reject', comment?: string): void {
    this.voyageService.takeExpenseAction(claim.id, action, comment).subscribe({
      next: () => this.loadExpenses(),
      error: () => this.loadExpenses()
    });
  }

  openRejectModal(claim: ExpenseItem): void {
    this.rejectTarget.set(claim);
    this.rejectionReason = '';
  }

  closeRejectModal(): void {
    this.rejectTarget.set(null);
    this.rejectionReason = '';
  }

  confirmReject(): void {
    const claim = this.rejectTarget();
    if (!claim) return;
    const reason = this.rejectionReason.trim() || 'Rejected by Finance';
    this.closeRejectModal();
    this.takeAction(claim, 'Reject', reason);
  }

  exportCsv(): void {
    const status = this.selectedStatus === 'All' ? 'Reimbursed' : this.selectedStatus;
    this.voyageService.exportPayoutCsv(status).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `voyagehub_payout_${status.toLowerCase()}.csv`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.showToast('Failed to export payout CSV.')
    });
  }

  readonly toastMessage = signal<string | null>(null);

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => {
      this.toastMessage.set(null);
    }, 3000);
  }

  batchPayout(): void {
    this.voyageService.batchPayout().subscribe({
      next: () => {
        this.showToast('Batch payout processed successfully! Claims marked as reimbursed.');
        this.loadExpenses();
      },
      error: () => this.loadExpenses()
    });
  }

  openReceiptModal(claim: ExpenseItem): void {
    this.previewClaim.set(claim);
  }

  closeReceiptModal(): void {
    this.previewClaim.set(null);
  }

  getProofUrl(proofPath?: string): string {
    return this.voyageService.getProofUrl(proofPath);
  }

  getSafeProofUrl(proofPath?: string): SafeResourceUrl | null {
    const url = this.getProofUrl(proofPath);
    return url ? this.sanitizer.bypassSecurityTrustResourceUrl(url) : null;
  }

  isImageProof(proofPath?: string): boolean {
    return this.voyageService.isImageProof(proofPath);
  }

  isPdfProof(proofPath?: string): boolean {
    return this.voyageService.isPdfProof(proofPath);
  }
}
