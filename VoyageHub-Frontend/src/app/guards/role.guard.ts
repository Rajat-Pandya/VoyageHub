import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: (allowedRoles: string[]) => CanActivateFn = (allowedRoles: string[]) => {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const user = authService.currentUser();
    if (!user) {
      void router.navigate(['/login']);
      return false;
    }

    const currentRole = user.role || 'Employee';
    if (allowedRoles.includes(currentRole)) {
      return true;
    }

    // Unauthorized for this specific route -> redirect to role's home view
    const defaultRoute = authService.getRoleDefaultRoute(currentRole);
    void router.navigate([defaultRoute]);
    return false;
  };
};
