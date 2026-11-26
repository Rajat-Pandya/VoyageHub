import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VoyageService, TravelRequestDetail, ExpenseItem } from '../../services/voyage.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-request-detail',
  imports: [RouterLink, CommonModule, CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './request-detail.html',
  styleUrl: './request-detail.css'
})
export class RequestDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly voyageService = inject(VoyageService);
  readonly authService = inject(AuthService);

  readonly detail = signal<TravelRequestDetail | null>(null);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly toastMessage = signal<string | null>(null);
  readonly activeTab = signal<'Itinerary' | 'Expenses' | 'Approval History'>('Itinerary');

  // Workflow Modals
  readonly showSegmentModal = signal(false);
  readonly showStayModal = signal(false);
  readonly showExpenseModal = signal(false);
  readonly showProofModal = signal(false);
  readonly proofToView = signal<ExpenseItem | null>(null);
  readonly showAttachModal = signal(false);
  readonly targetExpenseForProof = signal<ExpenseItem | null>(null);

  // File Upload State
  readonly selectedProofFile = signal<File | null>(null);
  readonly selectedProofFileName = signal<string | null>(null);
  readonly isUploadingProof = signal<boolean>(false);
  readonly attachFile = signal<File | null>(null);

  // New Segment Model
  segmentForm = {
    mode: 'Flight',
    carrier: '',
    origin: '',
    destination: '',
    departureTime: '',
    cost: 0,
    confirmationNumber: ''
  };

  // New Stay Model
  stayForm = {
    hotelName: '',
    checkIn: '',
    checkOut: '',
    nights: 1,
    cost: 0,
    confirmationNumber: ''
  };

  // New Expense Model
  expenseForm = {
    category: 'Meals',
    amount: 0,
    description: '',
    proofPath: ''
  };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDetail(id);
    } else {
      this.isLoading.set(false);
      this.detail.set(null);
    }
  }

  loadDetail(id: string): void {
    this.isLoading.set(true);
    this.voyageService.getRequestDetail(id).subscribe({
      next: (data) => {
        this.detail.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.detail.set(null);
        this.isLoading.set(false);
      }
    });
  }

  setTab(tab: 'Itinerary' | 'Expenses' | 'Approval History'): void {
    this.activeTab.set(tab);
  }

  getUnclaimedSegments(d: TravelRequestDetail) {
    const activeExp = (d.expenses || []).filter((e) => e.status !== 'Rejected');
    return (d.segments || []).filter(
      (s) =>
        s.cost > 0 &&
        !activeExp.some(
          (e) =>
            (e.category.toLowerCase() === 'airfare' || e.category.toLowerCase() === 'transport') &&
            e.amount === s.cost
        )
    );
  }

  getUnclaimedStays(d: TravelRequestDetail) {
    const activeExp = (d.expenses || []).filter((e) => e.status !== 'Rejected');
    return (d.stays || []).filter(
      (st) =>
        st.cost > 0 &&
        !activeExp.some(
          (e) =>
            (e.category.toLowerCase() === 'hotel' || e.category.toLowerCase() === 'lodging') &&
            e.amount === st.cost
        )
    );
  }

  getActualSpend(d: TravelRequestDetail): number {
    const segTotal = this.getUnclaimedSegments(d).reduce((sum, s) => sum + (s.cost || 0), 0);
    const stayTotal = this.getUnclaimedStays(d).reduce((sum, st) => sum + (st.cost || 0), 0);
    const expTotal = (d.expenses || [])
      .filter((e) => e.status !== 'Rejected')
      .reduce((sum, e) => sum + (e.amount || 0), 0);
    return segTotal + stayTotal + expTotal;
  }

  claimItineraryExpense(d: TravelRequestDetail, category: 'Airfare' | 'Hotel', amount: number, label: string): void {
    this.voyageService
      .submitExpense({
        requestId: d.id,
        category,
        amount,
        description: `Out-of-pocket ${label}`
      })
      .subscribe({
        next: (created: any) => {
          if (created && created.id) {
            this.detail.update((curr) =>
              curr ? { ...curr, expenses: [created, ...(curr.expenses || [])] } : curr
            );
          }
          this.showToast(`Claimed ${category} (₹${amount.toLocaleString('en-IN')}) for reimbursement.`);
          this.loadDetail(d.id);
        },
        error: (err: any) => {
          this.showToast(err?.error?.message || 'Failed to claim itinerary expense.');
        }
      });
  }

  getSpendPercent(d: TravelRequestDetail): number {
    if (!d.estimatedBudget || d.estimatedBudget <= 0) return 0;
    return Math.min(100, Math.round((this.getActualSpend(d) / d.estimatedBudget) * 100));
  }

  isOverBudget(d: TravelRequestDetail): boolean {
    return d.estimatedBudget > 0 && this.getActualSpend(d) > d.estimatedBudget;
  }

  cancelRequest(): void {
    const d = this.detail();
    if (!d) return;
    if (confirm(`Are you sure you want to cancel ${d.requestCode}?`)) {
      this.voyageService.cancelRequest(d.id).subscribe({
        next: () => {
          void this.router.navigate(['/requests']);
        },
        error: () => {
          void this.router.navigate(['/requests']);
        }
      });
    }
  }

  showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => {
      if (this.toastMessage() === msg) {
        this.toastMessage.set(null);
      }
    }, 4000);
  }

  canEditTrip(d: TravelRequestDetail): boolean {
    const userId = this.authService.currentUser()?.id;
    return this.authService.isAdmin() || (!!userId && userId.toLowerCase() === d.employeeId.toLowerCase());
  }

  canSubmitExpense(d: TravelRequestDetail): boolean {
    return this.canEditTrip(d) && d.status === 'Approved';
  }

  canModifyItinerary(d: TravelRequestDetail): boolean {
    return this.canEditTrip(d) && d.status !== 'Completed' && d.status !== 'Rejected' && d.status !== 'Cancelled';
  }

  isOwnRequest(d: TravelRequestDetail): boolean {
    const userId = this.authService.currentUser()?.id;
    return !!userId && userId.toLowerCase() === d.employeeId.toLowerCase();
  }

  get backRoute(): string {
    return this.authService.isFinance() ? '/approvals' : '/requests';
  }

  submitDraft(): void {
    const d = this.detail();
    if (!d || this.isSubmitting()) return;
    this.isSubmitting.set(true);
    this.voyageService.submitDraft(d.id).subscribe({
      next: (res: any) => {
        this.isSubmitting.set(false);
        this.showToast(res?.message || 'Request submitted for Manager review.');
        this.loadDetail(d.id);
      },
      error: (err: any) => {
        this.isSubmitting.set(false);
        this.showToast(err?.error?.message || err?.error || err?.message || 'Failed to submit draft.');
        this.loadDetail(d.id);
      }
    });
  }

  // Segment
  openSegmentModal(): void {
    this.showSegmentModal.set(true);
  }
  closeSegmentModal(): void {
    this.showSegmentModal.set(false);
  }
  addSegmentSubmit(): void {
    const d = this.detail();
    if (!d) return;
    this.voyageService.addSegment(d.id, this.segmentForm).subscribe({
      next: (created: any) => {
        if (created && created.id) {
          this.detail.update((curr) =>
            curr ? { ...curr, segments: [...(curr.segments || []), created] } : curr
          );
        }
        this.closeSegmentModal();
        this.loadDetail(d.id);
      },
      error: (err: any) => {
        this.closeSegmentModal();
        this.showToast(err?.error?.message || 'Failed to add segment.');
      }
    });
  }

  // Stay
  openStayModal(): void {
    this.showStayModal.set(true);
  }
  closeStayModal(): void {
    this.showStayModal.set(false);
  }
  addStaySubmit(): void {
    const d = this.detail();
    if (!d) return;
    this.voyageService.addStay(d.id, this.stayForm).subscribe({
      next: (created: any) => {
        if (created && created.id) {
          this.detail.update((curr) =>
            curr ? { ...curr, stays: [...(curr.stays || []), created] } : curr
          );
        }
        this.closeStayModal();
        this.loadDetail(d.id);
      },
      error: (err: any) => {
        this.closeStayModal();
        this.showToast(err?.error?.message || 'Failed to add stay.');
      }
    });
  }

  // Expense
  openExpenseModal(): void {
    this.selectedProofFile.set(null);
    this.selectedProofFileName.set(null);
    this.showExpenseModal.set(true);
  }

  closeExpenseModal(): void {
    this.showExpenseModal.set(false);
    this.selectedProofFile.set(null);
    this.selectedProofFileName.set(null);
  }

  onProofFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedProofFile.set(file);
      this.selectedProofFileName.set(file.name);
      this.expenseForm.proofPath = file.name;
    }
  }

  removeProofFile(): void {
    this.selectedProofFile.set(null);
    this.selectedProofFileName.set(null);
    this.expenseForm.proofPath = '';
  }

  submitExpenseSubmit(): void {
    const d = this.detail();
    if (!d) return;

    const file = this.selectedProofFile();
    if (file) {
      this.isUploadingProof.set(true);
      this.voyageService.uploadExpenseProof(file).subscribe({
        next: (uploadRes) => {
          this.isUploadingProof.set(false);
          this.executeExpensePost(d, uploadRes.proofPath);
        },
        error: () => {
          this.isUploadingProof.set(false);
          alert('Failed to upload proof file. Please try again.');
        }
      });
    } else {
      this.executeExpensePost(d, this.expenseForm.proofPath || '');
    }
  }

  private executeExpensePost(d: TravelRequestDetail, proofPath: string): void {
    this.voyageService.submitExpense({
      requestId: d.id,
      category: this.expenseForm.category,
      amount: this.expenseForm.amount,
      description: this.expenseForm.description,
      proofPath: proofPath
    }).subscribe({
      next: (created: any) => {
        if (created && created.id) {
          this.detail.update((curr) =>
            curr ? { ...curr, expenses: [created, ...(curr.expenses || [])] } : curr
          );
        }
        this.closeExpenseModal();
        this.activeTab.set('Expenses');
        this.loadDetail(d.id);
      },
      error: (err: any) => {
        this.closeExpenseModal();
        this.showToast(err?.error?.message || 'Failed to submit expense claim.');
      }
    });
  }

  // Proof Viewing & Verification
  openProofModal(expense: ExpenseItem): void {
    this.proofToView.set(expense);
    this.showProofModal.set(true);
  }

  closeProofModal(): void {
    this.showProofModal.set(false);
    this.proofToView.set(null);
  }

  openAttachProofModal(expense: ExpenseItem): void {
    this.targetExpenseForProof.set(expense);
    this.attachFile.set(null);
    this.showAttachModal.set(true);
  }

  closeAttachProofModal(): void {
    this.showAttachModal.set(false);
    this.targetExpenseForProof.set(null);
    this.attachFile.set(null);
  }

  onAttachFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.attachFile.set(input.files[0]);
    }
  }

  submitAttachProof(): void {
    const expense = this.targetExpenseForProof();
    const file = this.attachFile();
    const d = this.detail();
    if (!expense || !file || !d) return;

    this.isUploadingProof.set(true);
    this.voyageService.attachExpenseProof(expense.id, file).subscribe({
      next: () => {
        this.isUploadingProof.set(false);
        this.closeAttachProofModal();
        this.loadDetail(d.id);
      },
      error: () => {
        this.isUploadingProof.set(false);
        this.closeAttachProofModal();
      }
    });
  }

  getProofUrl(proofPath?: string): string {
    return this.voyageService.getProofUrl(proofPath);
  }

  isImageProof(proofPath?: string): boolean {
    return this.voyageService.isImageProof(proofPath);
  }

  isPdfProof(proofPath?: string): boolean {
    return this.voyageService.isPdfProof(proofPath);
  }

  // Close trip
  closeTrip(): void {
    const d = this.detail();
    if (!d) return;
    if (confirm(`Mark trip ${d.requestCode} as Completed and close request?`)) {
      this.voyageService.closeRequest(d.id).subscribe({
        next: () => {
          this.loadDetail(d.id);
        },
        error: () => {
          this.loadDetail(d.id);
        }
      });
    }
  }

  // Quick Approval on Detail Page
  quickApprove(): void {
    const d = this.detail();
    if (!d) return;
    const pendingStep = d.approvalHistory?.find(s => s.status === 'Pending');
    if (pendingStep) {
      this.voyageService.takeApprovalAction(pendingStep.id, 'Approve', 'Approved on trip detail.').subscribe({
        next: () => this.loadDetail(d.id),
        error: () => this.loadDetail(d.id)
      });
    }
  }

  quickReject(): void {
    const d = this.detail();
    if (!d) return;
    const pendingStep = d.approvalHistory?.find(s => s.status === 'Pending');
    if (pendingStep) {
      this.voyageService.takeApprovalAction(pendingStep.id, 'Reject', 'Budget exceeds limits.').subscribe({
        next: () => this.loadDetail(d.id),
        error: () => this.loadDetail(d.id)
      });
    }
  }
}
