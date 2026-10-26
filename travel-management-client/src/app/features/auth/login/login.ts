import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Auth, UserRole } from '../../../core/services/auth';

import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  readonly auth = inject(Auth);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  constructor() {
    if (this.auth.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }
  }

  isSubmitting = false;
  errorMessage = '';

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const { email, password } = this.loginForm.getRawValue();
    try {
      this.auth.login(email!, password!);
    } catch {
      this.errorMessage = 'Unable to sign in. Please verify your credentials.';
    } finally {
      this.isSubmitting = false;
    }
  }

  quickLoginAs(role: UserRole) {
    this.auth.loginAsDemo(role);
  }

  onSsoLogin() {
    this.quickLoginAs('Employee');
  }
}
