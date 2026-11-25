import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VoyageService, TravelRequestItem } from '../../services/voyage.service';
import { AuthService, getUserInitials } from '../../services/auth.service';
import { createPagination } from '../../shared/pagination';


@Component({
  selector: 'app-request-list',
  imports: [RouterLink, CommonModule, CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './request-list.html',
  styleUrl: './request-list.css'
})
export class RequestList implements OnInit {
  private readonly voyageService = inject(VoyageService);
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  readonly getUserInitials = getUserInitials;

  readonly requests = signal<TravelRequestItem[]>([]);
  readonly isLoading = signal(true);
  readonly scope = signal<'mine' | 'team'>('mine');
  readonly pagination = createPagination(this.requests, 10);

  searchQuery = '';
  selectedStatus = 'All Statuses';
  selectedDateRange = 'Last 6 Months';

  ngOnInit(): void {
    this.loadRequests();
  }

  setScope(scope: 'mine' | 'team'): void {
    if (this.scope() === scope) return;
    this.scope.set(scope);
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading.set(true);
    this.pagination.reset();
    const employeeId = this.scope() === 'mine' ? this.authService.currentUser()?.id : undefined;
    this.voyageService.getRequests(this.selectedStatus, this.searchQuery, 100, employeeId).subscribe({
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
    void this.router.navigate(['/requests', req.id]);
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
