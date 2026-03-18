import { Routes } from '@angular/router';
import { authGuard } from './auth.guard';
import { brewGuard } from './components/recipes/recipe-brew.component';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  {
    path: 'home',
    loadComponent: () =>
      import('./components/home/home.component').then(m => m.HomeComponent),
    canActivate: [authGuard],
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
    canActivate: [authGuard],
  },
  {
    path: 'coffees/add',
    loadComponent: () =>
      import('./components/coffees/add-coffee.component').then(m => m.AddCoffeeComponent),
    canActivate: [authGuard],
  },
  {
    path: 'grinders',
    loadComponent: () =>
      import('./components/grinders/grinders.component').then(m => m.GrindersComponent),
    canActivate: [authGuard],
  },
  {
    path: 'grinders/add',
    loadComponent: () =>
      import('./components/grinders/add-grinder.component').then(m => m.AddGrinderComponent),
    canActivate: [authGuard],
  },
  {
    path: 'logs',
    loadComponent: () =>
      import('./components/grind-logs/grind-logs.component').then(m => m.GrindLogsComponent),
    canActivate: [authGuard],
  },
  {
    path: 'recipes',
    loadComponent: () =>
      import('./components/recipes/recipes.component').then(m => m.RecipesComponent),
    canActivate: [authGuard],
  },
  {
    path: 'recipes/new',
    loadComponent: () =>
      import('./components/recipes/recipe-creator.component').then(m => m.RecipeCreatorComponent),
    canActivate: [authGuard],
  },
  {
    path: 'recipes/:id/edit',
    loadComponent: () =>
      import('./components/recipes/recipe-creator.component').then(m => m.RecipeCreatorComponent),
    canActivate: [authGuard],
  },
  {
    path: 'recipes/:id/brew',
    loadComponent: () =>
      import('./components/recipes/recipe-brew.component').then(m => m.RecipeBrewComponent),
    canActivate: [authGuard],
    canDeactivate: [brewGuard],
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
  { path: '**', redirectTo: 'login' },
];
