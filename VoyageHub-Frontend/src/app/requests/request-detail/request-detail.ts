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
    carrier: 'IndiGo 6E-5324',
    origin: 'BOM',
    destination: 'BLR',
    departureTime: '2026-10-20T08:15',
    cost: 9800.00,
    confirmationNumber: '#6E-90234'
  };

  // New Stay Model
  stayForm = {
    hotelName: 'The Leela Palace Bengaluru',
    checkIn: '2026-10-20',
    checkOut: '2026-10-24',
    nights: 4,
    cost: 28500.00,
    confirmationNumber: '#LP-88902'
  };

  // New Expense Model
  expenseForm = {
    category: 'Meals',
    amount: 3850.00,
    description: 'Client welcome dinner at Karavalli with GST bill',
    proofPath: 'gst_tax_invoice_karavalli.pdf'
  };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDetail(id);
    } else {
      this.loadMockOrFallback();
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
        this.loadMockOrFallback();
      }
    });
  }

  private loadMockOrFallback(): void {
    // Indian corporate trip fallback (BOM -> BLR)
    this.detail.set({
      id: 'AA010101-0000-0000-0000-000000000001',
      requestCode: '#TR-2048',
      employeeId: 'A1111111-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
      employeeName: 'Rohan Sharma',
      employeeEmail: 'rohan.sharma@voyagehub.in',
      departmentName: 'Sales & Business Dev (Mumbai HQ)',
      destination: 'Bengaluru, Karnataka',
      purpose: 'Annual Strategic Planning & Client Leadership Summit',
      startDate: '2026-10-20',
      endDate: '2026-10-24',
      tripDurationDays: 5,
      tripDurationNights: 4,
      estimatedBudget: 46500.00,
      travelClass: 'Economy (Standard)',
      status: 'Manager Review',
      policyCompliance: 'Flagged',
      submittedDate: '2026-10-12T10:00:00',
      primaryContact: 'VoyageHub India Corporate Desk (+91-80-4000-8800)',
      segments: [
        {
          id: '1',
          mode: 'Flight',
          carrier: 'IndiGo 6E-5324',
          origin: 'BOM (Mumbai)',
          destination: 'BLR (Bengaluru)',
          departureTime: '2026-10-20T08:15:00',
          arrivalTime: '2026-10-20T10:05:00',
          confirmationNumber: '#6E-90234',
          cost: 9800.00
        }
      ],
      stays: [
        {
          id: '1',
          hotelName: 'The Leela Palace Bengaluru',
          checkIn: '2026-10-20',
          checkOut: '2026-10-24',
          nights: 4,
          confirmationNumber: '#LP-88902',
          cost: 28500.00
        }
      ],
      expenses: [
        {
          id: '1',
          requestId: 'AA010101-0000-0000-0000-000000000001',
          requestCode: '#TR-2048',
          employeeName: 'Rohan Sharma',
          departmentName: 'Sales & Business Dev (Mumbai HQ)',
          tripPurpose: 'Client Leadership Summit',
          category: 'Airfare',
          amount: 9800.00,
          proofPath: 'indigo_gst_tax_invoice.pdf',
          status: 'Pending',
          submittedAt: '2026-10-21T09:00:00'
        },
        {
          id: '2',
          requestId: 'AA010101-0000-0000-0000-000000000001',
          requestCode: '#TR-2048',
          employeeName: 'Rohan Sharma',
          departmentName: 'Sales & Business Dev (Mumbai HQ)',
          tripPurpose: 'Client Leadership Summit',
          category: 'Meals',
          amount: 3850.00,
          proofPath: 'karavalli_dinner_gst_bill.pdf',
          status: 'Pending',
          submittedAt: '2026-10-22T21:30:00'
        }
      ],
      approvalHistory: [
        {
          id: '1',
          level: 'Manager',
          approverName: 'Priya Nair',
          status: 'Pending'
        },
        {
          id: '2',
          level: 'Finance',
          approverName: 'Vikram Malhotra',
          status: 'Pending'
        }
      ]
    });
    this.isLoading.set(false);
  }

  setTab(tab: 'Itinerary' | 'Expenses' | 'Approval History'): void {
    this.activeTab.set(tab);
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

  submitDraft(): void {
    const d = this.detail();
    if (!d) return;
    this.voyageService.submitDraft(d.id).subscribe({
      next: () => {
        this.loadDetail(d.id);
      },
      error: () => {
        // Optimistic UI fallback
        d.status = 'Manager Review';
        this.detail.set({ ...d });
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
      next: () => {
        this.closeSegmentModal();
        this.loadDetail(d.id);
      },
      error: () => {
        // Optimistic update
        d.segments.push({
          id: Date.now().toString(),
          mode: this.segmentForm.mode,
          carrier: this.segmentForm.carrier,
          origin: this.segmentForm.origin,
          destination: this.segmentForm.destination,
          departureTime: this.segmentForm.departureTime,
          cost: this.segmentForm.cost,
          confirmationNumber: this.segmentForm.confirmationNumber
        });
        this.detail.set({ ...d });
        this.closeSegmentModal();
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
      next: () => {
        this.closeStayModal();
        this.loadDetail(d.id);
      },
      error: () => {
        // Optimistic update
        d.stays.push({
          id: Date.now().toString(),
          hotelName: this.stayForm.hotelName,
          checkIn: this.stayForm.checkIn,
          checkOut: this.stayForm.checkOut,
          nights: this.stayForm.nights,
          cost: this.stayForm.cost,
          confirmationNumber: this.stayForm.confirmationNumber
        });
        this.detail.set({ ...d });
        this.closeStayModal();
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
          // Fallback to local name
          this.executeExpensePost(d, file.name);
        }
      });
    } else {
      this.executeExpensePost(d, this.expenseForm.proofPath || 'receipt.pdf');
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
      next: () => {
        this.closeExpenseModal();
        this.loadDetail(d.id);
        this.activeTab.set('Expenses');
      },
      error: () => {
        d.expenses.push({
          id: Date.now().toString(),
          requestId: d.id,
          requestCode: d.requestCode,
          employeeName: d.employeeName,
          departmentName: d.departmentName,
          tripPurpose: d.purpose,
          category: this.expenseForm.category,
          amount: this.expenseForm.amount,
          proofPath: proofPath,
          status: 'Pending',
          submittedAt: new Date().toISOString()
        });
        this.detail.set({ ...d });
        this.closeExpenseModal();
        this.activeTab.set('Expenses');
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
      next: (res) => {
        this.isUploadingProof.set(false);
        this.closeAttachProofModal();
        this.loadDetail(d.id);
      },
      error: () => {
        this.isUploadingProof.set(false);
        expense.proofPath = file.name;
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
          d.status = 'Completed';
          this.detail.set({ ...d });
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
