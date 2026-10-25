import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { Component, inject } from '@angular/core';
import { Timeline, TimelineEntry } from '../../../shared/components/timeline/timeline';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-request-detail',
  imports: [MatCardModule, MatChipsModule, MatStepperModule, MatTabsModule,
    MatTableModule, MatButtonModule, MatIconModule, Timeline],
  templateUrl: './request-detail.html',
  styleUrl: './request-detail.scss',
})
export class RequestDetail {
  private route = inject(ActivatedRoute);
  requestId = this.route.snapshot.paramMap.get('id');

  // MOCK DATA — replaced by a real service call once the API exists
  request = {
    destination: 'Mumbai',
    dates: '12 Nov – 15 Nov',
    purpose: 'Client meeting — Q4 contract review',
    status: 'Manager Approved' // Draft | Submitted | Manager Approved | Finance Approved | Rejected | Closed
  };

  steps = ['Submitted', 'Manager Approved', 'Finance Approved', 'Closed'];
  currentStepIndex = this.steps.indexOf(this.request.status);

  itineraryColumns = ['type', 'details', 'dates'];
  itinerary = [
    { type: 'Flight', details: 'Vadodara → Mumbai', dates: '12 Nov, 08:00' },
    { type: 'Stay', details: 'Taj Lands End', dates: '12 Nov – 15 Nov' },
  ];

  expenseColumns = ['category', 'amount', 'proof', 'status'];
  expenses = [
    { category: 'Transport', amount: '₹1,200', proof: 'receipt_1.pdf', status: 'Pending' },
    { category: 'Stay', amount: '₹6,000', proof: 'receipt_2.pdf', status: 'Reimbursed' },
  ];

  history: TimelineEntry[] = [
    { actor: 'Rajat (Employee)', action: 'Submitted the request', timestamp: '05 Oct, 10:12 AM' },
    { actor: 'Priya (Manager)', action: 'Approved', timestamp: '06 Oct, 09:40 AM' },
  ];
}

