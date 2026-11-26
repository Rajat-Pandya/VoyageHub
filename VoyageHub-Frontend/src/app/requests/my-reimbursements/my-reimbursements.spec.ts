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

  const mockExpenses = [
    {
      id: 'exp-1',
      requestId: 'req-1',
      requestCode: '#TR-2048',
      employeeName: 'Rohan',
      departmentName: 'Sales',
      tripPurpose: 'Summit',
      category: 'Meals',
      amount: 5000,
      status: 'Pending',
      submittedAt: '2026-11-25T10:00:00Z'
    },
    {
      id: 'exp-2',
      requestId: 'req-2',
      requestCode: '#TR-2049',
      employeeName: 'Rohan',
      departmentName: 'Sales',
      tripPurpose: 'Client Visit',
      category: 'Cab',
      amount: 1200,
      status: 'Reimbursed',
      submittedAt: '2026-11-24T10:00:00Z'
    }
  ];

  const mockVoyageService = {
    getMyExpenses: vi.fn().mockReturnValue(of(mockExpenses))
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    mockVoyageService.getMyExpenses.mockReturnValue(of(mockExpenses));

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

  it('loads expenses and computes totals and filtered lists', () => {
    expect(component).toBeTruthy();
    expect(component.isLoading()).toBe(false);
    expect(component.totalPending()).toBe(5000);
    expect(component.totalReimbursed()).toBe(1200);
    expect(component.filteredExpenses().length).toBe(2);

    component.statusFilter.set('Pending');
    expect(component.filteredExpenses().length).toBe(1);
    expect(component.filteredExpenses()[0].id).toBe('exp-1');
  });

  it('handles API errors gracefully', () => {
    mockVoyageService.getMyExpenses.mockReturnValue(throwError(() => new Error('fail')));
    component.ngOnInit();
    expect(component.expenses()).toEqual([]);
    expect(component.isLoading()).toBe(false);
  });
});
