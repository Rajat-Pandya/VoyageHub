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

  // Form Fields (Indian Corporate Standards)
  tripPurpose = 'Client Quarterly Review & Tech Summit Bengaluru';
  destinationCity = 'Bengaluru, Karnataka';
  startDate = '2026-10-20';
  endDate = '2026-10-24';
  estimatedBudget = 42500;
  travelClass = 'Economy (Standard)';
  detailedJustification = 'Annual client review sessions with Infosys & Wipro engineering leadership.';

  // Segments (Flight / Train)
  flightCarrier = 'IndiGo 6E-5324';
  flightOrigin = 'BOM (Mumbai)';
  flightDestination = 'BLR (Bengaluru)';
  flightDeparture = '2026-10-20T08:15';
  flightCost = 9800;

  // Stay (Hotel)
  hotelName = 'The Leela Palace Bengaluru';
  hotelNights = 4;
  hotelCost = 28000;

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

    this.voyageService.createRequest(payload, user?.id).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.router.navigate(['/requests', res.id]);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.router.navigate(['/requests']);
      }
    });
  }
}
