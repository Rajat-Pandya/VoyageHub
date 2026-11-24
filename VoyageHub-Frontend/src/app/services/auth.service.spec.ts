import '@angular/compiler';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { AuthService, PRESET_PERSONAS, DEMO_PASSWORD } from './auth.service';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('should be created and have unauthenticated default state', () => {
    expect(service).toBeTruthy();
    expect(service.currentUser()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(service.userRole()).toBe('');
    expect(service.isAdmin()).toBe(false);
    expect(service.isManager()).toBe(false);
    expect(service.isFinance()).toBe(false);
    expect(service.isEmployee()).toBe(false);
  });

  describe('getRoleDefaultRoute', () => {
    it('should return correct route for each persona role', () => {
      expect(service.getRoleDefaultRoute('Manager')).toBe('/approvals');
      expect(service.getRoleDefaultRoute('Finance')).toBe('/expenses');
      expect(service.getRoleDefaultRoute('Admin')).toBe('/policies');
      expect(service.getRoleDefaultRoute('Employee')).toBe('/dashboard');
      expect(service.getRoleDefaultRoute('')).toBe('/dashboard');
    });
  });

  describe('loginUser', () => {
    it('should authenticate user, update signals and store session', () => {
      const mockUser = PRESET_PERSONAS[0];
      const mockResponse = {
        success: true,
        message: 'Login successful',
        token: 'mock-jwt-token-xyz',
        user: mockUser
      };

      service.loginUser('rohan.sharma@voyagehub.in', DEMO_PASSWORD).subscribe((res) => {
        expect(res.success).toBe(true);
        expect(res.token).toBe('mock-jwt-token-xyz');
        expect(service.currentUser()?.email).toBe('rohan.sharma@voyagehub.in');
        expect(service.isAuthenticated()).toBe(true);
        expect(service.isEmployee()).toBe(true);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        email: 'rohan.sharma@voyagehub.in',
        password: DEMO_PASSWORD
      });

      req.flush(mockResponse);
    });

    it('should handle error when login fails', () => {
      let errorMessage = '';

      service.loginUser('unknown@voyagehub.in', 'WrongPwd!').subscribe({
        next: () => {},
        error: (err: Error) => {
          errorMessage = err.message;
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
      req.flush({ message: 'Invalid email or password.' }, { status: 401, statusText: 'Unauthorized' });

      expect(errorMessage).toBe('Invalid email or password.');
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('logout and session clearing', () => {
    it('should clear currentUser and session on logout', () => {
      const adminUser = PRESET_PERSONAS[3]; // Admin
      service.currentUser.set(adminUser);
      expect(service.isAuthenticated()).toBe(true);
      expect(service.isAdmin()).toBe(true);

      service.logout();

      expect(service.currentUser()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
      expect(service.isAdmin()).toBe(false);
    });
  });

  describe('updateProfile', () => {
    it('should update current user profile signals and make PUT request', () => {
      const initialUser = PRESET_PERSONAS[0];
      service.currentUser.set(initialUser);

      service.updateProfile({ seatPreference: 'Window', mealPreference: 'Vegetarian' }).subscribe((res) => {
        expect(res.success).toBe(true);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/auth/profile`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body.seatPreference).toBe('Window');

      req.flush({ success: true, message: 'Profile updated' });

      expect(service.currentUser()?.seatPreference).toBe('Window');
    });
  });
});
