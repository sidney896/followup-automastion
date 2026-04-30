import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const roleGuard: CanActivateFn = (route) => {
  const router = inject(Router);
  const allowedRoles = (route.data['roles'] as string[]) ?? [];
  const validRoles = new Set(['advisor', 'manager', 'admin']);

  let role = 'manager';
  try {
    const stored = localStorage.getItem('prototypeRole');
    if (stored && validRoles.has(stored)) {
      role = stored;
    } else {
      localStorage.setItem('prototypeRole', role);
    }
  } catch {
    // Local storage can be unavailable in restrictive browser contexts.
    role = 'manager';
  }

  if (!allowedRoles.length || allowedRoles.includes(role)) {
    return true;
  }

  return router.createUrlTree(['/follow-up/automation']);
};
