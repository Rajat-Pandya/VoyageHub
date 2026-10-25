import { Component, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

export type UserRole = 'Employee' | 'Manager' | 'Finance' | 'Admin';

export interface TravelRequest {
  id: string;
  travelerName: string;
  travelerEmail: string;
  travelerAvatar: string;
  department: string;
  destination: string;
  destinationCode: string;
  startDate: string;
  endDate: string;
  purpose: string;
  estimatedCost: number;
  status: 'Draft' | 'Pending Manager' | 'Pending Finance' | 'Approved' | 'Rejected' | 'Settled';
  complianceScore: number;
  hotelNeeded: boolean;
  notes?: string;
  managerApproved?: boolean;
  financeApproved?: boolean;
}

export interface TravelPolicy {
  id: string;
  name: string;
  category: 'Flights' | 'Hotels' | 'Meals' | 'Ground';
  ruleDetail: string;
  maxLimit: string;
  enabled: boolean;
}

export interface AuditLog {
  id: string;
  timestamp: string;
  actor: string;
  action: string;
  target: string;
  status: 'SUCCESS' | 'WARNING' | 'ALERT';
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnInit {
  // Current active role & user
  currentRole = signal<UserRole>('Employee');
  currentUserName = signal<string>('Rajat Pandya');
  currentUserEmail = signal<string>('rajat.employee@corp-tms.com');
  currentDepartment = signal<string>('Engineering');

  // Active navigation tab
  activeTab = signal<string>('overview');

  // Search & Filters
  searchQuery = signal<string>('');
  statusFilter = signal<string>('ALL');

  // Notification Drawer
  showNotifications = signal<boolean>(false);
  notificationCount = signal<number>(3);

  // New Travel Request Modal (for Employee)
  showNewTripModal = signal<boolean>(false);
  newTripDestination = signal<string>('');
  newTripDestinationCode = signal<string>('LHR');
  newTripStartDate = signal<string>('2026-11-10');
  newTripEndDate = signal<string>('2026-11-15');
  newTripPurpose = signal<string>('Client Architecture Review');
  newTripCost = signal<number>(2400);
  newTripHotel = signal<boolean>(true);

  // View Details Modal
  selectedRequest = signal<TravelRequest | null>(null);

  // Action toast / feedback
  actionFeedback = signal<string | null>(null);

  // Travel Requests State
  requests = signal<TravelRequest[]>([
    {
      id: 'TR-9104',
      travelerName: 'Rajat Pandya',
      travelerEmail: 'rajat.employee@corp-tms.com',
      travelerAvatar: 'RP',
      department: 'Engineering',
      destination: 'London HQ, United Kingdom',
      destinationCode: 'LHR',
      startDate: '2026-11-10',
      endDate: '2026-11-15',
      purpose: 'Global Architecture & Cloud Sync',
      estimatedCost: 3250,
      status: 'Pending Manager',
      complianceScore: 98,
      hotelNeeded: true,
      notes: 'Standard hotel within corporate $220/night tier.'
    },
    {
      id: 'TR-8942',
      travelerName: 'Marcus Vance',
      travelerEmail: 'marcus.v@corp-tms.com',
      travelerAvatar: 'MV',
      department: 'Engineering',
      destination: 'San Francisco, USA',
      destinationCode: 'SFO',
      startDate: '2026-11-20',
      endDate: '2026-11-25',
      purpose: 'AWS Cloud Summit & Partner Keynote',
      estimatedCost: 4100,
      status: 'Pending Finance',
      complianceScore: 94,
      hotelNeeded: true,
      managerApproved: true,
      notes: 'Approved by Engineering Director. Awaiting Finance budget sign-off.'
    },
    {
      id: 'TR-8720',
      travelerName: 'Sophia Miller',
      travelerEmail: 'sophia.m@corp-tms.com',
      travelerAvatar: 'SM',
      department: 'Sales',
      destination: 'Singapore Global Hub',
      destinationCode: 'SIN',
      startDate: '2026-12-01',
      endDate: '2026-12-06',
      purpose: 'APAC Enterprise Contract Signing',
      estimatedCost: 5600,
      status: 'Approved',
      complianceScore: 100,
      hotelNeeded: true,
      managerApproved: true,
      financeApproved: true,
      notes: 'All compliance checks cleared. Itinerary ticketed.'
    },
    {
      id: 'TR-8501',
      travelerName: 'Alex Rivera',
      travelerEmail: 'alex.r@corp-tms.com',
      travelerAvatar: 'AR',
      department: 'Operations',
      destination: 'Frankfurt Tech Center',
      destinationCode: 'FRA',
      startDate: '2026-10-14',
      endDate: '2026-10-18',
      purpose: 'Data Center Migration Inspection',
      estimatedCost: 1980,
      status: 'Settled',
      complianceScore: 100,
      hotelNeeded: false,
      managerApproved: true,
      financeApproved: true
    },
    {
      id: 'TR-8329',
      travelerName: 'Rajat Pandya',
      travelerEmail: 'rajat.employee@corp-tms.com',
      travelerAvatar: 'RP',
      department: 'Engineering',
      destination: 'Bangalore Innovation Lab',
      destinationCode: 'BLR',
      startDate: '2026-12-15',
      endDate: '2026-12-19',
      purpose: 'Annual Developer Workshop',
      estimatedCost: 950,
      status: 'Draft',
      complianceScore: 92,
      hotelNeeded: true
    }
  ]);

  // Admin Policies
  policies = signal<TravelPolicy[]>([
    {
      id: 'POL-01',
      name: 'Economy Class flights on domestic routes',
      category: 'Flights',
      ruleDetail: 'Flight durations under 6 hours require Economy class booking.',
      maxLimit: 'Tier Economy',
      enabled: true
    },
    {
      id: 'POL-02',
      name: 'Maximum Hotel Nightly Cap',
      category: 'Hotels',
      ruleDetail: 'Tier 1 metros (London, NYC, SF) capped at $250/night. Others $180/night.',
      maxLimit: '$250/night',
      enabled: true
    },
    {
      id: 'POL-03',
      name: 'Daily Per-Diem Meal Allowance',
      category: 'Meals',
      ruleDetail: 'Covers breakfast, lunch and dinner during active business dates.',
      maxLimit: '$75/day',
      enabled: true
    },
    {
      id: 'POL-04',
      name: 'Dual Approval on Trips > $3,500',
      category: 'Flights',
      ruleDetail: 'Any travel estimate exceeding $3,500 triggers mandatory Finance Director review.',
      maxLimit: '$3,500.00',
      enabled: true
    }
  ]);

  // Admin System Audit Logs
  auditLogs = signal<AuditLog[]>([
    {
      id: 'LOG-309',
      timestamp: 'Today 13:42',
      actor: 'David Vance (Finance)',
      action: 'APPROVED_EXPENSE_CLAIM',
      target: 'TR-8501 (Alex Rivera, $1,980)',
      status: 'SUCCESS'
    },
    {
      id: 'LOG-308',
      timestamp: 'Today 11:15',
      actor: 'Sarah Jenkins (Manager)',
      action: 'FIRST_TIER_APPROVAL',
      target: 'TR-8942 (Marcus Vance, $4,100)',
      status: 'SUCCESS'
    },
    {
      id: 'LOG-307',
      timestamp: 'Today 09:20',
      actor: 'Rajat Pandya (Employee)',
      action: 'SUBMITTED_TRAVEL_REQUEST',
      target: 'TR-9104 (London HQ, $3,250)',
      status: 'SUCCESS'
    },
    {
      id: 'LOG-306',
      timestamp: 'Yesterday 17:05',
      actor: 'System Policy Guard',
      action: 'POLICY_EXCEPTION_FLAGGED',
      target: 'TR-7981 (Hotel exceeds standard per-diem)',
      status: 'WARNING'
    }
  ]);

  constructor(private router: Router) {}

  ngOnInit() {
    // Check if user came from login with a stored role
    if (typeof window !== 'undefined' && window.sessionStorage) {
      const stored = window.sessionStorage.getItem('tms_user');
      if (stored) {
        try {
          const userObj = JSON.parse(stored);
          if (userObj.role) {
            this.setRole(userObj.role as UserRole);
          }
          if (userObj.name) {
            this.currentUserName.set(userObj.name);
          }
          if (userObj.email) {
            this.currentUserEmail.set(userObj.email);
          }
        } catch (e) {
          console.error(e);
        }
      }
    }
  }

  // Switch role dynamically
  setRole(role: UserRole) {
    this.currentRole.set(role);
    this.activeTab.set('overview');
    
    // Set appropriate persona data
    if (role === 'Employee') {
      this.currentUserName.set('Rajat Pandya');
      this.currentUserEmail.set('rajat.employee@corp-tms.com');
      this.currentDepartment.set('Engineering');
    } else if (role === 'Manager') {
      this.currentUserName.set('Sarah Jenkins');
      this.currentUserEmail.set('sarah.manager@corp-tms.com');
      this.currentDepartment.set('Engineering Management');
    } else if (role === 'Finance') {
      this.currentUserName.set('David Vance');
      this.currentUserEmail.set('david.finance@corp-tms.com');
      this.currentDepartment.set('Finance & Treasury');
    } else if (role === 'Admin') {
      this.currentUserName.set('Elena Rostova');
      this.currentUserEmail.set('elena.admin@corp-tms.com');
      this.currentDepartment.set('Corporate Operations');
    }

    this.showToast(`Switched active portal view to [${role}]`);
  }

  // Switch active tab
  setActiveTab(tab: string) {
    this.activeTab.set(tab);
  }

  // Filtered requests based on search and status
  filteredRequests = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const filter = this.statusFilter();
    const role = this.currentRole();
    const all = this.requests();

    return all.filter(r => {
      // Role scope (Employee primarily sees own unless searching, Manager/Finance/Admin see organization)
      if (role === 'Employee' && this.activeTab() === 'my-trips') {
        if (!r.travelerEmail.includes('rajat')) return false;
      }

      // Status filter
      if (filter !== 'ALL' && r.status !== filter) {
        return false;
      }

      // Text search
      if (query) {
        const match = 
          r.id.toLowerCase().includes(query) ||
          r.travelerName.toLowerCase().includes(query) ||
          r.destination.toLowerCase().includes(query) ||
          r.purpose.toLowerCase().includes(query);
        if (!match) return false;
      }

      return true;
    });
  });

  // Role-Specific Metrics
  metrics = computed(() => {
    const role = this.currentRole();
    const all = this.requests();

    if (role === 'Employee') {
      const myTrips = all.filter(r => r.travelerEmail.includes('rajat'));
      return [
        { label: 'My Active Trips', value: '2 Scheduled', hint: 'Upcoming travel', icon: 'plane' },
        { label: 'Pending Approvals', value: '1 Request', hint: 'TR-9104 (London HQ)', icon: 'clock' },
        { label: 'Reimbursed Amount', value: '$1,450.00', hint: 'Cleared this quarter', icon: 'wallet' },
        { label: 'Policy Adherence', value: '98.5%', hint: 'Zero policy violations', icon: 'shield' }
      ];
    } else if (role === 'Manager') {
      const pendingMgr = all.filter(r => r.status === 'Pending Manager').length;
      return [
        { label: 'Awaiting My Approval', value: `${pendingMgr} Direct Reports`, hint: 'Action required', icon: 'clock' },
        { label: 'Team Active Travelers', value: '3 Employees', hint: 'Currently on-site', icon: 'users' },
        { label: 'Q4 Team Travel Spend', value: '$34,200', hint: 'Budget: $50,000', icon: 'wallet' },
        { label: 'Average SLA Response', value: '3.4 Hours', hint: 'Top 10% compliance', icon: 'shield' }
      ];
    } else if (role === 'Finance') {
      const pendingFin = all.filter(r => r.status === 'Pending Finance').length;
      return [
        { label: 'Pending 2nd Approvals', value: `${pendingFin} Claims`, hint: '$4,100 volume pending', icon: 'clock' },
        { label: 'Settled Reimbursements', value: '$142,500', hint: 'Year to date', icon: 'wallet' },
        { label: 'Budget Utilization', value: '68.4%', hint: 'Company-wide cap', icon: 'chart' },
        { label: 'Audit Exceptions', value: '1 Flagged', hint: 'Review per-diem rules', icon: 'shield' }
      ];
    } else {
      // Admin
      return [
        { label: 'Active TMS Users', value: '342 Employees', hint: '4 Departments configured', icon: 'users' },
        { label: 'Configured Policies', value: '4 Active Rules', hint: 'Strict per-diem limits', icon: 'shield' },
        { label: 'Approval Hierarchies', value: '14 Dept Nodes', hint: 'Two-tier state machine', icon: 'chart' },
        { label: 'Audit Events Logged', value: '1,824 Today', hint: '100% compliant trail', icon: 'clock' }
      ];
    }
  });

  // Manager: Approve Request
  approveByManager(req: TravelRequest) {
    this.requests.update(list => 
      list.map(item => {
        if (item.id === req.id) {
          return {
            ...item,
            status: 'Pending Finance',
            managerApproved: true,
            notes: 'Approved by Manager (Sarah Jenkins). Routed to Finance for budget sign-off.'
          };
        }
        return item;
      })
    );
    this.showToast(`Request ${req.id} approved! Auto-routed to Finance Approver.`);
  }

  // Manager/Finance: Reject Request
  rejectRequest(req: TravelRequest) {
    const reason = prompt('Please provide mandatory rejection feedback for compliance audit:', 'Budget cap exceeded for current quarter.');
    if (reason === null) return;

    this.requests.update(list => 
      list.map(item => {
        if (item.id === req.id) {
          return {
            ...item,
            status: 'Rejected',
            notes: `Rejected with reason: "${reason}"`
          };
        }
        return item;
      })
    );
    this.showToast(`Request ${req.id} returned to employee with rejection reason.`);
  }

  // Finance: Sign Off & Approve
  approveByFinance(req: TravelRequest) {
    this.requests.update(list => 
      list.map(item => {
        if (item.id === req.id) {
          return {
            ...item,
            status: 'Approved',
            financeApproved: true,
            notes: 'Final Finance Sign-off completed by David Vance. Authorized for itinerary issuance.'
          };
        }
        return item;
      })
    );
    this.showToast(`Final approval granted for ${req.id}! Reimbursement code issued.`);
  }

  // Employee: Create New Travel Request
  createNewRequest() {
    if (!this.newTripDestination().trim()) {
      alert('Please enter destination.');
      return;
    }

    const newReq: TravelRequest = {
      id: 'TR-' + Math.floor(1000 + Math.random() * 9000),
      travelerName: this.currentUserName(),
      travelerEmail: this.currentUserEmail(),
      travelerAvatar: 'RP',
      department: this.currentDepartment(),
      destination: this.newTripDestination(),
      destinationCode: this.newTripDestinationCode().toUpperCase(),
      startDate: this.newTripStartDate(),
      endDate: this.newTripEndDate(),
      purpose: this.newTripPurpose(),
      estimatedCost: Number(this.newTripCost()),
      status: 'Pending Manager',
      complianceScore: 99,
      hotelNeeded: this.newTripHotel(),
      notes: 'Submitted via Employee Portal. Routed to direct Line Manager.'
    };

    this.requests.update(list => [newReq, ...list]);
    this.showNewTripModal.set(false);
    this.showToast(`Travel Request ${newReq.id} submitted successfully!`);
    
    // Reset form
    this.newTripDestination.set('');
  }

  // Admin: Toggle Policy
  togglePolicy(policy: TravelPolicy) {
    this.policies.update(list =>
      list.map(p => p.id === policy.id ? { ...p, enabled: !p.enabled } : p)
    );
    this.showToast(`Policy ${policy.id} updated.`);
  }

  // View details modal
  openDetails(req: TravelRequest) {
    this.selectedRequest.set(req);
  }

  closeDetails() {
    this.selectedRequest.set(null);
  }

  // Toast feedback helper
  showToast(msg: string) {
    this.actionFeedback.set(msg);
    setTimeout(() => {
      this.actionFeedback.set(null);
    }, 3200);
  }

  // Sign out
  logout() {
    if (typeof window !== 'undefined' && window.sessionStorage) {
      window.sessionStorage.removeItem('tms_user');
    }
    this.router.navigate(['/login']);
  }
}
