import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-request-list',
  imports: [RouterLink, MatTableModule, MatChipsModule, MatButtonModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule,
    MatNativeDateModule, MatIconModule],
  templateUrl: './request-list.html',
  styleUrl: './request-list.scss',
})
export class RequestList {
  displayedColumns = ['destination', 'dates', 'status', 'submitted', 'actions'];
  requests = [
    { id: 1, destination: 'Mumbai', dates: '12 Nov – 15 Nov', status: 'Submitted', submitted: '05 Oct' },
    { id: 2, destination: 'Bangalore', dates: '20 Nov – 22 Nov', status: 'Draft', submitted: '—' },
    { id: 3, destination: 'Delhi', dates: '01 Dec – 03 Dec', status: 'Rejected', submitted: '28 Sep' },
  ];
}
