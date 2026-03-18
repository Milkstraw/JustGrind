import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CoffeeService } from '../../services/services';
import { Coffee, ProcessingMethod } from '../../models/models';

@Component({
  selector: 'app-coffees',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div style="display:flex; align-items:baseline; justify-content:space-between; margin-bottom:24px;">
      <h1 class="page-title" style="margin-bottom:0;">Coffees</h1>
      <a routerLink="/coffees/add" class="btn btn-inv">Add Coffee</a>
    </div>

    <!-- Filters -->
    <div class="panel" style="margin-bottom: 20px;" role="search" aria-label="Coffee filters">
      <div class="panel-head">
        <span class="panel-title" id="filters-heading">Filters</span>
        <button class="btn btn-sm" (click)="clearFilters()">Clear filters</button>
      </div>
      <div class="panel-body">
        <div class="form-grid-3" role="group" aria-labelledby="filters-heading">
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label" for="coffee-search">Search</label>
            <input id="coffee-search" type="text" [(ngModel)]="search" (ngModelChange)="applyFilters()" placeholder="Name or roaster…" aria-describedby="coffee-count">
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label" for="origin-filter">Origin Country</label>
            <input id="origin-filter" type="text" [(ngModel)]="originFilter" (ngModelChange)="applyFilters()" placeholder="e.g. Ethiopia">
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label" for="processing-filter">Processing</label>
            <select id="processing-filter" [(ngModel)]="processingFilter" (ngModelChange)="applyFilters()">
              <option value="">All</option>
              <option *ngFor="let p of processingMethods" [value]="p">{{ p }}</option>
            </select>
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label" for="min-roast">Min Roast</label>
            <input id="min-roast" type="number" [(ngModel)]="minRoast" (ngModelChange)="applyFilters()" min="1" max="5" step="0.5">
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label" for="max-roast">Max Roast</label>
            <input id="max-roast" type="number" [(ngModel)]="maxRoast" (ngModelChange)="applyFilters()" min="1" max="5" step="0.5">
          </div>
        </div>
      </div>
    </div>

    <!-- Count -->
    <div class="section-label" id="coffee-count" aria-live="polite" aria-atomic="true">{{ filteredCoffees.length }} coffees</div>

    <!-- Loading -->
    <div *ngIf="loading" style="font-size:12px; color:#666; padding:20px 0;" role="status" aria-live="polite">Loading coffees…</div>

    <!-- Table -->
    <div class="panel" *ngIf="!loading">
      <div style="overflow-x: auto;">
      <table class="table" style="min-width: 800px;" aria-label="Coffee catalog">
        <thead>
          <tr>
            <th scope="col">Name</th>
            <th scope="col">Roaster</th>
            <th scope="col">Origin</th>
            <th scope="col">Processing</th>
            <th scope="col">Roast</th>
            <th scope="col">Variety</th>
            <th scope="col">Notes</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let c of filteredCoffees">
            <td><strong>{{ c.name }}</strong></td>
            <td>{{ c.roaster }}</td>
            <td>{{ c.originRegion ? c.originRegion + ', ' : '' }}{{ c.originCountry }}</td>
            <td><span class="status-pill">{{ c.processingMethod }}</span></td>
            <td>{{ roastLabel(c.roastLevel) }} ({{ c.roastLevel }})</td>
            <td style="color:#666;">{{ c.variety || '—' }}</td>
            <td style="max-width:200px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; color:#666;">{{ c.tastingNotes || '—' }}</td>
          </tr>
          <tr *ngIf="filteredCoffees.length === 0">
            <td colspan="7" style="text-align:center; color:#999; padding:20px;">No coffees match the current filters.</td>
          </tr>
        </tbody>
      </table>
      </div>
    </div>
  `,
})
export class CoffeesComponent implements OnInit {
  private coffeeService = inject(CoffeeService);

  allCoffees:      Coffee[] = [];
  filteredCoffees: Coffee[] = [];
  loading = true;

  search           = '';
  originFilter     = '';
  processingFilter: ProcessingMethod | '' = '';
  minRoast?: number;
  maxRoast?: number;

  processingMethods: ProcessingMethod[] = ['Washed', 'Natural', 'Honey', 'Anaerobic', 'WetHulled', 'Other'];

  ngOnInit() {
    this.coffeeService.getAll().subscribe(c => {
      this.allCoffees = c;
      this.filteredCoffees = c;
      this.loading = false;
    });
  }

  applyFilters() {
    this.filteredCoffees = this.allCoffees.filter(c => {
      const matchSearch   = !this.search || c.name.toLowerCase().includes(this.search.toLowerCase()) ||
                            (c.roaster ?? '').toLowerCase().includes(this.search.toLowerCase());
      const matchOrigin   = !this.originFilter || (c.originCountry ?? '').toLowerCase().includes(this.originFilter.toLowerCase());
      const matchProcess  = !this.processingFilter || c.processingMethod === this.processingFilter;
      const matchMinRoast = this.minRoast == null || c.roastLevel >= this.minRoast;
      const matchMaxRoast = this.maxRoast == null || c.roastLevel <= this.maxRoast;
      return matchSearch && matchOrigin && matchProcess && matchMinRoast && matchMaxRoast;
    });
  }

  clearFilters() {
    this.search = ''; this.originFilter = ''; this.processingFilter = '';
    this.minRoast = undefined; this.maxRoast = undefined;
    this.filteredCoffees = this.allCoffees;
  }

  roastLabel(level: number): string {
    if (level <= 1.5) return 'Light';
    if (level <= 2.5) return 'Lt-Med';
    if (level <= 3.5) return 'Medium';
    if (level <= 4.0) return 'Med-Dark';
    return 'Dark';
  }
}
