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

  readonly yAxisLabels = computed(() => {
    const list = this.analytics().monthlySpend;
    const max = list && list.length > 0 ? Math.max(...list.map(m => m.amount), 1) : 100000;
    const steps = [1, 0.8, 0.6, 0.4, 0.2, 0];
    const yPositions = [24, 64, 104, 144, 184, 224];
    return steps.map((ratio, idx) => ({
      y: yPositions[idx],
      label: this.formatCompactInr(Math.round(max * ratio))
    }));
  });

  private formatCompactInr(val: number): string {
    if (val === 0) return '₹0';
    if (val >= 100000) return `₹${(val / 100000).toFixed(1)}L`;
    if (val >= 1000) return `₹${(val / 1000).toFixed(0)}K`;
    return `₹${val}`;
  }

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
