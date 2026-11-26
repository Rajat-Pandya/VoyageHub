import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ExpenseItem, VoyageService } from '../../services/voyage.service';

@Component({
  selector: 'app-my-reimbursements',
  imports: [CommonModule, CurrencyPipe, DatePipe, RouterLink, FormsModule],
  templateUrl: './my-reimbursements.html',
  styleUrl: './my-reimbursements.css'
})
export class MyReimbursements implements OnInit {
  private readonly voyageService = inject(VoyageService);

  readonly expenses = signal<ExpenseItem[]>([]);
  readonly isLoading = signal(true);
  readonly statusFilter = signal<'All' | 'Pending' | 'Reimbursed' | 'Rejected'>('All');

  readonly filteredExpenses = computed(() => {
    const filter = this.statusFilter();
    const list = this.expenses();
    if (filter === 'All') return list;
    return list.filter((e) => e.status === filter);
  });

  readonly totalPending = computed(() =>
    this.expenses().filter((e) => e.status === 'Pending').reduce((s, e) => s + (e.amount || 0), 0)
  );

  readonly totalReimbursed = computed(() =>
    this.expenses().filter((e) => e.status === 'Reimbursed').reduce((s, e) => s + (e.amount || 0), 0)
  );

  ngOnInit(): void {
    this.isLoading.set(true);
    this.voyageService.getMyExpenses().subscribe({
      next: (list) => {
        this.expenses.set(list || []);
        this.isLoading.set(false);
      },
      error: () => {
        this.expenses.set([]);
        this.isLoading.set(false);
      }
    });
  }
}
