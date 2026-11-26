import { Routes } from '@angular/router';
import { Login } from './login/login';
import { ResetPassword } from './reset-password/reset-password';
import { AppLayout } from './layout/layout';
import { Dashboard } from './dashboard/dashboard';
import { RequestList } from './requests/request-list/request-list';
import { NewRequest } from './requests/new-request/new-request';
import { RequestDetail } from './requests/request-detail/request-detail';
import { MyReimbursements } from './requests/my-reimbursements/my-reimbursements';
import { ApprovalQueue } from './approvals/approval-queue/approval-queue';
import { ExpenseReview } from './finance/expense-review/expense-review';
import { AnalyticsReports } from './analytics/analytics-reports/analytics-reports';
import { PolicyConfigComponent } from './admin/policy-config/policy-config';
import { AuditLogs } from './admin/audit-logs/audit-logs';
import { EmployeeManagement } from './admin/employee-management/employee-management';
import { Settings } from './settings/settings';
import { Support } from './support/support';
import { roleGuard } from './guards/role.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'forgot-password', component: Login, data: { mode: 'forgot' } },
  { path: 'reset-password', component: ResetPassword },
  {
    path: '',
    component: AppLayout,
    children: [
      { path: 'dashboard', component: Dashboard, canActivate: [roleGuard(['Employee', 'Manager', 'Finance', 'Admin'])] },
      { path: 'requests', component: RequestList, canActivate: [roleGuard(['Employee', 'Manager', 'Admin'])] },
      { path: 'requests/new', component: NewRequest, canActivate: [roleGuard(['Employee', 'Manager'])] },
      { path: 'requests/:id', component: RequestDetail, canActivate: [roleGuard(['Employee', 'Manager', 'Finance', 'Admin'])] },
      { path: 'reimbursements', component: MyReimbursements, canActivate: [roleGuard(['Employee', 'Manager', 'Finance', 'Admin'])] },
      { path: 'approvals', component: ApprovalQueue, canActivate: [roleGuard(['Manager', 'Finance', 'Admin'])] },
      { path: 'expenses', component: ExpenseReview, canActivate: [roleGuard(['Finance', 'Admin'])] },
      { path: 'analytics', component: AnalyticsReports, canActivate: [roleGuard(['Manager', 'Finance', 'Admin'])] },
      { path: 'policies', component: PolicyConfigComponent, canActivate: [roleGuard(['Admin'])] },
      { path: 'audit', component: AuditLogs, canActivate: [roleGuard(['Admin'])] },
      { path: 'admin/employees', component: EmployeeManagement, canActivate: [roleGuard(['Admin'])] },
      { path: 'employees', component: EmployeeManagement, canActivate: [roleGuard(['Admin'])] },
      { path: 'settings', component: Settings, canActivate: [roleGuard(['Employee', 'Manager', 'Finance', 'Admin'])] },
      { path: 'support', component: Support, canActivate: [roleGuard(['Employee', 'Manager', 'Finance', 'Admin'])] }
    ]
  },
  { path: '**', redirectTo: 'login' }
];
