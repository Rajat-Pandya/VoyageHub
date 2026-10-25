import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-admin',
  imports: [MatCardModule, MatButtonModule, MatIconModule, MatTableModule],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
})
export class Admin {
  policyColumns = ['level', 'flightClass', 'hotelLimit', 'perDiem', 'actions'];
  policies = [
    { level: 'Executive (L6+)', flightClass: 'Business (International) / Economy (Domestic)', hotelLimit: '₹12,000 / night', perDiem: '₹3,500 / day' },
    { level: 'Senior Staff (L4-L5)', flightClass: 'Economy Plus', hotelLimit: '₹8,000 / night', perDiem: '₹2,500 / day' },
    { level: 'Staff & Associate (L1-L3)', flightClass: 'Standard Economy', hotelLimit: '₹5,500 / night', perDiem: '₹1,800 / day' },
  ];
}
