import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-reports',
  imports: [MatCardModule, MatButtonModule, MatIconModule, MatTableModule],
  templateUrl: './reports.html',
  styleUrl: './reports.scss',
})
export class Reports {
  displayedColumns = ['department', 'budget', 'spent', 'variance'];
  spendData = [
    { department: 'Engineering', budget: '₹12,00,000', spent: '₹9,80,000', variance: '-18%' },
    { department: 'Sales & BD', budget: '₹25,00,000', spent: '₹23,40,000', variance: '-6%' },
    { department: 'Product & Design', budget: '₹8,00,000', spent: '₹5,10,000', variance: '-36%' },
    { department: 'Executive Leadership', budget: '₹15,00,000', spent: '₹14,20,000', variance: '-5%' },
  ];
}
