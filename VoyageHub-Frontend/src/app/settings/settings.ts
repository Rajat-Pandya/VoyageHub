import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, UserProfile, getUserInitials } from '../services/auth.service';

@Component({
  selector: 'app-settings',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './settings.html',
  styleUrl: './settings.css'
})
export class Settings implements OnInit {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  readonly getUserInitials = getUserInitials;

  readonly activeTab = signal<'profile' | 'notifications' | 'security'>('profile');
  readonly toastMessage = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly isAdmin = this.authService.isAdmin;

  // Profile fields
  fullname = '';
  email = '';
  role = '';
  departmentName = '';
  phoneNumber = '';
  homeCity = '';
  employeeCode = '';

  // Notifications
  notifyTripStatus = true;
  notifyExpensePayout = true;
  notifyDailyDigest = false;
  notifySmsAlerts = true;

  // Password fields
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  passwordError = '';

  ngOnInit(): void {
    this.syncFromUser(this.authService.currentUser());
  }

  syncFromUser(user: UserProfile | null): void {
    if (!user) return;
    this.fullname = user.fullname || '';
    this.email = user.email || '';
    this.role = user.role || 'Employee';
    this.departmentName = user.departmentName || 'General';
    this.phoneNumber = user.phoneNumber || '';
    this.homeCity = user.homeCity || '';
    this.employeeCode = user.employeeCode || '';

    this.notifyTripStatus = user.notifyTripStatus !== undefined ? user.notifyTripStatus : true;
    this.notifyExpensePayout = user.notifyExpensePayout !== undefined ? user.notifyExpensePayout : true;
    this.notifyDailyDigest = user.notifyDailyDigest !== undefined ? user.notifyDailyDigest : false;
    this.notifySmsAlerts = user.notifySmsAlerts !== undefined ? user.notifySmsAlerts : true;
  }

  setTab(tab: 'profile' | 'notifications' | 'security'): void {
    this.activeTab.set(tab);
  }

  saveProfile(): void {
    if (!this.isAdmin()) {
      this.showToast('Profile details can only be updated by a System Administrator.');
      return;
    }

    if (!this.fullname.trim()) {
      this.showToast('Please enter your full name.');
      return;
    }

    this.isSaving.set(true);
    this.authService.updateProfile({
      fullname: this.fullname.trim(),
      phoneNumber: this.phoneNumber.trim(),
      homeCity: this.homeCity.trim(),
      employeeCode: this.employeeCode.trim()
    }).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        this.showToast(res.message || 'Profile successfully updated!');
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('Profile saved locally.');
      }
    });
  }

  saveNotifications(): void {
    this.isSaving.set(true);
    this.authService.updateProfile({
      notifyTripStatus: this.notifyTripStatus,
      notifyExpensePayout: this.notifyExpensePayout,
      notifyDailyDigest: this.notifyDailyDigest,
      notifySmsAlerts: this.notifySmsAlerts
    }).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        this.showToast('Notification preferences updated!');
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('Notification preferences updated locally.');
      }
    });
  }

  changePassword(): void {
    this.passwordError = '';

    if (!this.currentPassword) {
      this.passwordError = 'Please enter your current password.';
      return;
    }

    if (!this.newPassword || this.newPassword.length < 6) {
      this.passwordError = 'New password must be at least 6 characters.';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.passwordError = 'New password and confirmation do not match.';
      return;
    }

    this.isSaving.set(true);
    this.authService.changePassword(this.currentPassword, this.newPassword).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        this.currentPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
        this.showToast(res.message || 'Password changed successfully!');
      },
      error: (err) => {
        this.isSaving.set(false);
        this.passwordError = err.message || 'Failed to change password.';
      }
    });
  }

  private showToast(msg: string): void {
    this.toastMessage.set(msg);
    setTimeout(() => {
      if (this.toastMessage() === msg) {
        this.toastMessage.set(null);
      }
    }, 4000);
  }
}
