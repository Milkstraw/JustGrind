import { Routes } from '@angular/router';
import { authGuard } from './auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  {
    path: 'home',
    loadComponent: () =>
      import('./components/home/home.component').then(m => m.HomeComponent),
  },
  {
    path: 'estimator',
    loadComponent: () =>
      import('./components/estimator/estimator.component').then(m => m.EstimatorComponent),
    canActivate: [authGuard],
  },
  {
    path: 'coffees',
    loadComponent: () =>
      import('./components/coffees/coffees.component').then(m => m.CoffeesComponent),
  },
  {
    path: 'coffees/add',
    loadComponent: () =>
      import('./components/coffees/add-coffee.component').then(m => m.AddCoffeeComponent),
  },
  {
    path: 'grinders',
    loadComponent: () =>
      import('./components/grinders/grinders.component').then(m => m.GrindersComponent),
  },
  {
    path: 'grinders/add',
    loadComponent: () =>
      import('./components/grinders/add-grinder.component').then(m => m.AddGrinderComponent),
  },
  {
    path: 'logs',
    loadComponent: () =>
      import('./components/grind-logs/grind-logs.component').then(m => m.GrindLogsComponent),
    canActivate: [authGuard],
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./components/auth/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./components/auth/register.component').then(m => m.RegisterComponent),
  },
  { path: '**', redirectTo: 'home' },
];
