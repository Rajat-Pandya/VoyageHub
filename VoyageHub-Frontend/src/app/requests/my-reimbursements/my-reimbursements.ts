import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TripReimbursementReport, VoyageService } from '../../services/voyage.service';

@Component({
  selector: 'app-my-reimbursements',
  imports: [CommonModule, CurrencyPipe, DatePipe, RouterLink],
  templateUrl: './my-reimbursements.html',
  styleUrl: './my-reimbursements.css'
})
export class MyReimbursements implements OnInit {
  private readonly voyageService = inject(VoyageService);

  readonly reports = signal<TripReimbursementReport[]>([]);
  readonly isLoading = signal(true);
  readonly statusFilter = signal<'All' | 'Pending' | 'Reimbursed' | 'Rejected' | 'No Claims'>('All');

  readonly filteredReports = computed(() => {
    const filter = this.statusFilter();
    const list = this.reports();
    if (filter === 'All') return list;
    return list.filter((r) => r.reportStatus === filter || (filter === 'Pending' && r.reportStatus === 'Partially Reimbursed'));
  });

  readonly totalPending = computed(() =>
    this.reports().reduce((s, r) => s + (r.pendingAmount || 0), 0)
  );

  readonly totalReimbursed = computed(() =>
    this.reports().reduce((s, r) => s + (r.reimbursedAmount || 0), 0)
  );

  readonly totalCorporatePaid = computed(() =>
    this.reports().reduce((s, r) => s + (r.corporatePaidAmount || 0), 0)
  );

  ngOnInit(): void {
    this.isLoading.set(true);
    this.voyageService.getMyReimbursementReports().subscribe({
      next: (list) => {
        this.reports.set(list || []);
        this.isLoading.set(false);
      },
      error: () => {
        this.reports.set([]);
        this.isLoading.set(false);
      }
    });
  }
}
