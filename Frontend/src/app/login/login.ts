import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

export interface DemoPersona {
  id: string;
  name: string;
  role: 'Employee' | 'Manager' | 'Finance' | 'Admin';
  roleTitle: string;
  department: string;
  email: string;
  avatar: string;
  badgeColor: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  // Form Fields
  email = signal<string>('rajat.employee@corp-tms.com');
  password = signal<string>('TravelPass2026!');
  rememberMe = signal<boolean>(true);

  // UI Interactive States
  showPassword = signal<boolean>(false);
  isCapsLockOn = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  loadingAction = signal<string>('');
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  selectedPersonaId = signal<string>('employee');

  // Forgot Password Modal
  showForgotModal = signal<boolean>(false);
  forgotEmail = signal<string>('');
  forgotSubmitted = signal<boolean>(false);
  forgotCountdown = signal<number>(0);
  private countdownTimer: any = null;



  // TMS Pre-configured Demo Personas
  readonly personas: DemoPersona[] = [
    {
      id: 'employee',
      name: 'Rajat Pandya',
      role: 'Employee',
      roleTitle: 'Software Engineer',
      department: 'Engineering',
      email: 'rajat.employee@corp-tms.com',
      avatar: 'RP',
      badgeColor: '#0ea5e9'
    },
    {
      id: 'manager',
      name: 'Sarah Jenkins',
      role: 'Manager',
      roleTitle: 'Engineering Director (1st Approver)',
      department: 'Engineering',
      email: 'sarah.manager@corp-tms.com',
      avatar: 'SJ',
      badgeColor: '#8b5cf6'
    },
    {
      id: 'finance',
      name: 'David Vance',
      role: 'Finance',
      roleTitle: 'Lead Finance Auditor (2nd Approver)',
      department: 'Finance & Treasury',
      email: 'david.finance@corp-tms.com',
      avatar: 'DV',
      badgeColor: '#10b981'
    },
    {
      id: 'admin',
      name: 'Elena Rostova',
      role: 'Admin',
      roleTitle: 'Corporate Travel Administrator',
      department: 'Operations',
      email: 'elena.admin@corp-tms.com',
      avatar: 'ER',
      badgeColor: '#f59e0b'
    }
  ];

  // Quick select persona
  selectPersona(persona: DemoPersona) {
    this.selectedPersonaId.set(persona.id);
    this.email.set(persona.email);
    this.password.set('TravelPass2026!');
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  constructor(private router: Router) {}

  // Toggle password visibility
  togglePasswordVisibility() {
    this.showPassword.update((val) => !val);
  }

  // Detect Caps Lock state
  onKeyCheckCaps(event: KeyboardEvent) {
    const isCaps = event.getModifierState && event.getModifierState('CapsLock');
    this.isCapsLockOn.set(!!isCaps);
  }

  // Submit Sign In Form
  onSubmitSignIn() {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const emailVal = this.email().trim();
    const passwordVal = this.password();

    if (!emailVal || !emailVal.includes('@')) {
      this.errorMessage.set('Please enter a valid corporate email address.');
      return;
    }

    if (!passwordVal || passwordVal.length < 6) {
      this.errorMessage.set('Password must be at least 6 characters long.');
      return;
    }

    this.isLoading.set(true);
    this.loadingAction.set('Verifying credentials & corporate permissions...');

    // Simulate authenticating against TMS backend
    setTimeout(() => {
      this.isLoading.set(false);
      this.loadingAction.set('');

      const matchedPersona = this.personas.find(p => p.email.toLowerCase() === emailVal.toLowerCase());
      const roleName = matchedPersona ? matchedPersona.role : 'Corporate User';
      const userName = matchedPersona ? matchedPersona.name : emailVal.split('@')[0];

      this.successMessage.set(
        `Welcome back, ${userName}! Authenticated as [${roleName}]. Redirecting to workspace...`
      );

      // Store simulated session
      if (typeof window !== 'undefined' && window.sessionStorage) {
        window.sessionStorage.setItem('tms_user', JSON.stringify({
          email: emailVal,
          name: userName,
          role: roleName,
          token: 'jwt-simulated-' + Date.now()
        }));
      }

      // Navigate to dashboard
      setTimeout(() => {
        this.router.navigate(['/dashboard']);
      }, 500);
    }, 1000);
  }

  // Simulated Single Sign-On (Microsoft / Google / Okta)
  onTriggerSSO(provider: 'Microsoft 365' | 'Google' | 'Okta') {
    this.isLoading.set(true);
    this.loadingAction.set(`Connecting to ${provider} Enterprise SSO...`);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    setTimeout(() => {
      this.isLoading.set(false);
      this.loadingAction.set('');
      this.email.set('rajat.employee@corp-tms.com');
      this.selectedPersonaId.set('employee');
      this.successMessage.set(`Successfully authenticated via ${provider} SSO!`);

      if (typeof window !== 'undefined' && window.sessionStorage) {
        window.sessionStorage.setItem('tms_user', JSON.stringify({
          email: 'rajat.employee@corp-tms.com',
          name: 'Rajat Pandya',
          role: 'Employee',
          token: 'jwt-sso-' + Date.now()
        }));
      }

      setTimeout(() => {
        this.router.navigate(['/dashboard']);
      }, 500);
    }, 1200);
  }

  // Forgot password flow
  openForgotModal() {
    this.forgotEmail.set(this.email());
    this.forgotSubmitted.set(false);
    this.showForgotModal.set(true);
  }

  closeForgotModal() {
    this.showForgotModal.set(false);
    if (this.countdownTimer) {
      clearInterval(this.countdownTimer);
    }
  }

  submitForgotPassword() {
    const emailVal = this.forgotEmail().trim();
    if (!emailVal || !emailVal.includes('@')) {
      return;
    }

    this.forgotSubmitted.set(true);
    this.forgotCountdown.set(60);

    if (this.countdownTimer) {
      clearInterval(this.countdownTimer);
    }

    this.countdownTimer = setInterval(() => {
      this.forgotCountdown.update(c => {
        if (c <= 1) {
          clearInterval(this.countdownTimer);
          return 0;
        }
        return c - 1;
      });
    }, 1000);
  }
}
