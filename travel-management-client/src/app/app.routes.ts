import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: 'login', loadComponent: () => import('./features/auth/login/login').then(m => m.Login) },
    { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard').then(m => m.Dashboard) },
    { path: 'requests', loadComponent: () => import('./features/travel-requests/request-list/request-list').then(m => m.RequestList) },
    { path: 'requests/new', loadComponent: () => import('./features/travel-requests/new-request/new-request').then(m => m.NewRequest) },
    { path: 'requests/:id', loadComponent: () => import('./features/travel-requests/request-detail/request-detail').then(m => m.RequestDetail) },
    { path: 'approvals', loadComponent: () => import('./features/approvals/approval-queue/approval-queue').then(m => m.ApprovalQueue) },
    { path: 'reports', loadComponent: () => import('./features/reports/reports').then(m => m.Reports) },
    { path: 'admin', loadComponent: () => import('./features/admin/admin').then(m => m.Admin) },
    { path: '**', redirectTo: 'login' },
];

