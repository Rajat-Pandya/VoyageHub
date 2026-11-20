import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { VoyageService, AnalyticsOverview } from '../../services/voyage.service';

@Component({
  selector: 'app-analytics-reports',
  imports: [CommonModule, CurrencyPipe],
  templateUrl: './analytics-reports.html',
  styleUrl: './analytics-reports.css'
})
export class AnalyticsReports implements OnInit {
  private readonly voyageService = inject(VoyageService);

  readonly analytics = signal<AnalyticsOverview>({
    totalSpent: 142500,
    vsLastPeriodPercent: -12.5,
    monthlySpend: [
      { month: 'Jan', amount: 24000 },
      { month: 'Feb', amount: 28000 },
      { month: 'Mar', amount: 22000 },
      { month: 'Apr', amount: 31000 },
      { month: 'May', amount: 29000 },
      { month: 'Jun', amount: 25000 }
    ],
    departmentSpend: [
      { department: 'Sales & Marketing', trips: 42, avgCost: 1850, total: 77700 },
      { department: 'Engineering', trips: 18, avgCost: 2100, total: 37800 },
      { department: 'Human Resources', trips: 8, avgCost: 950, total: 7600 }
    ],
    categorySpend: [
      { category: 'Airfare', percentage: 55, amount: 78375, color: '#4f46e5' },
      { category: 'Hotel', percentage: 25, amount: 35625, color: '#f59e0b' },
      { category: 'Meals', percentage: 12, amount: 17100, color: '#10b981' },
      { category: 'Transport', percentage: 8, amount: 11400, color: '#ef4444' }
    ]
  });

  ngOnInit(): void {
    this.voyageService.getAnalytics().subscribe({
      next: (data) => this.analytics.set(data),
      error: () => {}
    });
  }

  exportPdf(): void {
    window.print();
  }
}
