import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TravelRequestService } from '../../../core/services/travel-request.service';

@Component({
  selector: 'app-new-request',
  imports: [
    CommonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    ReactiveFormsModule,
  ],
  templateUrl: './new-request.html',
  styleUrl: './new-request.scss',
})
export class NewRequest {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  readonly travelService = inject(TravelRequestService);

  readonly budgetPolicyLimit = this.travelService.budgetPolicyLimit;

  form = this.fb.group({
    destination: ['', Validators.required],
    startDate: [null as Date | null, Validators.required],
    endDate: [null as Date | null, Validators.required],
    purpose: ['', Validators.required],
    estimatedBudget: [null as number | null, [Validators.required, Validators.min(1)]],
  });

  submitting = false;

  // Live policy check evaluation per the workflow diagram (Node D)
  get policyViolation(): boolean {
    const budget = this.form.get('estimatedBudget')?.value;
    return !!budget && budget > this.budgetPolicyLimit;
  }

  private formatDate(date: Date | null): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('en-GB', { day: '2-digit', month: 'short' });
  }

  saveDraft() {
    if (this.form.get('destination')?.invalid) {
      this.form.get('destination')?.markAsTouched();
      return;
    }
    const val = this.form.getRawValue();
    const created = this.travelService.createRequest({
      destination: val.destination || 'Untitled Trip',
      startDate: this.formatDate(val.startDate),
      endDate: this.formatDate(val.endDate),
      purpose: val.purpose || 'Travel',
      estimatedBudget: val.estimatedBudget || 0,
      asDraft: true,
    });
    this.router.navigate(['/requests', created.id]);
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting = true;
    const val = this.form.getRawValue();
    const created = this.travelService.createRequest({
      destination: val.destination!,
      startDate: this.formatDate(val.startDate),
      endDate: this.formatDate(val.endDate),
      purpose: val.purpose!,
      estimatedBudget: val.estimatedBudget!,
      asDraft: false,
    });
    this.router.navigate(['/requests', created.id]);
  }
}
