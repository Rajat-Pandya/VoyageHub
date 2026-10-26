import { Component, OnInit, inject, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';

export interface PolicyTier {
  id: number;
  level: string;
  flightClass: string;
  hotelLimit: string;
  perDiem: string;
  autoApprovalLimit?: string;
}

export interface SystemStatus {
  environment: string;
  framework: string;
  database: string;
  databaseName: string;
  totalRequests: number;
  activePolicies: number;
  lastRequestTime: string;
  serverStatus: string;
}

@Component({
  selector: 'app-admin',
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatTableModule],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
})
export class Admin implements OnInit {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);

  private readonly apiBase = 'http://localhost:5201/api/Admin';

  policyColumns = ['level', 'flightClass', 'hotelLimit', 'perDiem', 'autoApprovalLimit', 'actions'];
  readonly policies = signal<PolicyTier[]>([]);
  readonly systemStatus = signal<SystemStatus | null>(null);

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadPolicies();
      this.loadSystemStatus();
    }
  }

  loadPolicies() {
    this.http.get<PolicyTier[]>(`${this.apiBase}/policies`).subscribe({
      next: (data) => this.policies.set(data),
      error: () => {
        this.policies.set([
          {
            id: 1,
            level: 'Executive (L6+)',
            flightClass: 'Business (International) / Economy (Domestic)',
            hotelLimit: '₹12,000 / night',
            perDiem: '₹3,500 / day',
            autoApprovalLimit: '₹1,50,000',
          },
          {
            id: 2,
            level: 'Senior Staff (L4-L5)',
            flightClass: 'Economy Plus',
            hotelLimit: '₹8,000 / night',
            perDiem: '₹2,500 / day',
            autoApprovalLimit: '₹50,000',
          },
          {
            id: 3,
            level: 'Staff & Associate (L1-L3)',
            flightClass: 'Standard Economy',
            hotelLimit: '₹5,500 / night',
            perDiem: '₹1,800 / day',
            autoApprovalLimit: '₹25,000',
          },
        ]);
      },
    });
  }

  loadSystemStatus() {
    this.http.get<SystemStatus>(`${this.apiBase}/system-status`).subscribe({
      next: (data) => this.systemStatus.set(data),
      error: () => {
        this.systemStatus.set({
          environment: 'Local / Development',
          framework: '.NET 8.0 ASP.NET Core',
          database: 'localhost\\SQLEXPRESS06',
          databaseName: 'TravelDB',
          totalRequests: 4,
          activePolicies: 3,
          lastRequestTime: 'Active',
          serverStatus: 'Online',
        });
      },
    });
  }
}
