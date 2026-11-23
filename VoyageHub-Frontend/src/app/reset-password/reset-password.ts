import { Component, OnInit, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-reset-password',
  imports: [FormsModule],
  templateUrl: './reset-password.html',
  styleUrls: ['../login/login.css', './reset-password.css']
})
export class ResetPassword implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly platformId = inject(PLATFORM_ID);

  email = '';
  token = '';
  newPassword = '';
  confirmPassword = '';

  readonly hasPrefilledLink = signal(false);
  readonly isVerifyingToken = signal(false);
  readonly tokenInvalidError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly resetCompleted = signal(false);
  readonly showNewPassword = signal(false);
  readonly showConfirmPassword = signal(false);

  ngOnInit(): void {
    const queryMap = this.route.snapshot.queryParamMap;
    this.email = queryMap.get('email') || '';
    this.token = queryMap.get('token') || '';

    if (this.email && this.token) {
      this.hasPrefilledLink.set(true);
      if (isPlatformBrowser(this.platformId)) {
        this.verifyLinkToken();
      }
    }
  }

  private verifyLinkToken(): void {
    this.isVerifyingToken.set(true);
    this.tokenInvalidError.set(null);

    this.authService.verifyResetToken(this.email, this.token).subscribe({
      next: () => {
        this.isVerifyingToken.set(false);
      },
      error: (err) => {
        this.isVerifyingToken.set(false);
        this.tokenInvalidError.set(err.message || 'This password reset link is invalid or has expired.');
      }
    });
  }

  submitResetPassword(): void {
    this.errorMessage.set(null);

    if (!this.email.trim()) {
      this.errorMessage.set('Corporate email address is required.');
      return;
    }

    if (!this.token.trim()) {
      this.errorMessage.set('Recovery token is required.');
      return;
    }

    if (!this.newPassword || this.newPassword.length < 6) {
      this.errorMessage.set('New password must be at least 6 characters long.');
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage.set('Passwords do not match.');
      return;
    }

    this.isSubmitting.set(true);
    this.authService.resetPassword(this.email.trim(), this.token.trim(), this.newPassword).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.resetCompleted.set(true);
        this.successMessage.set(res.message || 'Your password has been successfully reset.');
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(err.message || 'Failed to reset password.');
      }
    });
  }

  goToLogin(): void {
    void this.router.navigate(['/login']);
  }

  requestNewLink(): void {
    void this.router.navigate(['/forgot-password']);
  }
}

