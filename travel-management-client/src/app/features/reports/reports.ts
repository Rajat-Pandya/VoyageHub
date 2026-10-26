import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';

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
    MatIconModule
  ],
  templateUrl: './reports.html',
  styleUrl: './reports.scss',
})
export class Reports {
  private fb = new FormBuilder();

  filters = this.fb.group({
    department: ['all'],
    fromDate: [null as Date | null],
    toDate: [null as Date | null]
  });

  departmentColumns = ['department', 'totalSpend', 'requestCount', 'avgPerTrip'];
  // MOCK DATA — replaced by GET /api/reports/department-spend once the API exists
  departmentSpend = [
    { department: 'Sales', totalSpend: '₹2,45,000', requestCount: 18, avgPerTrip: '₹13,600' },
    { department: 'Engineering', totalSpend: '₹1,12,000', requestCount: 9, avgPerTrip: '₹12,400' },
    { department: 'Marketing', totalSpend: '₹78,500', requestCount: 6, avgPerTrip: '₹13,080' },
  ];

  employeeSearch = '';
  employeeHistoryColumns = ['destination', 'dates', 'status', 'amount'];
  // MOCK DATA — replaced by GET /api/reports/employee-history?employeeId=... later
  employeeHistory = [
    { destination: 'Mumbai', dates: '12 Nov – 15 Nov', status: 'Closed', amount: '₹7,200' },
    { destination: 'Delhi', dates: '01 Sep – 03 Sep', status: 'Closed', amount: '₹5,400' },
  ];
}
