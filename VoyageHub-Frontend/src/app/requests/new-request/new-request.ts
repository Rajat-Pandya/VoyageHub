import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VoyageService } from '../../services/voyage.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-new-request',
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './new-request.html',
  styleUrl: './new-request.css'
})
export class NewRequest {
  private readonly voyageService = inject(VoyageService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  currentStep = signal<1 | 2 | 3>(1);
  isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // Form Fields
  tripPurpose = '';
  destinationCity = '';
  startDate = '';
  endDate = '';
  estimatedBudget = 0;
  travelClass = 'Economy (Standard)';
  detailedJustification = '';

  // Segments (Flight / Train)
  flightCarrier = '';
  flightOrigin = '';
  flightDestination = '';
  flightDeparture = '';
  flightCost = 0;

  // Stay (Hotel)
  hotelName = '';
  hotelNights = 1;
  hotelCost = 0;

  // Policy threshold check (e.g. standard domestic departmental limit is ₹35,000)
  get isPolicyExceeded(): boolean {
    return this.estimatedBudget > 35000;
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
            mode: 'Flight',
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
      startDate: this.startDate,
      endDate: this.endDate,
      estimatedBudget: this.estimatedBudget,
      travelClass: this.travelClass,
      justification: this.detailedJustification,
      saveAsDraft: saveAsDraft,
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
