import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { VoyageService, TravelRequestItem, DashboardStats } from '../services/voyage.service';

const FALLBACK_INDIAN_REQUESTS: TravelRequestItem[] = [
  {
    id: 'aa010101-0000-0000-0000-000000000001',
    requestCode: '#TR-2048',
    destination: 'Bengaluru, Karnataka',
    purpose: 'Client Review & Tech Summit',
    datesFormatted: '18 Jun – 22 Jun',
    startDate: '2026-06-18T00:00:00',
    endDate: '2026-06-22T00:00:00',
    submittedDate: '2026-06-01T10:00:00',
    estimatedBudget: 42500.00,
    approverName: 'Priya Nair',
    status: 'In Review',
    policyCompliance: 'Flagged',
    travelClass: 'Economy (Standard)',
    employeeName: 'Rohan Sharma',
    departmentName: 'Sales & Business Dev (Mumbai HQ)'
  },
  {
    id: 'aa020202-0000-0000-0000-000000000002',
    requestCode: '#TR-2041',
    destination: 'New Delhi, NCR',
    purpose: 'Government Partner Alignment',
    datesFormatted: '02 Jul – 09 Jul',
    startDate: '2026-07-02T00:00:00',
    endDate: '2026-07-09T00:00:00',
    submittedDate: '2026-05-15T09:30:00',
    estimatedBudget: 28000.00,
    approverName: 'Priya Nair',
    status: 'Approved',
    policyCompliance: 'Standard',
    travelClass: 'Economy (Standard)',
    employeeName: 'Rohan Sharma',
    departmentName: 'Sales & Business Dev (Mumbai HQ)'
  },
  {
    id: 'aa030303-0000-0000-0000-000000000003',
    requestCode: '#TR-2035',
    destination: 'Hyderabad, Telangana',
    purpose: 'HITEC City Solution Workshop',
    datesFormatted: '10 May – 12 May',
    startDate: '2026-05-10T00:00:00',
    endDate: '2026-05-12T00:00:00',
    submittedDate: '2026-05-01T14:15:00',
    estimatedBudget: 18500.00,
    approverName: 'Priya Nair',
    status: 'Completed',
    policyCompliance: 'Standard',
    travelClass: 'Economy (Standard)',
    employeeName: 'Rohan Sharma',
    departmentName: 'Sales & Business Dev (Mumbai HQ)'
  },
  {
    id: 'aa040404-0000-0000-0000-000000000004',
    requestCode: '#TR-2029',
    destination: 'Chennai, Tamil Nadu',
    purpose: 'Regional Ops Training',
    datesFormatted: '11 Apr – 15 Apr',
    startDate: '2026-04-11T00:00:00',
    endDate: '2026-04-15T00:00:00',
    submittedDate: '2026-04-01T11:00:00',
    estimatedBudget: 16200.00,
    approverName: 'Priya Nair',
    status: 'Rejected',
    policyCompliance: 'Standard',
    travelClass: 'Economy (Standard)',
    employeeName: 'Rohan Sharma',
    departmentName: 'Sales & Business Dev (Mumbai HQ)'
  },
  {
    id: 'aa050505-0000-0000-0000-000000000005',
    requestCode: '#TR-2022',
    destination: 'Pune, Maharashtra',
    purpose: 'Client Site Delivery Sprint',
    datesFormatted: '04 Mar – 08 Mar',
    startDate: '2026-03-04T00:00:00',
    endDate: '2026-03-08T00:00:00',
    submittedDate: '2026-02-20T08:00:00',
    estimatedBudget: 12400.00,
    approverName: 'Priya Nair',
    status: 'Draft',
    policyCompliance: 'Standard',
    travelClass: 'Economy (Standard)',
    employeeName: 'Rohan Sharma',
    departmentName: 'Sales & Business Dev (Mumbai HQ)'
  }
];

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
    draftsCount: 1,
    pendingCount: 2,
    upcomingTripsCount: 1,
    totalReimbursement: 34500.00
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
        if (data && data.length > 0) {
          this.requests.set(data);
        } else {
          this.applyFallbackFilter();
        }
        this.isLoading.set(false);
      },
      error: () => {
        this.applyFallbackFilter();
        this.isLoading.set(false);
      }
    });
  }

  private applyFallbackFilter(): void {
    let list = [...FALLBACK_INDIAN_REQUESTS];
    const filter = this.selectedFilter();
    if (filter === 'Pending') {
      list = list.filter(r => r.status === 'In Review' || r.status === 'Manager Review' || r.status === 'Pending');
    } else if (filter === 'Approved') {
      list = list.filter(r => r.status === 'Approved');
    }

    if (this.searchQuery && this.searchQuery.trim()) {
      const q = this.searchQuery.trim().toLowerCase();
      list = list.filter(r =>
        r.destination.toLowerCase().includes(q) ||
        r.purpose.toLowerCase().includes(q) ||
        r.requestCode.toLowerCase().includes(q)
      );
    }
    this.requests.set(list);
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
