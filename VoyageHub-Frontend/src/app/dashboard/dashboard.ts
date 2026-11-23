import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { VoyageService, TravelRequestItem, DashboardStats } from '../services/voyage.service';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, CommonModule, CurrencyPipe, FormsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  readonly authService = inject(AuthService);
  private readonly voyageService = inject(VoyageService);
  private readonly router = inject(Router);

  readonly stats = signal<DashboardStats>({
    draftsCount: 0,
    pendingCount: 0,
    upcomingTripsCount: 0,
    totalReimbursement: 0
  });

  readonly requests = signal<TravelRequestItem[]>([]);
  readonly isLoading = signal(true);
  readonly selectedFilter = signal<'All' | 'Pending' | 'Approved'>('All');
  searchQuery = '';

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading.set(true);

    this.voyageService.getDashboardStats().subscribe({
      next: (s) => this.stats.set(s),
      error: () => {}
    });

    this.fetchRequests();
  }

  fetchRequests(): void {
    const filter = this.selectedFilter();

    this.voyageService.getRequests(filter, this.searchQuery, 8).subscribe({
      next: (data) => {
        this.requests.set(data || []);
        this.isLoading.set(false);
      },
      error: () => {
        this.requests.set([]);
        this.isLoading.set(false);
      }
    });
  }

  setFilter(filter: 'All' | 'Pending' | 'Approved'): void {
    this.selectedFilter.set(filter);
    this.fetchRequests();
  }

  onSearch(): void {
    this.fetchRequests();
  }

  viewDetail(req: TravelRequestItem): void {
    void this.router.navigate(['/requests', req.id]);
  }
}
