import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { EmployeeManagement } from './employee-management';
import { VoyageService, EmployeeItem } from '../../services/voyage.service';

describe('EmployeeManagement', () => {
  let component: EmployeeManagement;
  let fixture: ComponentFixture<EmployeeManagement>;

  const mockEmployees: EmployeeItem[] = [
    {
      id: 'emp-1',
      name: 'Rohan Sharma',
      email: 'rohan.sharma@voyagehub.in',
      role: 'Employee',
      departmentName: 'Sales & Business Dev',
      createdAt: '2026-01-01T00:00:00'
    },
    {
      id: 'emp-2',
      name: 'Priya Nair',
      email: 'priya.nair@voyagehub.in',
      role: 'Manager',
      departmentName: 'Engineering',
      createdAt: '2026-01-02T00:00:00'
    },
    {
      id: 'emp-3',
      name: 'Ananya Verma',
      email: 'ananya.verma@voyagehub.in',
      role: 'Admin',
      departmentName: 'Administration',
      createdAt: '2026-01-03T00:00:00'
    }
  ];

  const mockVoyageService = {
    getEmployees: vi.fn().mockReturnValue(of(mockEmployees)),
    createEmployee: vi.fn().mockReturnValue(of({ success: true, message: 'Created' })),
    updateEmployee: vi.fn().mockReturnValue(of({ success: true, message: 'Updated' })),
    deleteEmployee: vi.fn().mockReturnValue(of({ success: true, message: 'Deleted' }))
  };

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [EmployeeManagement],
      providers: [
        { provide: VoyageService, useValue: mockVoyageService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EmployeeManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should load employees on init', () => {
    expect(component.employees().length).toBe(3);
    expect(mockVoyageService.getEmployees).toHaveBeenCalledTimes(1);
  });

  describe('departments computed signal', () => {
    it('should extract unique departments and sort alphabetically using localeCompare', () => {
      const depts = component.departments();
      expect(depts).toEqual(['Administration', 'Engineering', 'Sales & Business Dev']);
    });
  });

  describe('managerOptions computed signal', () => {
    it('should return only Manager and Admin users', () => {
      const managers = component.managerOptions();
      expect(managers.length).toBe(2);
      expect(managers.map(m => m.role)).toEqual(['Manager', 'Admin']);
    });

    it('should exclude the current editing user from manager options', () => {
      component.editingId.set('emp-2'); // Priya Nair
      const managers = component.managerOptions();
      expect(managers.length).toBe(1);
      expect(managers[0].id).toBe('emp-3'); // Ananya Verma
    });
  });

  describe('filteredEmployees computed signal', () => {
    it('should filter by search term matching name or email', () => {
      component.searchTerm.set('priya');
      expect(component.filteredEmployees().length).toBe(1);
      expect(component.filteredEmployees()[0].name).toBe('Priya Nair');
    });

    it('should filter by department', () => {
      component.selectedDepartment.set('Engineering');
      expect(component.filteredEmployees().length).toBe(1);
      expect(component.filteredEmployees()[0].departmentName).toBe('Engineering');
    });

    it('should filter by role', () => {
      component.selectedRole.set('Admin');
      expect(component.filteredEmployees().length).toBe(1);
      expect(component.filteredEmployees()[0].role).toBe('Admin');
    });

    it('should return all when filters are set to default All', () => {
      component.searchTerm.set('');
      component.selectedDepartment.set('All');
      component.selectedRole.set('All');
      expect(component.filteredEmployees().length).toBe(3);
    });
  });

  describe('modal interactions', () => {
    it('should open create modal with empty form', () => {
      component.openAddModal();
      expect(component.showAddEditModal()).toBe(true);
      expect(component.isEditMode()).toBe(false);
      expect(component.employeeForm.name).toBe('');
    });

    it('should open edit modal populated with selected employee data', () => {
      component.openEditModal(mockEmployees[0]);
      expect(component.showAddEditModal()).toBe(true);
      expect(component.isEditMode()).toBe(true);
      expect(component.editingId()).toBe('emp-1');
      expect(component.employeeForm.name).toBe('Rohan Sharma');
      expect(component.employeeForm.email).toBe('rohan.sharma@voyagehub.in');
    });
  });
});
