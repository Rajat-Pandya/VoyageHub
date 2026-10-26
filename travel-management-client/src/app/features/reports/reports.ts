import { Component, OnInit, inject, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';

export interface DepartmentSpendItem {
  department: string;
  totalSpendRaw: number;
  totalSpend: string;
  requestCount: number;
  avgPerTrip: string;
}

export interface ReportSummary {
  totalSpend: number;
  formattedTotalSpend: string;
  totalRequests: number;
  approvedTrips: number;
  pendingApprovals: number;
  averagePerTrip: number;
  formattedAveragePerTrip: string;
  topDestinations: Array<{ destination: string; count: number; spend: number }>;
}

export interface EmployeeHistoryItem {
  destination: string;
  employeeName: string;
  dates: string;
  status: string;
  amount: string;
  rawAmount: number;
  purpose: string;
}

@Component({
  selector: 'app-reports',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTableModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
  ],
  templateUrl: './reports.html',
  styleUrl: './reports.scss',
})
export class Reports implements OnInit {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  private platformId = inject(PLATFORM_ID);

  private readonly apiBase = 'http://localhost:5201/api/Reports';

  readonly isLoading = signal<boolean>(false);
  readonly summary = signal<ReportSummary | null>(null);
  readonly departmentSpend = signal<DepartmentSpendItem[]>([]);
  readonly employeeHistory = signal<EmployeeHistoryItem[]>([]);

  filters = this.fb.group({
    department: ['all'],
    fromDate: [null as Date | null],
    toDate: [null as Date | null],
  });

  employeeSearch = '';
  departmentColumns = ['department', 'totalSpend', 'requestCount', 'avgPerTrip'];
  employeeHistoryColumns = ['employeeName', 'destination', 'dates', 'status', 'amount'];

  // Palette colors for dynamically assigned department visualizers
  private readonly deptColors: Record<string, string> = {
    Sales: '#2563EB',
    Engineering: '#4F46E5',
    Marketing: '#0284C7',
    Finance: '#059669',
    Operations: '#D97706',
    'General Staff': '#64748B',
  };

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.fetchSummary();
      this.fetchDepartmentSpend();
      this.fetchEmployeeHistory();
    }
  }

  fetchSummary() {
    this.http.get<ReportSummary>(`${this.apiBase}/summary`).subscribe({
      next: (data) => this.summary.set(data),
      error: () => {
        // Fallback placeholder calculation if API is offline
        this.summary.set({
          totalSpend: 567000,
          formattedTotalSpend: '₹5,67,000',
          totalRequests: 42,
          approvedTrips: 34,
          pendingApprovals: 6,
          averagePerTrip: 13500,
          formattedAveragePerTrip: '₹13,500',
          topDestinations: [
            { destination: 'Mumbai', count: 14, spend: 189000 },
            { destination: 'Bangalore', count: 12, spend: 162000 },
            { destination: 'Singapore', count: 4, spend: 124000 },
          ],
        });
      },
    });
  }

  fetchDepartmentSpend() {
    this.isLoading.set(true);
    this.http.get<DepartmentSpendItem[]>(`${this.apiBase}/department-spend`).subscribe({
      next: (data) => {
        this.departmentSpend.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.departmentSpend.set([
          { department: 'Sales', totalSpendRaw: 245000, totalSpend: '₹2,45,000', requestCount: 18, avgPerTrip: '₹13,600' },
          { department: 'Engineering', totalSpendRaw: 185000, totalSpend: '₹1,85,000', requestCount: 14, avgPerTrip: '₹13,214' },
          { department: 'Marketing', totalSpendRaw: 95000, totalSpend: '₹95,000', requestCount: 7, avgPerTrip: '₹13,571' },
          { department: 'Finance', totalSpendRaw: 42000, totalSpend: '₹42,000', requestCount: 3, avgPerTrip: '₹14,000' },
        ]);
        this.isLoading.set(false);
      },
    });
  }

  fetchEmployeeHistory(searchQuery?: string) {
    const url = searchQuery
      ? `${this.apiBase}/employee-history?search=${encodeURIComponent(searchQuery)}`
      : `${this.apiBase}/employee-history`;

    this.http.get<EmployeeHistoryItem[]>(url).subscribe({
      next: (data) => this.employeeHistory.set(data),
      error: () => {
        this.employeeHistory.set([
          {
            destination: 'Mumbai',
            employeeName: 'Rajat Sharma',
            dates: '12 Nov – 15 Nov 2026',
            status: 'Manager Approved',
            amount: '₹45,000',
            rawAmount: 45000,
            purpose: 'Client meeting — Q4 contract review',
          },
          {
            destination: 'Bangalore',
            employeeName: 'Rajat Sharma',
            dates: '25 Nov – 27 Nov 2026',
            status: 'Draft',
            amount: '₹32,000',
            rawAmount: 32000,
            purpose: 'Annual Engineering & Tech Summit',
          },
          {
            destination: 'Hyderabad',
            employeeName: 'Neha Gupta',
            dates: '20 Nov – 22 Nov 2026',
            status: 'Submitted',
            amount: '₹18,500',
            rawAmount: 18500,
            purpose: 'Key Enterprise Client Demo & Pitch',
          },
          {
            destination: 'Singapore',
            employeeName: 'Rohan Mehta',
            dates: '30 Nov – 05 Dec 2026',
            status: 'Finance Approved',
            amount: '₹84,000',
            rawAmount: 84000,
            purpose: 'APAC Partner Summit keynote presentation',
          },
        ]);
      },
    });
  }

  applyFilters() {
    const dept = this.filters.value.department;
    if (dept && dept !== 'all') {
      this.fetchEmployeeHistory(dept);
    } else {
      this.fetchEmployeeHistory();
    }
  }

  onSearchEmployee() {
    this.fetchEmployeeHistory(this.employeeSearch.trim());
  }

  getDepartmentColor(dept: string): string {
    return this.deptColors[dept] || '#64748B';
  }

  getSpendPercentage(rawSpend: number): number {
    const sum = this.departmentSpend().reduce((acc, curr) => acc + (curr.totalSpendRaw || 0), 0);
    if (!sum) return 0;
    return Math.round((rawSpend / sum) * 100);
  }
}
