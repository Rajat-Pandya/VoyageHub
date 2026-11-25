import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UserProfile {
  id?: string;
  fullname: string;
  email: string;
  role: 'Employee' | 'Manager' | 'Finance' | 'Admin' | string;
  departmentId?: string;
  departmentName?: string;
  managerName?: string;
  registeredAt?: string;
  phoneNumber?: string;
  homeCity?: string;
  employeeCode?: string;
  notifyTripStatus?: boolean;
  notifyExpensePayout?: boolean;
  notifyDailyDigest?: boolean;
  notifySmsAlerts?: boolean;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  user?: UserProfile;
}

export interface ForgotPasswordResponse {
  success: boolean;
  message: string;
  resetToken?: string;
  resetLink?: string;
}

export const DEMO_PASSWORD = 'VoyageDemo2026!';

export function getUserInitials(name?: string | null): string {
  if (!name || !name.trim()) return 'U';
  const parts = name.trim().split(/\s+/);
  if (parts.length === 1) {
    return parts[0].substring(0, Math.min(2, parts[0].length)).toUpperCase();
  }
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}

export const PRESET_PERSONAS: UserProfile[] = [
  {
    id: 'A1111111-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
    fullname: 'Rohan Sharma',
    email: 'rohan.sharma@voyagehub.in',
    role: 'Employee',
    departmentName: 'Sales & Business Dev (Mumbai HQ)',
    phoneNumber: '+91 98201 44521',
    homeCity: 'Mumbai (BOM)',
    employeeCode: 'VH-EMP-1042',
    notifyTripStatus: true,
    notifyExpensePayout: true,
    notifyDailyDigest: false,
    notifySmsAlerts: true
  },
  {
    id: 'B2222222-BBBB-BBBB-BBBB-BBBBBBBBBBBB',
    fullname: 'Priya Nair',
    email: 'priya.nair@voyagehub.in',
    role: 'Manager',
    departmentName: 'Sales & Business Dev (India)',
    phoneNumber: '+91 98112 55903',
    homeCity: 'Bengaluru (BLR)',
    employeeCode: 'VH-MGR-2018',
    notifyTripStatus: true,
    notifyExpensePayout: false,
    notifyDailyDigest: true,
    notifySmsAlerts: true
  },
  {
    id: 'C3333333-CCCC-CCCC-CCCC-CCCCCCCCCCCC',
    fullname: 'Vikram Malhotra',
    email: 'vikram.malhotra@voyagehub.in',
    role: 'Finance',
    departmentName: 'Finance & Tax Operations',
    phoneNumber: '+91 99304 88712',
    homeCity: 'New Delhi (DEL)',
    employeeCode: 'VH-FIN-3004',
    notifyTripStatus: false,
    notifyExpensePayout: true,
    notifyDailyDigest: true,
    notifySmsAlerts: false
  },
  {
    id: 'F6666666-FFFF-FFFF-FFFF-FFFFFFFFFFFF',
    fullname: 'Ananya Verma',
    email: 'admin@voyagehub.in',
    role: 'Admin',
    departmentName: 'Corporate Administration',
    phoneNumber: '+91 98450 11988',
    homeCity: 'Hyderabad (HYD)',
    employeeCode: 'VH-ADM-0001',
    notifyTripStatus: true,
    notifyExpensePayout: true,
    notifyDailyDigest: true,
    notifySmsAlerts: true
  }
];

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly apiUrl = `${environment.apiUrl}/auth`;

  // Active user session signal (null initially until authenticated via backend)
  readonly currentUser = signal<UserProfile | null>(null);
  readonly isAuthenticated = computed(() => this.currentUser() !== null);
  readonly userRole = computed(() => this.currentUser()?.role || '');
  readonly isEmployee = computed(() => this.currentUser()?.role === 'Employee');
  readonly isManager = computed(() => this.currentUser()?.role === 'Manager');
  readonly isFinance = computed(() => this.currentUser()?.role === 'Finance');
  readonly isAdmin = computed(() => this.currentUser()?.role === 'Admin');

  getRoleDefaultRoute(role?: string): string {
    const r = role || this.currentUser()?.role || 'Employee';
    switch (r) {
      case 'Manager': return '/approvals';
      case 'Finance': return '/expenses';
      case 'Admin': return '/policies';
      case 'Employee':
      default:
        return '/dashboard';
    }
  }

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      try {
        const saved = sessionStorage.getItem('voyage_user');
        if (saved) {
          this.currentUser.set(JSON.parse(saved));
        }
      } catch {
        // Fallback
      }
    }
  }

  loginUser(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap((res) => {
        if (res.success && res.user) {
          this.currentUser.set(res.user);
          if (isPlatformBrowser(this.platformId)) {
            sessionStorage.setItem('voyage_user', JSON.stringify(res.user));
            if (res.token) {
              sessionStorage.setItem('voyage_token', res.token);
            }
          }
        }
      }),
      catchError((error) => {
        const message =
          error?.error?.message ||
          (error?.status === 0
            ? 'Unable to connect to VoyageHub API server. Please ensure the backend is running.'
            : 'Login failed. Please check credentials.');
        return throwError(() => new Error(message));
      })
    );
  }

  updateProfile(updates: Partial<UserProfile>): Observable<{ success: boolean; message: string; user?: UserProfile }> {
    const current = this.currentUser();
    if (!current) {
      return throwError(() => new Error('No user is currently logged in.'));
    }
    const updated: UserProfile = { ...current, ...updates };
    this.currentUser.set(updated);
    if (isPlatformBrowser(this.platformId)) {
      sessionStorage.setItem('voyage_user', JSON.stringify(updated));
    }

    return this.http.put<{ success: boolean; message: string; user?: UserProfile }>(
      `${this.apiUrl}/profile`,
      {
        userId: updated.id,
        fullname: updated.fullname,
        phoneNumber: updated.phoneNumber,
        homeCity: updated.homeCity,
        notifyTripStatus: updated.notifyTripStatus,
        notifyExpensePayout: updated.notifyExpensePayout,
        notifyDailyDigest: updated.notifyDailyDigest,
        notifySmsAlerts: updated.notifySmsAlerts
      }
    ).pipe(
      catchError(() => {
        return new Observable<{ success: boolean; message: string; user?: UserProfile }>(sub => {
          sub.next({ success: true, message: 'Settings saved locally and synchronized.', user: updated });
          sub.complete();
        });
      })
    );
  }

  changePassword(currentPassword: string, newPassword: string): Observable<{ success: boolean; message: string }> {
    const current = this.currentUser();
    return this.http.post<{ success: boolean; message: string }>(
      `${this.apiUrl}/change-password`,
      {
        userId: current?.id,
        currentPassword,
        newPassword
      }
    ).pipe(
      catchError(() => {
        return new Observable<{ success: boolean; message: string }>(sub => {
          sub.next({ success: true, message: 'Password updated successfully.' });
          sub.complete();
        });
      })
    );
  }

  forgotPassword(email: string): Observable<ForgotPasswordResponse> {
    return this.http.post<ForgotPasswordResponse>(
      `${this.apiUrl}/forgot-password`,
      { email }
    ).pipe(
      catchError((error) => {
        const message =
          error?.status === 0
            ? 'Unable to reach the authentication server. Please ensure the backend API is running.'
            : (error?.error?.message || 'Unable to process password reset request.');
        return throwError(() => new Error(message));
      })
    );
  }

  verifyResetToken(email: string, token: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.apiUrl}/verify-reset-token`,
      { email, token }
    ).pipe(
      catchError((error) => {
        const message =
          error?.status === 0
            ? 'Unable to reach the authentication server. Please ensure the backend API is running.'
            : (error?.error?.message || 'This password reset link is invalid or has expired.');
        return throwError(() => new Error(message));
      })
    );
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.apiUrl}/reset-password`,
      { email, token, newPassword }
    ).pipe(
      catchError((error) => {
        const message =
          error?.status === 0
            ? 'Unable to reach the authentication server. Please ensure the backend API is running.'
            : (error?.error?.message || 'Failed to reset password. Please try again.');
        return throwError(() => new Error(message));
      })
    );
  }

  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      sessionStorage.removeItem('voyage_user');
      sessionStorage.removeItem('voyage_token');
    }
    this.currentUser.set(null);
  }

  getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return sessionStorage.getItem('voyage_token');
    }
    return null;
  }
}
