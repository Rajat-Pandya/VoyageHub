import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VoyageService, TravelRequestItem } from '../../services/voyage.service';
import { AuthService } from '../../services/auth.service';

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
  selector: 'app-request-list',
  imports: [RouterLink, CommonModule, CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './request-list.html',
  styleUrl: './request-list.css'
})
export class RequestList implements OnInit {
  private readonly voyageService = inject(VoyageService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly requests = signal<TravelRequestItem[]>([]);
  readonly isLoading = signal(true);

  searchQuery = '';
  selectedStatus = 'All Statuses';
  selectedDateRange = 'Last 6 Months';

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading.set(true);

    this.voyageService.getRequests(this.selectedStatus, this.searchQuery, 20).subscribe({
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
    if (this.selectedStatus && this.selectedStatus !== 'All Statuses' && this.selectedStatus !== 'All') {
      const s = this.selectedStatus.toLowerCase();
      list = list.filter(r => {
        if (s === 'pending' || s === 'in review' || s === 'manager review') {
          return r.status === 'In Review' || r.status === 'Manager Review' || r.status === 'Pending';
        }
        return r.status.toLowerCase().includes(s);
      });
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

  getAvatarClass(name: string): string {
    const char = (name || 'B').toUpperCase().charAt(0);
    if ('ABC'.includes(char)) return 'avatar-indigo';
    if ('DEF'.includes(char)) return 'avatar-emerald';
    if ('GHI'.includes(char)) return 'avatar-amber';
    if ('LMN'.includes(char)) return 'avatar-green';
    return 'avatar-slate';
  }

  getAvatarColor(name: string): string {
    const char = (name || 'B').toUpperCase().charAt(0);
    if ('ABC'.includes(char)) return 'background-color: #e0e7ff; color: #4338ca;';
    if ('DEF'.includes(char)) return 'background-color: #ecfdf5; color: #047857;';
    if ('GHI'.includes(char)) return 'background-color: #fffbeb; color: #b45309;';
    if ('LMN'.includes(char)) return 'background-color: #dcfce7; color: #15803d;';
    return 'background-color: #f1f5f9; color: #475569;';
  }

  viewDetail(req: TravelRequestItem): void {
    this.router.navigate(['/requests', req.id]);
  }

  exportCsv(): void {
    const items = this.requests();
    let csv = 'RequestCode,Destination,Purpose,StartDate,EndDate,Budget,Status\n';
    items.forEach(i => {
      csv += `"${i.requestCode}","${i.destination}","${i.purpose}","${i.startDate}","${i.endDate}","${i.estimatedBudget}","${i.status}"\n`;
    });
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'VoyageHub_Travel_Requests.csv';
    a.click();
  }
}
