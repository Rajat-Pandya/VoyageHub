import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NewRequest } from './new-request';
import { VoyageService } from '../../services/voyage.service';
import { AuthService } from '../../services/auth.service';

describe('NewRequest', () => {
  let component: NewRequest;
  let fixture: ComponentFixture<NewRequest>;
  let router: Router;

  const mockPolicy = {
    id: 'pol-1',
    departmentId: 'dept-1',
    departmentName: 'Engineering',
    domesticLimit: 40000,
    intlBudgetLimit: 250000,
    allowedClass: 'ECONOMY+',
    hardEnforcement: true
  };

  const mockVoyageService = {
    getMyPolicy: vi.fn().mockReturnValue(of(mockPolicy)),
    createRequest: vi.fn().mockReturnValue(of({ id: 'req-99' }))
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    mockVoyageService.getMyPolicy.mockReturnValue(of(mockPolicy));
    mockVoyageService.createRequest.mockReturnValue(of({ id: 'req-99' }));

    await TestBed.configureTestingModule({
      imports: [NewRequest],
      providers: [
        provideRouter([]),
        { provide: VoyageService, useValue: mockVoyageService },
        { provide: AuthService, useValue: { currentUser: vi.fn().mockReturnValue({ id: 'u1' }) } }
      ]
    }).compileComponents();

    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    fixture = TestBed.createComponent(NewRequest);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('loads department policy and evaluates domestic vs international limits', () => {
    expect(component.myPolicy()?.departmentName).toBe('Engineering');
    expect(component.activeBudgetLimit).toBe(40000);

    component.estimatedBudget = 50000;
    component.isInternational = false;
    expect(component.isBudgetExceeded).toBe(true);
    expect(component.isPolicyExceeded).toBe(true);
    expect(component.policyAlertMessage).toContain('domestic limit');

    component.isInternational = true;
    expect(component.activeBudgetLimit).toBe(250000);
    expect(component.isBudgetExceeded).toBe(false);

    component.travelClass = 'Business';
    expect(component.isClassExceeded).toBe(true);
    expect(component.policyAlertMessage).toContain('allowed class');
  });

  it('calculates itemizedTotal and syncs estimated budget', () => {
    component.estimatedBudget = 10000;
    component.flightCost = 15000;
    component.hotelCost = 12000;
    expect(component.itemizedTotal).toBe(27000);
    expect(component.showBudgetSyncHint).toBe(true);

    component.syncEstimatedBudget();
    expect(component.estimatedBudget).toBe(27000);
    expect(component.showBudgetSyncHint).toBe(false);
  });

  it('navigates wizard steps and validates/submits request', () => {
    component.nextStep();
    expect(component.currentStep()).toBe(2);
    component.nextStep();
    expect(component.currentStep()).toBe(3);
    component.prevStep();
    expect(component.currentStep()).toBe(2);
    component.prevStep();
    expect(component.currentStep()).toBe(1);

    component.submitRequest();
    expect(component.errorMessage()).toContain('Please fill in');

    component.tripPurpose = 'Summit';
    component.destinationCity = 'Berlin';
    component.isInternational = true;
    component.startDate = '2026-12-01';
    component.endDate = '2026-12-05';
    component.estimatedBudget = 120000;
    component.flightCarrier = 'LH123';
    component.hotelName = 'Hyatt';

    component.saveDraft();
    expect(mockVoyageService.createRequest).toHaveBeenCalledWith(
      expect.objectContaining({ isInternational: true, saveAsDraft: true })
    );
  });

  it('handles createRequest error', () => {
    mockVoyageService.createRequest.mockReturnValue(throwError(() => ({ error: { message: 'Invalid dates' } })));
    component.tripPurpose = 'Summit';
    component.destinationCity = 'Pune';
    component.startDate = '2026-12-01';
    component.endDate = '2026-12-02';
    component.submitRequest();
    expect(component.errorMessage()).toBe('Invalid dates');
    expect(component.isSubmitting()).toBe(false);
  });
});
