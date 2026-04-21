import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CoffeeService, GrinderService, GrindLogService, RecipeService } from '../../services/services';
import { logError } from '../../utils/logger';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <h1 class="page-title">Grind Atlas</h1>

    <div class="section-label">Overview</div>
    <div class="stats-row" style="margin-bottom: 28px;">
      <div class="stat-card">
        <div class="stat-label">Coffees</div>
        <div class="stat-num">{{ statsLoading ? '…' : (coffeeCount ?? '—') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Grinders</div>
        <div class="stat-num">{{ statsLoading ? '…' : (grinderCount ?? '—') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Grind Logs</div>
        <div class="stat-num">{{ statsLoading ? '…' : (logCount ?? '—') }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Recipes</div>
        <div class="stat-num">{{ statsLoading ? '…' : (recipeCount ?? '—') }}</div>
      </div>
    </div>

    <div class="section-label">Navigation</div>
    <div class="grid-2">
      <div class="panel">
        <div class="panel-head">
          <span class="panel-title">Coffees</span>
        </div>
        <div class="panel-body">
          <p style="font-size:12px; margin: 0 0 14px; color:#555;">Browse the coffee catalog and add new coffees to the database.</p>
          <div style="display:flex; gap:10px;">
            <a routerLink="/coffees" class="btn btn-inv btn-sm">Browse →</a>
            <a routerLink="/coffees/add" class="btn btn-sm">Add New</a>
          </div>
        </div>
      </div>

      <div class="panel">
        <div class="panel-head">
          <span class="panel-title">Grinders</span>
        </div>
        <div class="panel-body">
          <p style="font-size:12px; margin: 0 0 14px; color:#555;">Browse the grinder catalog and add new grinders with NGI calibrations.</p>
          <div style="display:flex; gap:10px;">
            <a routerLink="/grinders" class="btn btn-inv btn-sm">Browse →</a>
            <a routerLink="/grinders/add" class="btn btn-sm">Add New</a>
          </div>
        </div>
      </div>

      <div class="panel">
        <div class="panel-head">
          <span class="panel-title">Grind Advisor</span>
        </div>
        <div class="panel-body">
          <p style="font-size:12px; margin: 0 0 14px; color:#555;">Get a recommended grind setting for any coffee and grinder combination.</p>
          <div style="display:flex; gap:10px;">
            <a routerLink="/grind-advisor" class="btn btn-inv btn-sm">Open →</a>
          </div>
        </div>
      </div>

      <div class="panel">
        <div class="panel-head">
          <span class="panel-title">Grind Logs</span>
        </div>
        <div class="panel-body">
          <p style="font-size:12px; margin: 0 0 14px; color:#555;">Record your brew sessions to improve Grind Advisor accuracy over time.</p>
          <div style="display:flex; gap:10px;">
            <a routerLink="/logs" class="btn btn-inv btn-sm">View →</a>
          </div>
        </div>
      </div>

      <div class="panel">
        <div class="panel-head">
          <span class="panel-title">Brew Recipes</span>
        </div>
        <div class="panel-body">
          <p style="font-size:12px; margin: 0 0 14px; color:#555;">Build step-by-step brew recipes with a guided timer and session logging.</p>
          <div style="display:flex; gap:10px;">
            <a routerLink="/recipes" class="btn btn-inv btn-sm">View →</a>
            <a routerLink="/recipes/new" class="btn btn-sm">New Recipe</a>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class HomeComponent implements OnInit {
  private coffeeService  = inject(CoffeeService);
  private grinderService = inject(GrinderService);
  private logService     = inject(GrindLogService);
  private recipeService  = inject(RecipeService);

  coffeeCount?: number;
  grinderCount?: number;
  logCount?: number;
  recipeCount?: number;
  statsLoading = true;

  ngOnInit() {
    forkJoin({
      coffees:  this.coffeeService.getAll(),
      grinders: this.grinderService.getAll(),
      logs:     this.logService.getAll(),
      recipes:  this.recipeService.getAll(),
    }).subscribe({
      next: ({ coffees, grinders, logs, recipes }) => {
        this.coffeeCount  = coffees.length;
        this.grinderCount = grinders.length;
        this.logCount     = logs.length;
        this.recipeCount  = recipes.length;
        this.statsLoading = false;
      },
      error: (err) => {
        logError('HomeComponent.ngOnInit', err, {
          url: 'forkJoin(coffees,grinders,logs,recipes)',
          status: err?.status,
          body: err?.error,
        });
        this.statsLoading = false;
      },
    });
  }
}
