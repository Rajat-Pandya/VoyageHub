import '@angular/compiler';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { Login } from './login';
import { AuthService, DEMO_PASSWORD, PRESET_PERSONAS } from '../services/auth.service';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let router: Router;

  const mockAuthService = {
    loginUser: vi.fn().mockReturnValue(of({
      token: 'jwt-mock-token',
      user: PRESET_PERSONAS[0] // Rohan Sharma (Employee)
    })),
    requestPasswordReset: vi.fn().mockReturnValue(of({
      message: 'Reset token generated successfully',
      resetToken: 'test-token-123'
    }))
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService }
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should toggle password visibility signal', () => {
    expect(component.showPassword()).toBe(false);
    component.togglePasswordVisibility();
    expect(component.showPassword()).toBe(true);
    component.togglePasswordVisibility();
    expect(component.showPassword()).toBe(false);
  });

  it('should switch to forgot password mode and back', () => {
    expect(component.viewMode()).toBe('login');
    component.openForgotPassword();
    expect(component.viewMode()).toBe('forgot');
    component.backToLogin();
    expect(component.viewMode()).toBe('login');
  });

  it('should authenticate user and navigate on submitLogin', () => {
    const navSpy = vi.spyOn(router, 'navigate');
    component.email = 'rohan.sharma@voyagehub.in';
    component.password = DEMO_PASSWORD;
    component.submitLogin();

    expect(mockAuthService.loginUser).toHaveBeenCalledWith('rohan.sharma@voyagehub.in', DEMO_PASSWORD);
    expect(navSpy).toHaveBeenCalledWith(['/dashboard']);
  });

  it('should navigate manager to approvals on login', () => {
    mockAuthService.loginUser.mockReturnValueOnce(of({
      token: 'jwt-manager-token',
      user: PRESET_PERSONAS[1] // Priya Nair (Manager)
    }));
    const navSpy = vi.spyOn(router, 'navigate');
    component.submitLogin();

    expect(navSpy).toHaveBeenCalledWith(['/approvals']);
  });

  it('should handle login error gracefully', () => {
    mockAuthService.loginUser.mockReturnValueOnce(throwError(() => new Error('Invalid corporate credentials')));
    component.submitLogin();

    expect(component.errorMessage()).toBe('Invalid corporate credentials');
    expect(component.isSubmitting()).toBe(false);
  });

  it('should validate empty email in submitForgotPassword', () => {
    component.forgotEmail = '   ';
    component.submitForgotPassword();

    expect(component.errorMessage()).toBe('Please enter your corporate email address.');
  });
});
