import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: 'login', loadComponent: () => import('./features/auth/login/login').then(m => m.Login)},
    { path: 'requests', loadComponent: () => import('./features/travel-requests/request-list/request-list').then(m => m.RequestList)},
    { path: '', redirectTo: 'login', pathMatch: 'full'},
    { path: '**', redirectTo: 'login' }
];
