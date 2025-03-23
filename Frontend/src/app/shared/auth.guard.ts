import { Inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = Inject(Router);
  const localData = localStorage.getItem('userToken');
  if (localData != null) {
    return true;
  } else {
    router.navigate('/login');
    return false;
  }
};