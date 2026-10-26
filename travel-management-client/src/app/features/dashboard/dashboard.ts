import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TravelRequestService } from '../../core/services/travel-request.service';

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    RouterLink,
    MatCardModule,
    MatTableModule,
    MatChipsModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  readonly travelService = inject(TravelRequestService);

  readonly draftCount = computed(() => {
    return this.travelService.requests().filter(r => r.status === 'Draft').length;
  });

  readonly pendingApprovalCount = computed(() => {
    return this.travelService.requests().filter(
      r => r.status === 'Submitted' || r.status === 'Manager Approved'
    ).length;
  });

  readonly upcomingTripsCount = computed(() => {
    return this.travelService.requests().filter(
      r => r.status === 'Finance Approved'
    ).length;
  });

  readonly pendingReimbursementAmount = computed(() => {
    let sum = 0;
    for (const req of this.travelService.requests()) {
      for (const exp of req.expenses) {
        if (exp.status === 'Pending') {
          sum += exp.numericAmount;
        }
      }
    }
    return sum;
  });

  readonly recentRequests = computed(() => {
    return this.travelService.requests().slice(0, 5);
  });

  displayedColumns = ['destination', 'dates', 'budget', 'status', 'actions'];
}
