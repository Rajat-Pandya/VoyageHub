import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { Dashboard } from './dashboard';
import { AuthService, PRESET_PERSONAS } from '../services/auth.service';
import { VoyageService } from '../services/voyage.service';

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;

  const mockAuthService = {
    currentUser: vi.fn().mockReturnValue(PRESET_PERSONAS[0])
  };

  const mockVoyageService = {
    getDashboardStats: vi.fn().mockReturnValue(of({
      draftsCount: 1,
      pendingCount: 2,
      upcomingTripsCount: 1,
      totalReimbursement: 34500.00
    })),
    getRequests: vi.fn().mockReturnValue(of([
      {
        id: 'req-1',
        requestCode: '#TR-2048',
        destination: 'Bengaluru, Karnataka',
        purpose: 'Client Review & Tech Summit',
        datesFormatted: '18 Jun – 22 Jun',
        startDate: '2026-06-18T00:00:00',
        endDate: '2026-06-22T00:00:00',
        submittedDate: '2026-06-01T10:00:00',
        estimatedBudget: 42500.00,
        approverName: 'Priya Nair',
        status: 'In Review',
        policyCompliance: 'Flagged',
        travelClass: 'Economy (Standard)',
        employeeName: 'Rohan Sharma',
        departmentName: 'Sales & Business Dev (Mumbai HQ)'
      }
    ]))
  };

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService },
        { provide: VoyageService, useValue: mockVoyageService }
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load dashboard data on init', () => {
    component.ngOnInit();
    expect(mockVoyageService.getDashboardStats).toHaveBeenCalled();
    expect(mockVoyageService.getRequests).toHaveBeenCalled();
    expect(component.requests().length).toBeGreaterThan(0);
  });

  it('should update filter and refetch requests on setFilter', () => {
    component.setFilter('Approved');
    expect(component.selectedFilter()).toBe('Approved');
    expect(mockVoyageService.getRequests).toHaveBeenLastCalledWith('Approved', '', 8);
  });

  it('should filter requests by search query', () => {
    component.searchQuery = 'Bengaluru';
    component.onSearch();
    expect(mockVoyageService.getRequests).toHaveBeenLastCalledWith('All', 'Bengaluru', 8);
  });
});
