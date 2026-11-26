import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { MyReimbursements } from './my-reimbursements';
import { VoyageService } from '../../services/voyage.service';

describe('MyReimbursements', () => {
  let component: MyReimbursements;
  let fixture: ComponentFixture<MyReimbursements>;

  const mockReports = [
    {
      requestId: 'req-1',
      requestCode: '#TR-2054',
      destination: 'Mumbai',
      purpose: 'Server Recovery',
      startDate: '2026-11-18',
      endDate: '2026-11-20',
      tripStatus: 'Completed',
      approvedBudget: 32000,
      totalTripSpend: 32000,
      corporatePaidAmount: 27500,
      claimedTotal: 4500,
      pendingAmount: 0,
      reimbursedAmount: 4500,
      reportStatus: 'Reimbursed',
      lineItems: [
        { kind: 'Itinerary' as const, category: 'Flight', description: 'AMD → BOM', amount: 11500, status: 'Corporate-Paid', date: '2026-11-18' },
        { kind: 'Expense' as const, category: 'Meals', description: 'Meals Claim', amount: 4500, status: 'Reimbursed', date: '2026-11-26' }
      ]
    },
    {
      requestId: 'req-2',
      requestCode: '#TR-2055',
      destination: 'Delhi',
      purpose: 'Client Sync',
      startDate: '2026-11-25',
      endDate: '2026-11-26',
      tripStatus: 'Approved',
      approvedBudget: 20000,
      totalTripSpend: 12000,
      corporatePaidAmount: 10000,
      claimedTotal: 2000,
      pendingAmount: 2000,
      reimbursedAmount: 0,
      reportStatus: 'Pending',
      lineItems: []
    }
  ];

  const mockVoyageService = {
    getMyReimbursementReports: vi.fn().mockReturnValue(of(mockReports))
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    mockVoyageService.getMyReimbursementReports.mockReturnValue(of(mockReports));

    await TestBed.configureTestingModule({
      imports: [MyReimbursements],
      providers: [
        provideRouter([]),
        { provide: VoyageService, useValue: mockVoyageService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MyReimbursements);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('loads trip reports and computes pending, reimbursed, and corporate-paid totals', () => {
    expect(component).toBeTruthy();
    expect(component.isLoading()).toBe(false);
    expect(component.totalPending()).toBe(2000);
    expect(component.totalReimbursed()).toBe(4500);
    expect(component.totalCorporatePaid()).toBe(37500);
    expect(component.filteredReports().length).toBe(2);

    component.statusFilter.set('Pending');
    expect(component.filteredReports().length).toBe(1);
    expect(component.filteredReports()[0].requestCode).toBe('#TR-2055');
  });

  it('handles API errors gracefully', () => {
    mockVoyageService.getMyReimbursementReports.mockReturnValue(throwError(() => new Error('fail')));
    component.ngOnInit();
    expect(component.reports()).toEqual([]);
    expect(component.isLoading()).toBe(false);
  });
});
