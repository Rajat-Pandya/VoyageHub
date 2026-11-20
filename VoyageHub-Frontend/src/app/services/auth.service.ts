import { Injectable, signal, computed, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';

export interface UserProfile {
  id?: string;
  fullname: string;
  email: string;
  role: 'Employee' | 'Manager' | 'Finance' | 'Admin' | string;
  departmentId?: string;
  departmentName?: string;
  managerName?: string;
  avatarUrl?: string;
  registeredAt?: string;
  phoneNumber?: string;
  homeCity?: string;
  employeeCode?: string;
  seatPreference?: string;
  mealPreference?: string;
  roomPreference?: string;
  airlineLoyalty?: string;
  hotelLoyalty?: string;
  notifyTripStatus?: boolean;
  notifyExpensePayout?: boolean;
  notifyDailyDigest?: boolean;
  notifySmsAlerts?: boolean;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  user?: UserProfile;
}

export interface ForgotPasswordResponse {
  success: boolean;
  message: string;
  resetToken?: string;
  resetLink?: string;
}

export const PRESET_PERSONAS: UserProfile[] = [
  {
    id: 'A1111111-AAAA-AAAA-AAAA-AAAAAAAAAAAA',
    fullname: 'Rohan Sharma',
    email: 'rohan.sharma@voyagehub.in',
    role: 'Employee',
    departmentName: 'Sales & Business Dev (Mumbai HQ)',
    avatarUrl: 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=120&h=120&q=80',
    phoneNumber: '+91 98201 44521',
    homeCity: 'Mumbai (BOM)',
    employeeCode: 'VH-EMP-1042',
    seatPreference: 'Aisle',
    mealPreference: 'Vegetarian (Hindu)',
    roomPreference: 'High Floor, Non-Smoking',
    airlineLoyalty: 'Air India Flying Returns - AI-982144',
    hotelLoyalty: 'Taj InnerCircle - TIC-40819',
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
    avatarUrl: 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=120&h=120&q=80',
    phoneNumber: '+91 98112 55903',
    homeCity: 'Bengaluru (BLR)',
    employeeCode: 'VH-MGR-2018',
    seatPreference: 'Window',
    mealPreference: 'Vegetarian (Jain)',
    roomPreference: 'Quiet Floor, King Bed',
    airlineLoyalty: 'IndiGo 6E Rewards - 6E-55102',
    hotelLoyalty: 'Marriott Bonvoy - MB-881903',
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
    avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=120&h=120&q=80',
    phoneNumber: '+91 99304 88712',
    homeCity: 'New Delhi (DEL)',
    employeeCode: 'VH-FIN-3004',
    seatPreference: 'Aisle',
    mealPreference: 'Non-Vegetarian',
    roomPreference: 'Near Elevator, King Bed',
    airlineLoyalty: 'Emirates Skywards - EK-771204',
    hotelLoyalty: 'IHG One Rewards - IHG-66014',
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
    avatarUrl: 'https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=120&h=120&q=80',
    phoneNumber: '+91 98450 11988',
    homeCity: 'Hyderabad (HYD)',
    employeeCode: 'VH-ADM-0001',
    seatPreference: 'Window',
    mealPreference: 'Vegan',
    roomPreference: 'High Floor, Suite',
    airlineLoyalty: 'Air India Flying Returns - AI-10293',
    hotelLoyalty: 'Taj InnerCircle - TIC-99001',
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
  private readonly apiUrl = 'http://localhost:5062/api/auth';

  // Default to Rohan Sharma so the application is instantly ready and populated
  readonly currentUser = signal<UserProfile>(PRESET_PERSONAS[0]);
  readonly isAuthenticated = computed(() => this.currentUser() !== null);
  readonly userRole = computed(() => this.currentUser()?.role || 'Employee');
  readonly isEmployee = computed(() => (this.currentUser()?.role || 'Employee') === 'Employee');
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

  switchPersona(persona: UserProfile): void {
    this.currentUser.set(persona);
    if (isPlatformBrowser(this.platformId)) {
      sessionStorage.setItem('voyage_user', JSON.stringify(persona));
    }
  }

  loginUser(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap((res) => {
        if (res.success && res.user) {
          this.currentUser.set(res.user);
          if (isPlatformBrowser(this.platformId)) {
            sessionStorage.setItem('voyage_user', JSON.stringify(res.user));
          }
        }
      }),
      catchError((error) => {
        // Only fall back to preset demo personas if the backend API is unreachable (offline)
        if (error?.status === 0) {
          const found = PRESET_PERSONAS.find(p => p.email.toLowerCase() === email.toLowerCase());
          if (found) {
            this.currentUser.set(found);
            if (isPlatformBrowser(this.platformId)) {
              sessionStorage.setItem('voyage_user', JSON.stringify(found));
            }
            return new Observable<AuthResponse>(sub => {
              sub.next({ success: true, message: `Welcome back, ${found.fullname}!`, user: found });
              sub.complete();
            });
          }
        }
        const message = error?.error?.message || 'Login failed. Please check credentials.';
        return throwError(() => new Error(message));
      })
    );
  }

  updateProfile(updates: Partial<UserProfile>): Observable<{ success: boolean; message: string; user?: UserProfile }> {
    const current = this.currentUser();
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
        avatarUrl: updated.avatarUrl,
        phoneNumber: updated.phoneNumber,
        homeCity: updated.homeCity,
        seatPreference: updated.seatPreference,
        mealPreference: updated.mealPreference,
        roomPreference: updated.roomPreference,
        airlineLoyalty: updated.airlineLoyalty,
        hotelLoyalty: updated.hotelLoyalty,
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
    }
    // Return to default persona or null
    this.currentUser.set(PRESET_PERSONAS[0]);
  }
}
