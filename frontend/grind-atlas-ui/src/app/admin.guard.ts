import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './services/services';

export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  if (auth.isAdmin()) return true;
  inject(Router).navigate([auth.isLoggedIn() ? '/home' : '/']);
  return false;
};
