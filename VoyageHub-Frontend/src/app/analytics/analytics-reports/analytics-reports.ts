import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { VoyageService, AnalyticsOverview } from '../../services/voyage.service';

interface DonutSegment {
  category: string;
  color: string;
  dashArray: string;
  dashOffset: number;
}

@Component({
  selector: 'app-analytics-reports',
  imports: [CommonModule, CurrencyPipe],
  templateUrl: './analytics-reports.html',
  styleUrl: './analytics-reports.css'
})
export class AnalyticsReports implements OnInit {
  private readonly voyageService = inject(VoyageService);

  readonly analytics = signal<AnalyticsOverview>({
    totalSpent: 0,
    vsLastPeriodPercent: 0,
    monthlySpend: [],
    departmentSpend: [],
    categorySpend: []
  });

  readonly chartPoints = computed(() => {
    const list = this.analytics().monthlySpend;
    if (!list || list.length === 0) {
      return { polyline: '', polygon: '', points: [] as Array<{ x: number; y: number; month: string; amount: number }> };
    }
    const max = Math.max(...list.map(m => m.amount), 1);
    const startX = 100;
    const availableWidth = 700;
    const stepX = list.length > 1 ? availableWidth / (list.length - 1) : 0;
    const points = list.map((m, i) => {
      const x = Math.round(startX + i * stepX);
      const y = Math.round(220 - (m.amount / max) * 160);
      return { x, y, month: m.month, amount: m.amount };
    });
    const polyline = points.map(p => `${p.x},${p.y}`).join(' ');
    const polygon = `${polyline} ${points[points.length - 1].x},220 ${points[0].x},220`;
    return { polyline, polygon, points };
  });

  readonly donutSegments = computed<DonutSegment[]>(() => {
    const cats = this.analytics().categorySpend;
    if (!cats || cats.length === 0) return [];
    const circumference = 314.159;
    let accumulatedOffset = 78.5; // Start at top
    return cats.map(c => {
      const segLength = (c.percentage / 100) * circumference;
      const gapLength = Math.max(0, circumference - segLength);
      const dashArray = `${segLength.toFixed(1)} ${gapLength.toFixed(1)}`;
      const dashOffset = accumulatedOffset;
      accumulatedOffset -= segLength;
      return {
        category: c.category,
        color: c.color,
        dashArray,
        dashOffset
      };
    });
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
