import { Routes } from '@angular/router';

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
  },
  { path: '**', redirectTo: 'home' },
];
