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
    this.isSubmitting.set(true);
    const user = this.authService.currentUser();

    const payload = {
      purpose: this.tripPurpose,
      destination: this.destinationCity,
      startDate: this.startDate,
      endDate: this.endDate,
      estimatedBudget: this.estimatedBudget,
      travelClass: this.travelClass,
      justification: this.detailedJustification,
      saveAsDraft: saveAsDraft,
      segments: [
        {
          mode: 'Flight',
          carrier: this.flightCarrier,
          origin: this.flightOrigin,
          destination: this.flightDestination,
          departureTime: this.flightDeparture,
          cost: this.flightCost
        }
      ],
      stays: [
        {
          hotelName: this.hotelName,
          checkIn: this.startDate,
          checkOut: this.endDate,
          nights: this.hotelNights,
          cost: this.hotelCost
        }
      ]
    };

    this.voyageService.createRequest(payload).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        void this.router.navigate(['/requests', res.id]);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        void this.router.navigate(['/requests']);
      }
    });
  }
}
