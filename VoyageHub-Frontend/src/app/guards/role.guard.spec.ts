import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { Injector, runInInjectionContext, signal } from '@angular/core';
import { Router } from '@angular/router';
import { roleGuard } from './role.guard';
import { AuthService, UserProfile, PRESET_PERSONAS } from '../services/auth.service';

describe('roleGuard', () => {
  let mockAuthService: {
    currentUser: ReturnType<typeof signal<UserProfile | null>>;
    getRoleDefaultRoute: (role?: string) => string;
  };
  let mockRouter: {
    navigate: ReturnType<typeof vi.fn>;
  };
  let testInjector: Injector;

  beforeEach(() => {
    mockAuthService = {
      currentUser: signal<UserProfile | null>(PRESET_PERSONAS[0]), // Rohan Sharma (Employee)
      getRoleDefaultRoute: (role?: string) => {
        switch (role) {
          case 'Manager': return '/approvals';
          case 'Finance': return '/expenses';
          case 'Admin': return '/policies';
          case 'Employee':
          default:
            return '/dashboard';
        }
      }
    };

    mockRouter = {
      navigate: vi.fn()
    };

    testInjector = Injector.create({
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  const executeGuard = (allowedRoles: string[]) => {
    return runInInjectionContext(testInjector, () => {
      const guardFn = roleGuard(allowedRoles);
      return guardFn(null as any, null as any);
    });
  };

  describe('When user is unauthenticated (null)', () => {
    it('should redirect to /login and return false', () => {
      mockAuthService.currentUser.set(null);

      const result = executeGuard(['Employee', 'Manager']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/login']);
    });
  });

  describe('When user role is Employee', () => {
    beforeEach(() => {
      mockAuthService.currentUser.set(PRESET_PERSONAS[0]); // Employee
    });

    it('should allow access to routes with Employee role (e.g. /dashboard, /requests)', () => {
      const result = executeGuard(['Employee', 'Manager', 'Finance', 'Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should deny access to /approvals and redirect to /dashboard', () => {
      const result = executeGuard(['Manager', 'Finance', 'Admin']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should deny access to /expenses and redirect to /dashboard', () => {
      const result = executeGuard(['Finance', 'Admin']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should deny access to /policies and redirect to /dashboard', () => {
      const result = executeGuard(['Admin']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should deny access to /audit and redirect to /dashboard', () => {
      const result = executeGuard(['Admin']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });
  });

  describe('When user role is Manager', () => {
    beforeEach(() => {
      mockAuthService.currentUser.set(PRESET_PERSONAS[1]); // Priya Nair (Manager)
    });

    it('should allow access to /approvals', () => {
      const result = executeGuard(['Manager', 'Finance', 'Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should allow access to /analytics', () => {
      const result = executeGuard(['Manager', 'Finance', 'Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should deny access to /policies and redirect to /approvals', () => {
      const result = executeGuard(['Admin']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/approvals']);
    });

    it('should deny access to /expenses and redirect to /approvals', () => {
      const result = executeGuard(['Finance', 'Admin']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/approvals']);
    });
  });

  describe('When user role is Finance', () => {
    beforeEach(() => {
      mockAuthService.currentUser.set(PRESET_PERSONAS[2]); // Vikram Malhotra (Finance)
    });

    it('should allow access to /expenses', () => {
      const result = executeGuard(['Finance', 'Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should allow access to /approvals', () => {
      const result = executeGuard(['Manager', 'Finance', 'Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should deny access to /requests and redirect to /expenses', () => {
      const result = executeGuard(['Employee', 'Manager', 'Admin']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/expenses']);
    });

    it('should deny access to /policies and redirect to /expenses', () => {
      const result = executeGuard(['Admin']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/expenses']);
    });
  });

  describe('When user role is Admin', () => {
    beforeEach(() => {
      mockAuthService.currentUser.set(PRESET_PERSONAS[3]); // Ananya Verma (Admin)
    });

    it('should allow access to /policies', () => {
      const result = executeGuard(['Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should allow access to /audit', () => {
      const result = executeGuard(['Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should allow access to /dashboard', () => {
      const result = executeGuard(['Employee', 'Manager', 'Finance', 'Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should allow access to /approvals', () => {
      const result = executeGuard(['Manager', 'Finance', 'Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should allow access to /expenses', () => {
      const result = executeGuard(['Finance', 'Admin']);

      expect(result).toBe(true);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  describe('When user has an empty or missing role', () => {
    it('should default to Employee and redirect accordingly', () => {
      mockAuthService.currentUser.set({
        fullname: 'Guest User',
        email: 'guest@voyagehub.in',
        role: ''
      });

      const result = executeGuard(['Admin']);

      expect(result).toBe(false);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });
  });
});
