import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-approval-queue',
  imports: [RouterLink, MatTableModule, MatChipsModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './approval-queue.html',
  styleUrl: './approval-queue.scss',
})
export class ApprovalQueue {
  displayedColumns = ['employee', 'destination', 'dates', 'amount', 'status', 'actions'];
  pendingApprovals = [
    { id: 101, employee: 'Neha Gupta', destination: 'Hyderabad', dates: '14 Nov – 16 Nov', amount: '₹18,500', status: 'Submitted' },
    { id: 102, employee: 'Rohan Mehta', destination: 'Singapore', dates: '22 Nov – 27 Nov', amount: '₹84,000', status: 'Manager Approved' },
    { id: 103, employee: 'Aarav Patel', destination: 'Chennai', dates: '02 Dec – 04 Dec', amount: '₹12,200', status: 'Submitted' },
  ];
}
