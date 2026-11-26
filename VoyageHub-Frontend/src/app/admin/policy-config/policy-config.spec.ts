import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { PolicyConfigComponent } from './policy-config';
import { VoyageService } from '../../services/voyage.service';

describe('PolicyConfigComponent', () => {
  let component: PolicyConfigComponent;
  let fixture: ComponentFixture<PolicyConfigComponent>;

  const samplePolicies = [
    {
      id: 'pol-1',
      departmentId: 'dept-1',
      departmentName: 'Engineering',
      intlBudgetLimit: 250000,
      domesticLimit: 40000,
      allowedClass: 'ECONOMY+',
      hardEnforcement: true
    }
  ];

  const mockVoyageService = {
    getPolicies: vi.fn().mockReturnValue(of(samplePolicies)),
    getAvailablePolicyDepartments: vi.fn().mockReturnValue(of(['Analytics', 'IT'])),
    createPolicy: vi.fn().mockReturnValue(of(samplePolicies[0])),
    updatePolicy: vi.fn().mockReturnValue(of({ success: true })),
    deletePolicy: vi.fn().mockReturnValue(of({ success: true }))
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    mockVoyageService.getPolicies.mockReturnValue(of(samplePolicies));
    mockVoyageService.getAvailablePolicyDepartments.mockReturnValue(of(['Analytics', 'IT']));

    await TestBed.configureTestingModule({
      imports: [PolicyConfigComponent],
      providers: [
        provideRouter([]),
        { provide: VoyageService, useValue: mockVoyageService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PolicyConfigComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('loads policies, opens add modal with available departments, and creates policy', () => {
    expect(component.policies().length).toBe(1);
    expect(component.hardEnforcementEnabled()).toBe(true);

    component.openAddPolicy();
    expect(component.showAddModal()).toBe(true);
    expect(component.availableDepartments()).toEqual(['Analytics', 'IT']);
    expect(component.selectedDepartmentOption).toBe('Analytics');
    expect(component.newPolicy.departmentName).toBe('Analytics');

    component.onDepartmentOptionChange('__new__');
    expect(component.newPolicy.departmentName).toBe('');
    component.submitAddPolicy();
    expect(component.toastMessage()).toContain('Please select or enter');

    component.onDepartmentOptionChange('IT');
    component.submitAddPolicy();
    expect(mockVoyageService.createPolicy).toHaveBeenCalled();
  });

  it('toggles enforcement, edits, and deletes policy', () => {
    component.toggleEnforcement();
    expect(mockVoyageService.updatePolicy).toHaveBeenCalled();

    component.editPolicy(samplePolicies[0]);
    expect(component.editingPolicy()?.id).toBe('pol-1');
    component.savePolicyEdit();
    expect(mockVoyageService.updatePolicy).toHaveBeenCalledTimes(2);

    vi.spyOn(window, 'confirm').mockReturnValue(true);
    component.deletePolicy(samplePolicies[0]);
    expect(mockVoyageService.deletePolicy).toHaveBeenCalledWith('pol-1');

    component.saveAllChanges();
    expect(component.toastMessage()).toContain('synchronized');
  });

  it('handles errors gracefully', () => {
    mockVoyageService.getAvailablePolicyDepartments.mockReturnValue(throwError(() => new Error('err')));
    component.openAddPolicy();
    expect(component.availableDepartments()).toEqual([]);
    component.closeAddPolicy();
    expect(component.showAddModal()).toBe(false);
  });
});
