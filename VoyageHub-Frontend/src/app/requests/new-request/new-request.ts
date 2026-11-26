import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PolicyConfig, VoyageService } from '../../services/voyage.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-new-request',
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './new-request.html',
  styleUrl: './new-request.css'
})
export class NewRequest implements OnInit {
  private readonly voyageService = inject(VoyageService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  currentStep = signal<1 | 2 | 3>(1);
  isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly myPolicy = signal<PolicyConfig | null>(null);

  // Form Fields
  tripPurpose = '';
  destinationCity = '';
  isInternational = false;
  selfPaidItinerary = false;
  startDate = '';
  endDate = '';
  estimatedBudget = 0;
  travelClass = 'Economy (Standard)';
  detailedJustification = '';

  // Segments (Flight / Train / Cab / Bus)
  transportMode = 'Flight';
  flightCarrier = '';
  flightOrigin = '';
  flightDestination = '';
  flightDeparture = '';
  flightCost = 0;

  // Stay (Hotel)
  hotelName = '';
  hotelNights = 1;
  hotelCost = 0;

  ngOnInit(): void {
    this.voyageService.getMyPolicy().subscribe({
      next: (policy) => this.myPolicy.set(policy),
      error: () => this.myPolicy.set(null)
    });
  }

  get activeBudgetLimit(): number {
    const policy = this.myPolicy();
    if (!policy) return 0;
    return this.isInternational ? policy.intlBudgetLimit : policy.domesticLimit;
  }

  get isBudgetExceeded(): boolean {
    const policy = this.myPolicy();
    const limit = this.activeBudgetLimit;
    return Boolean(policy?.hardEnforcement && limit > 0 && this.estimatedBudget > limit);
  }

  get isClassExceeded(): boolean {
    const policy = this.myPolicy();
    if (!policy?.hardEnforcement || !policy.allowedClass || !this.travelClass) {
      return false;
    }
    const allowed = policy.allowedClass.trim().toUpperCase();
    const requested = this.travelClass.trim().toUpperCase();
    const isBusinessOrFirst = requested.includes('BUSINESS') || requested.includes('FIRST');
    return (allowed === 'ECONOMY' || allowed === 'ECONOMY+') && isBusinessOrFirst;
  }

  get isPolicyExceeded(): boolean {
    return this.isBudgetExceeded || this.isClassExceeded;
  }

  get itemizedTotal(): number {
    return (Number(this.flightCost) || 0) + (Number(this.hotelCost) || 0);
  }

  get showBudgetSyncHint(): boolean {
    return this.itemizedTotal > 0 && this.itemizedTotal > (Number(this.estimatedBudget) || 0);
  }

  syncEstimatedBudget(): void {
    this.estimatedBudget = this.itemizedTotal;
  }

  get policyAlertMessage(): string {
    const policy = this.myPolicy();
    if (!policy) return '';

    const parts: string[] = [];
    if (this.isBudgetExceeded) {
      const tripLabel = this.isInternational ? 'international' : 'domestic';
      parts.push(
        `Estimated budget (₹${this.estimatedBudget.toLocaleString('en-IN')}) exceeds your ${policy.departmentName} ${tripLabel} limit (₹${this.activeBudgetLimit.toLocaleString('en-IN')}).`
      );
    }
    if (this.isClassExceeded) {
      parts.push(
        `Requested travel class (${this.travelClass}) exceeds your allowed class (${policy.allowedClass}).`
      );
    }
    parts.push('This request will be flagged as a Policy Exception for Manager and Finance review.');
    return parts.join(' ');
  }

  nextStep(): void {
    if (this.currentStep() === 1) {
      this.currentStep.set(2);
    } else if (this.currentStep() === 2) {
      this.currentStep.set(3);
    }
  }

  prevStep(): void {
    if (this.currentStep() === 3) {
      this.currentStep.set(2);
    } else if (this.currentStep() === 2) {
      this.currentStep.set(1);
    }
  }

  saveDraft(): void {
    this.submitForm(true);
  }

  submitRequest(): void {
    this.submitForm(false);
  }

  private submitForm(saveAsDraft: boolean): void {
    this.errorMessage.set(null);

    if (!this.destinationCity.trim() || !this.tripPurpose.trim() || !this.startDate || !this.endDate) {
      this.errorMessage.set('Please fill in Destination, Purpose, Start Date, and End Date before submitting.');
      return;
    }

    this.isSubmitting.set(true);

    const hasSegment = Boolean(this.flightCarrier.trim() || this.flightOrigin.trim() || this.flightDestination.trim());
    const segments = hasSegment
      ? [
          {
            mode: this.transportMode,
            carrier: this.flightCarrier.trim(),
            origin: this.flightOrigin.trim(),
            destination: this.flightDestination.trim(),
            departureTime: this.flightDeparture || this.startDate,
            cost: this.flightCost
          }
        ]
      : [];

    const stays = this.hotelName.trim()
      ? [
          {
            hotelName: this.hotelName.trim(),
            checkIn: this.startDate,
            checkOut: this.endDate,
            nights: this.hotelNights,
            cost: this.hotelCost
          }
        ]
      : [];

    const payload = {
      purpose: this.tripPurpose.trim(),
      destination: this.destinationCity.trim(),
      isInternational: this.isInternational,
      startDate: this.startDate,
      endDate: this.endDate,
      estimatedBudget: this.estimatedBudget,
      travelClass: this.travelClass,
      justification: this.detailedJustification,
      saveAsDraft: saveAsDraft,
      selfPaidItinerary: this.selfPaidItinerary,
      segments,
      stays
    };

    this.voyageService.createRequest(payload).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        void this.router.navigate(['/requests', res.id]);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        const msg = err?.error?.message || err?.error?.title || 'Unable to save travel request. Please check your entries and try again.';
        this.errorMessage.set(msg);
      }
    });
  }
}
