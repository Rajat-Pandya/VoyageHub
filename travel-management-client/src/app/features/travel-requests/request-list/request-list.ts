import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { TravelRequestService } from '../../../core/services/travel-request.service';

@Component({
  selector: 'app-request-list',
  imports: [
    CommonModule,
    RouterLink,
    FormsModule,
    MatTableModule,
    MatChipsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatIconModule,
  ],
  templateUrl: './request-list.html',
  styleUrl: './request-list.scss',
})
export class RequestList {
  readonly travelService = inject(TravelRequestService);

  readonly selectedStatus = signal<string>('all');

  readonly filteredRequests = computed(() => {
    const list = this.travelService.requests();
    const filter = this.selectedStatus();
    if (filter === 'all') return list;
    return list.filter(r => r.status.toLowerCase().replace(' ', '-') === filter);
  });

  displayedColumns = ['destination', 'dates', 'budget', 'policy', 'status', 'actions'];
}
