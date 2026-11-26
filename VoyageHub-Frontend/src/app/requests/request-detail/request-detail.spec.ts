import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { RequestDetail } from './request-detail';
import { VoyageService } from '../../services/voyage.service';
import { AuthService } from '../../services/auth.service';

describe('RequestDetail', () => {
  let component: RequestDetail;
  let fixture: ComponentFixture<RequestDetail>;

  const sampleDetail = {
    id: 'req-1',
    requestCode: '#TR-2053',
    employeeId: 'emp-1',
    employeeName: 'Viral Patel',
    employeeEmail: 'viral@voyagehub.in',
    departmentName: 'Analytics',
    destination: 'Auckland',
    purpose: 'Tech Summit',
    startDate: '2026-11-26',
    endDate: '2026-11-30',
    tripDurationDays: 4,
    tripDurationNights: 3,
    estimatedBudget: 150000,
    travelClass: 'Economy+',
    status: 'Approved',
    policyCompliance: 'Standard',
    policyFlagReasons: [],
    submittedDate: '2026-11-20',
    primaryContact: 'Desk',
    segments: [{ id: 's1', mode: 'Flight', origin: 'SIN', destination: 'AKL', departureTime: '2026-11-26', cost: 68500 }],
    stays: [{ id: 'st1', hotelName: 'Cordis', checkIn: '2026-11-26', checkOut: '2026-11-30', nights: 4, cost: 76000 }],
    expenses: [
      { id: 'e1', requestId: 'req-1', requestCode: '#TR-2053', employeeName: 'Viral', departmentName: 'Analytics', tripPurpose: 'Summit', category: 'Meals', amount: 10000, status: 'Pending', submittedAt: '2026-11-25' },
      { id: 'e2', requestId: 'req-1', requestCode: '#TR-2053', employeeName: 'Viral', departmentName: 'Analytics', tripPurpose: 'Summit', category: 'Cab', amount: 5000, status: 'Rejected', submittedAt: '2026-11-25' }
    ],
    approvalHistory: [{ id: 'ap-1', level: 'Manager', approverName: 'Priya', status: 'Pending' }]
  };

  const mockVoyageService = {
    getRequestDetail: vi.fn().mockReturnValue(of(sampleDetail)),
    addSegment: vi.fn().mockReturnValue(of({})),
    addStay: vi.fn().mockReturnValue(of({})),
    submitExpense: vi.fn().mockReturnValue(of({})),
    submitDraft: vi.fn().mockReturnValue(of({ message: 'Submitted' })),
    cancelRequest: vi.fn().mockReturnValue(of({})),
    closeRequest: vi.fn().mockReturnValue(of({})),
    takeApprovalAction: vi.fn().mockReturnValue(of({})),
    uploadExpenseProof: vi.fn().mockReturnValue(of({ proofPath: '/api/expenses/proof/bill.pdf' })),
    attachExpenseProof: vi.fn().mockReturnValue(of({ proofPath: '/api/expenses/proof/bill.pdf' })),
    getProofUrl: vi.fn((p?: string) => p || ''),
    isImageProof: vi.fn().mockReturnValue(false),
    isPdfProof: vi.fn().mockReturnValue(true)
  };

  const mockAuthService = {
    currentUser: vi.fn().mockReturnValue({ id: 'emp-1', role: 'Employee' }),
    isAdmin: vi.fn().mockReturnValue(false),
    isManager: vi.fn().mockReturnValue(false),
    isFinance: vi.fn().mockReturnValue(false)
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    mockVoyageService.getRequestDetail.mockReturnValue(of(sampleDetail));

    await TestBed.configureTestingModule({
      imports: [RequestDetail],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'req-1' } } } },
        { provide: VoyageService, useValue: mockVoyageService },
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RequestDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('computes actual spend excluding rejected expenses and checks over-budget', () => {
    expect(component.getActualSpend(sampleDetail)).toBe(154500);
    expect(component.getSpendPercent(sampleDetail)).toBe(100);
    expect(component.isOverBudget(sampleDetail)).toBe(true);
    expect(component.canEditTrip(sampleDetail)).toBe(true);
    expect(component.canSubmitExpense(sampleDetail)).toBe(true);
    expect(component.canSubmitExpense({ ...sampleDetail, status: 'Draft' })).toBe(false);
    expect(component.canModifyItinerary(sampleDetail)).toBe(true);
    expect(component.canModifyItinerary({ ...sampleDetail, status: 'Completed' })).toBe(false);
    expect(component.backRoute).toBe('/requests');
    expect(component.getUnclaimedSegments(sampleDetail).length).toBe(1);
    expect(component.getUnclaimedStays(sampleDetail).length).toBe(1);
    component.claimItineraryExpense(sampleDetail, 'Airfare', 68500, 'SIN -> AKL');
    expect(mockVoyageService.submitExpense).toHaveBeenCalled();
  });

  it('handles modals and mutations', () => {
    component.setTab('Expenses');
    expect(component.activeTab()).toBe('Expenses');

    component.openSegmentModal();
    component.addSegmentSubmit();
    expect(mockVoyageService.addSegment).toHaveBeenCalled();

    component.openStayModal();
    component.addStaySubmit();
    expect(mockVoyageService.addStay).toHaveBeenCalled();

    component.openExpenseModal();
    component.submitExpenseSubmit();
    expect(mockVoyageService.submitExpense).toHaveBeenCalled();

    component.quickApprove();
    expect(mockVoyageService.takeApprovalAction).toHaveBeenCalledWith('ap-1', 'Approve', 'Approved on trip detail.');

    component.quickReject();
    expect(mockVoyageService.takeApprovalAction).toHaveBeenCalledWith('ap-1', 'Reject', 'Budget exceeds limits.');

    component.submitDraft();
    expect(mockVoyageService.submitDraft).toHaveBeenCalledWith('req-1');

    vi.spyOn(window, 'confirm').mockReturnValue(true);
    component.closeTrip();
    expect(mockVoyageService.closeRequest).toHaveBeenCalledWith('req-1');

    component.openProofModal(sampleDetail.expenses[0]);
    expect(component.showProofModal()).toBe(true);
    expect(component.getProofUrl('x.pdf')).toBe('x.pdf');
    expect(component.isPdfProof('x.pdf')).toBe(true);
    expect(component.isImageProof('x.pdf')).toBe(false);
    component.closeProofModal();
    expect(component.showProofModal()).toBe(false);

    const file = new File(['abc'], 'receipt.pdf', { type: 'application/pdf' });
    component.onProofFileChange({ target: { files: [file] } } as unknown as Event);
    expect(component.selectedProofFileName()).toBe('receipt.pdf');
    component.submitExpenseSubmit();
    expect(mockVoyageService.uploadExpenseProof).toHaveBeenCalledWith(file);
    component.removeProofFile();
    expect(component.selectedProofFile()).toBeNull();

    component.openAttachProofModal(sampleDetail.expenses[0]);
    component.onAttachFileChange({ target: { files: [file] } } as unknown as Event);
    component.submitAttachProof();
    expect(mockVoyageService.attachExpenseProof).toHaveBeenCalledWith('e1', file);
  });
});
