import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, DEMO_PASSWORD, PRESET_PERSONAS, UserProfile } from '../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  email = 'rohan.sharma@voyagehub.in';
  password = DEMO_PASSWORD;
  stayLoggedIn = true;

  // Forgot password state
  forgotEmail = '';
  readonly viewMode = signal<'login' | 'forgot'>('login');
  readonly showPassword = signal(false);

  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly devResetToken = signal<string | null>(null);
  readonly personas = PRESET_PERSONAS;

  ngOnInit(): void {
    const mode = this.route.snapshot.data['mode'];
    if (mode === 'forgot') {
      this.openForgotPassword();
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(v => !v);
  }

  openForgotPassword(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.devResetToken.set(null);
    this.forgotEmail = this.email || '';
    this.viewMode.set('forgot');
  }

  backToLogin(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.devResetToken.set(null);
    this.viewMode.set('login');
    if (this.router.url.startsWith('/forgot-password')) {
      void this.router.navigate(['/login']);
    }
  }

  quickSelect(persona: UserProfile): void {
    this.email = persona.email;
    this.password = DEMO_PASSWORD;
    this.submitLogin();
  }

  submitLogin(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.isSubmitting.set(true);

    this.authService.loginUser(this.email, this.password).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        if (res.user?.role === 'Manager') {
          void this.router.navigate(['/approvals']);
        } else if (res.user?.role === 'Finance') {
          void this.router.navigate(['/expenses']);
        } else if (res.user?.role === 'Admin') {
          void this.router.navigate(['/policies']);
        } else {
          void this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.message || 'Login failed. Please check credentials.');
      }
    });
  }

  submitForgotPassword(): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.devResetToken.set(null);

    const trimmedEmail = this.forgotEmail.trim();
    if (!trimmedEmail) {
      this.errorMessage.set('Please enter your corporate email address.');
      return;
    }

    this.isSubmitting.set(true);
    this.authService.forgotPassword(trimmedEmail).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.successMessage.set(res.message);
        if (res.resetToken) {
          this.devResetToken.set(res.resetToken);
        }
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.message || 'Unable to process password reset request.');
      }
    });
  }

  goToResetPassword(withToken = false): void {
    const queryParams: Record<string, string> = {};
    if (this.forgotEmail.trim()) {
      queryParams['email'] = this.forgotEmail.trim();
    }
    if (withToken && this.devResetToken()) {
      queryParams['token'] = this.devResetToken()!;
    }
    void this.router.navigate(['/reset-password'], { queryParams });
  }
}
