import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { TravelRequestService, UserRole } from './travel-request.service';

export type { UserRole };

export interface User {
  id: string;
  email: string;
  name: string;
  department: string;
  role: UserRole;
  avatarInitials: string;
}

export const DEMO_USERS: Record<UserRole, User> = {
  Employee: {
    id: 'emp-01',
    email: 'employee@company.com',
    name: 'Rajat Sharma',
    department: 'Corporate Travel',
    role: 'Employee',
    avatarInitials: 'RS',
  },
  Manager: {
    id: 'mgr-01',
    email: 'manager@company.com',
    name: 'Priya Nair',
    department: 'Engineering Dept',
    role: 'Manager',
    avatarInitials: 'PN',
  },
  Finance: {
    id: 'fin-01',
    email: 'finance@company.com',
    name: 'Anand Verma',
    department: 'Corporate Finance',
    role: 'Finance',
    avatarInitials: 'AV',
  },
  Admin: {
    id: 'adm-01',
    email: 'admin@company.com',
    name: 'Vikram Singhania',
    department: 'Enterprise IT Admin',
    role: 'Admin',
    avatarInitials: 'VS',
  },
};

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private router = inject(Router);
  private travelService = inject(TravelRequestService);

  private readonly _currentUser = signal<User | null>(this.getInitialUser());
  readonly currentUser = this._currentUser.asReadonly();
  readonly isLoggedIn = computed(() => !!this._currentUser());

  constructor() {
    const user = this._currentUser();
    if (user) {
      this.syncServiceWithUser(user);
    }
  }

  private getInitialUser(): User | null {
    if (typeof localStorage === 'undefined') return DEMO_USERS.Employee;
    const stored = localStorage.getItem('tms_user');
    if (stored) {
      try {
        return JSON.parse(stored) as User;
      } catch {
        return DEMO_USERS.Employee;
      }
    }
    // Default to initial logged-in user for developer ease
    return DEMO_USERS.Employee;
  }

  login(email: string, _password?: string): boolean {
    const cleanEmail = email.trim().toLowerCase();
    let matchedUser: User | undefined = Object.values(DEMO_USERS).find(
      u => u.email.toLowerCase() === cleanEmail
    );

    // If custom email entered, construct a user profile
    if (!matchedUser) {
      const namePart = cleanEmail.split('@')[0] || 'User';
      const capitalized = namePart.charAt(0).toUpperCase() + namePart.slice(1);
      matchedUser = {
        id: `usr-${Date.now()}`,
        email: cleanEmail,
        name: capitalized,
        department: 'General Staff',
        role: 'Employee',
        avatarInitials: (namePart.slice(0, 2) || 'US').toUpperCase(),
      };
    }

    this.setCurrentUser(matchedUser);
    this.router.navigate(['/dashboard']);
    return true;
  }

  loginAsDemo(role: UserRole) {
    const demo = DEMO_USERS[role];
    if (demo) {
      this.setCurrentUser(demo);
      this.router.navigate(['/dashboard']);
    }
  }

  logout() {
    this._currentUser.set(null);
    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('tms_user');
    }
    this.router.navigate(['/login']);
  }

  private setCurrentUser(user: User) {
    this._currentUser.set(user);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem('token', `mock-jwt-token-for-${user.id}`);
      localStorage.setItem('tms_user', JSON.stringify(user));
    }
    this.syncServiceWithUser(user);
  }

  private syncServiceWithUser(user: User) {
    this.travelService.setRole(user.role);
    this.travelService.currentUserName.set(user.name);
  }
}
