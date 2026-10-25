import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, MatCardModule, MatTableModule, MatChipsModule, MatButtonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  displayedColumns = ['destination', 'dates', 'status', 'submitted'];
  recentRequests = [
    { destination: 'Mumbai', dates: '12 Nov – 15 Nov', status: 'Submitted', submitted: '05 Oct' },
    { destination: 'Bangalore', dates: '20 Nov – 22 Nov', status: 'Draft', submitted: '—' },
  ];
}
