import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, UserProfile } from '../services/auth.service';

@Component({
  selector: 'app-settings',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './settings.html',
  styleUrl: './settings.css'
})
export class Settings implements OnInit {
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly activeTab = signal<'profile' | 'travel' | 'notifications' | 'security'>('profile');
  readonly toastMessage = signal<string | null>(null);
  readonly isSaving = signal(false);
  readonly isAdmin = this.authService.isAdmin;

  // Preset Avatars for quick selection
  readonly presetAvatars: string[] = [
    'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?auto=format&fit=crop&w=120&h=120&q=80',
    'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=120&h=120&q=80',
    'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=120&h=120&q=80',
    'https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=120&h=120&q=80',
    'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=120&h=120&q=80',
    'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=120&h=120&q=80'
  ];

  // Profile fields
  fullname = '';
  email = '';
  role = '';
  departmentName = '';
  phoneNumber = '';
  homeCity = '';
  employeeCode = '';
  avatarUrl = '';

  // Travel preferences
  seatPreference = 'Aisle';
  mealPreference = 'Vegetarian (Hindu)';
  roomPreference = 'High Floor, Non-Smoking';
  airlineLoyalty = '';
  hotelLoyalty = '';

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

  syncFromUser(user: UserProfile): void {
    if (!user) return;
    this.fullname = user.fullname || '';
    this.email = user.email || '';
    this.role = user.role || 'Employee';
    this.departmentName = user.departmentName || 'General';
    this.phoneNumber = user.phoneNumber || '';
    this.homeCity = user.homeCity || '';
    this.employeeCode = user.employeeCode || '';
    this.avatarUrl = user.avatarUrl || this.presetAvatars[0];

    this.seatPreference = user.seatPreference || 'Aisle';
    this.mealPreference = user.mealPreference || 'Standard';
    this.roomPreference = user.roomPreference || 'Non-Smoking';
    this.airlineLoyalty = user.airlineLoyalty || '';
    this.hotelLoyalty = user.hotelLoyalty || '';

    this.notifyTripStatus = user.notifyTripStatus !== undefined ? user.notifyTripStatus : true;
    this.notifyExpensePayout = user.notifyExpensePayout !== undefined ? user.notifyExpensePayout : true;
    this.notifyDailyDigest = user.notifyDailyDigest !== undefined ? user.notifyDailyDigest : false;
    this.notifySmsAlerts = user.notifySmsAlerts !== undefined ? user.notifySmsAlerts : true;
  }

  setTab(tab: 'profile' | 'travel' | 'notifications' | 'security'): void {
    this.activeTab.set(tab);
  }

  selectAvatar(url: string): void {
    if (!this.isAdmin()) return;
    this.avatarUrl = url;
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
      avatarUrl: this.avatarUrl,
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

  saveTravelPreferences(): void {
    this.isSaving.set(true);
    this.authService.updateProfile({
      seatPreference: this.seatPreference,
      mealPreference: this.mealPreference,
      roomPreference: this.roomPreference,
      airlineLoyalty: this.airlineLoyalty.trim(),
      hotelLoyalty: this.hotelLoyalty.trim()
    }).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        this.showToast('Travel preferences saved successfully!');
      },
      error: () => {
        this.isSaving.set(false);
        this.showToast('Travel preferences saved locally.');
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
