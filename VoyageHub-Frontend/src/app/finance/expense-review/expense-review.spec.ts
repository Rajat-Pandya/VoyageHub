import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ExpenseReview } from './expense-review';
import { VoyageService } from '../../services/voyage.service';

describe('ExpenseReview', () => {
  let component: ExpenseReview;
  let fixture: ComponentFixture<ExpenseReview>;

  const sampleClaim = {
    id: 'exp-1',
    requestId: 'req-1',
    requestCode: '#TR-2053',
    employeeName: 'Viral Patel',
    departmentName: 'Analytics',
    tripPurpose: 'Summit',
    category: 'Meals',
    amount: 5000,
    proofPath: '/api/expenses/proof/bill.pdf',
    status: 'Pending',
    submittedAt: '2026-11-25T10:00:00Z',
    isOverBudget: true,
    approvedBudget: 4000,
    actualSpend: 5000
  };

  const mockVoyageService = {
    getPendingExpenses: vi.fn().mockReturnValue(of({ totalPendingAmount: 5000, pendingClaims: [sampleClaim] })),
    takeExpenseAction: vi.fn().mockReturnValue(of({ success: true })),
    batchPayout: vi.fn().mockReturnValue(of({ success: true })),
    getProofUrl: vi.fn((p?: string) => p || ''),
    isImageProof: vi.fn((p?: string) => Boolean(p?.endsWith('.png'))),
    isPdfProof: vi.fn((p?: string) => Boolean(p?.endsWith('.pdf')))
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    mockVoyageService.getPendingExpenses.mockReturnValue(of({ totalPendingAmount: 5000, pendingClaims: [sampleClaim] }));

    await TestBed.configureTestingModule({
      imports: [ExpenseReview],
      providers: [provideRouter([]), { provide: VoyageService, useValue: mockVoyageService }]
    }).compileComponents();

    fixture = TestBed.createComponent(ExpenseReview);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('loads claims with selected status and handles actions', () => {
    expect(component.claims().length).toBe(1);
    expect(component.totalPending()).toBe(5000);

    component.takeAction(sampleClaim, 'Approve');
    expect(mockVoyageService.takeExpenseAction).toHaveBeenCalledWith('exp-1', 'Approve');

    component.batchPayout();
    expect(mockVoyageService.batchPayout).toHaveBeenCalled();
    expect(component.toastMessage()).toContain('Batch payout');
  });

  it('opens/closes receipt modal and checks proof helpers', () => {
    component.openReceiptModal(sampleClaim);
    expect(component.previewClaim()).toEqual(sampleClaim);
    expect(component.getProofUrl(sampleClaim.proofPath)).toBe(sampleClaim.proofPath);
    expect(component.isPdfProof(sampleClaim.proofPath)).toBe(true);
    expect(component.isImageProof(sampleClaim.proofPath)).toBe(false);

    component.closeReceiptModal();
    expect(component.previewClaim()).toBeNull();
  });

  it('handles loadExpenses error gracefully', () => {
    mockVoyageService.getPendingExpenses.mockReturnValue(throwError(() => new Error('err')));
    component.loadExpenses();
    expect(component.claims()).toEqual([]);
    expect(component.totalPending()).toBe(0);
    expect(component.isLoading()).toBe(false);
  });
});
