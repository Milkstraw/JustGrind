import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CoffeeService, GrinderService, EstimatorService } from '../../services/services';
import { Coffee, Grinder, EstimateResponse, BrewMethod, BREW_METHOD_LABELS } from '../../models/models';

@Component({
  selector: 'app-estimator',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h1 class="page-title">Estimator</h1>

    <!-- Input Panel -->
    <div class="panel" style="margin-bottom: 20px;">
      <div class="panel-head">
        <span class="panel-title">Estimate a Grind Setting</span>
      </div>
      <div class="panel-body">
        <div class="form-grid-3" style="margin-bottom: 16px;">
          <div class="form-group" style="margin-bottom: 0;">
            <label class="form-label">Coffee</label>
            <select [(ngModel)]="selectedCoffeeId" (ngModelChange)="onSelectionChange()">
              <option [ngValue]="undefined">Select coffee…</option>
              <option *ngFor="let c of coffees" [ngValue]="c.id">
                {{ c.name }} — {{ c.roaster }}
              </option>
            </select>
          </div>
          <div class="form-group" style="margin-bottom: 0;">
            <label class="form-label">Target Grinder</label>
            <select [(ngModel)]="selectedGrinderId" (ngModelChange)="onSelectionChange()">
              <option [ngValue]="undefined">Select grinder…</option>
              <option *ngFor="let g of grinders" [ngValue]="g.id">
                {{ g.brand }} {{ g.model }}
              </option>
            </select>
          </div>
          <div class="form-group" style="margin-bottom: 0;">
            <label class="form-label">Brew Method</label>
            <select [(ngModel)]="selectedBrewMethod" (ngModelChange)="onSelectionChange()">
              <option [ngValue]="undefined">Select method…</option>
              <option *ngFor="let m of brewMethods" [ngValue]="m.value">{{ m.label }}</option>
            </select>
          </div>
        </div>

        <div style="display: flex; gap: 10px;">
          <button class="btn btn-inv" (click)="runEstimate()" [disabled]="!canEstimate || loading">
            {{ loading ? 'Estimating...' : 'Get Estimate' }}
          </button>
          <button class="btn" (click)="reset()" *ngIf="result">Clear</button>
        </div>
      </div>
    </div>

    <!-- Coffee Preview -->
    <div class="panel" style="margin-bottom: 20px;" *ngIf="selectedCoffee">
      <div class="panel-head">
        <span class="panel-title">{{ selectedCoffee.name }}</span>
        <span class="status-pill">{{ selectedCoffee.processingMethod }}</span>
      </div>
      <div class="panel-body">
        <div class="form-grid-3">
          <div>
            <div class="stat-label">Origin</div>
            <div style="font-size:12px;">{{ selectedCoffee.originRegion ? selectedCoffee.originRegion + ', ' : '' }}{{ selectedCoffee.originCountry }}</div>
          </div>
          <div>
            <div class="stat-label">Variety</div>
            <div style="font-size:12px;">{{ selectedCoffee.variety || '—' }}</div>
          </div>
          <div>
            <div class="stat-label">Elevation</div>
            <div style="font-size:12px;">{{ selectedCoffee.elevationMasl ? selectedCoffee.elevationMasl + ' masl' : '—' }}</div>
          </div>
          <div>
            <div class="stat-label">Roast Level</div>
            <div style="font-size:12px;">{{ roastLabel(selectedCoffee.roastLevel) }} ({{ selectedCoffee.roastLevel }})</div>
          </div>
          <div style="grid-column: span 2;" *ngIf="getTastingNotes(selectedCoffee).length">
            <div class="stat-label">Tasting Notes</div>
            <div style="display:flex; flex-wrap:wrap; gap:6px; margin-top:4px;">
              <span class="status-pill s-hold" *ngFor="let note of getTastingNotes(selectedCoffee)">{{ note }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Result -->
    <div class="panel" *ngIf="result" style="margin-bottom: 20px;">
      <div class="panel-head">
        <span class="panel-title">Estimated Grind Setting</span>
        <span class="status-pill s-active">Layer {{ result.inferenceLayer }}</span>
      </div>
      <div class="panel-body">
        <div style="display: flex; align-items: baseline; gap: 32px; margin-bottom: 20px;">
          <div>
            <div class="stat-label">Native Setting</div>
            <div class="stat-num">{{ formatNativeSetting(result.estimatedNativeSetting) }}</div>
            <div style="font-size:10px; color:#666; margin-top:4px; letter-spacing:0.06em; text-transform:uppercase;">on grinder scale</div>
          </div>
          <div>
            <div class="stat-label">NGI Value</div>
            <div class="stat-num" style="font-size:20px;">{{ result.estimatedNgi }}</div>
            <div style="font-size:10px; color:#666; margin-top:4px; letter-spacing:0.06em; text-transform:uppercase;">/ 100 normalized</div>
          </div>
        </div>

        <div style="border-top: 1px solid var(--mid); padding-top: 16px;">
          <div style="margin-bottom: 14px;">
            <div class="stat-label" style="margin-bottom:6px;">Confidence — {{ (result.confidenceScore * 100).toFixed(0) }}%</div>
            <div class="bar-track" style="width: 240px;">
              <div class="bar-fill" [style.width.%]="result.confidenceScore * 100"></div>
            </div>
          </div>
          <div style="margin-bottom: 10px;">
            <span class="stat-label">Method: </span>
            <span style="font-size:12px;">{{ result.inferenceLayerLabel }}</span>
          </div>
          <div *ngIf="result.sourceLogCount > 0" style="margin-bottom: 10px;">
            <span class="stat-label">Based on: </span>
            <span style="font-size:12px;">{{ result.sourceLogCount }} log{{ result.sourceLogCount !== 1 ? 's' : '' }}
              <span *ngIf="result.avgSimilarityScore > 0"> (avg similarity {{ (result.avgSimilarityScore * 100).toFixed(0) }}%)</span>
            </span>
          </div>
          <div>
            <div class="stat-label">Explanation</div>
            <div style="font-size:12px; margin-top:4px;">{{ result.explanation }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Error -->
    <div class="error-msg" *ngIf="error">{{ error }}</div>
  `,
})
export class EstimatorComponent implements OnInit {
  private coffeeService    = inject(CoffeeService);
  private grinderService   = inject(GrinderService);
  private estimatorService = inject(EstimatorService);

  coffees: Coffee[]   = [];
  grinders: Grinder[] = [];
  brewMethods = Object.entries(BREW_METHOD_LABELS).map(([value, label]) => ({ value: value as BrewMethod, label }));

  selectedCoffeeId?: number;
  selectedGrinderId?: number;
  selectedBrewMethod?: BrewMethod;
  selectedCoffee?: Coffee;

  result?: EstimateResponse;
  loading = false;
  error?: string;

  get canEstimate() {
    return this.selectedCoffeeId && this.selectedGrinderId && this.selectedBrewMethod;
  }

  ngOnInit() {
    this.coffeeService.getAll().subscribe(c => this.coffees = c);
    this.grinderService.getAll().subscribe(g => this.grinders = g);
  }

  get selectedGrinder(): Grinder | undefined {
    return this.grinders.find(g => g.id === this.selectedGrinderId);
  }

  onSelectionChange() {
    this.result = undefined;
    this.error  = undefined;
    this.selectedCoffee = this.coffees.find(c => c.id === this.selectedCoffeeId);
  }

  formatNativeSetting(n: number): string {
    const g = this.selectedGrinder;
    if (!g) return String(n);
    if (g.scaleType === 'Alpha') return this.numToAlpha(Math.round(n));
    if (g.scaleType === 'AlphaNumeric') {
      const subDivs = g.scaleSubDivisions ?? 9;
      const fmt     = g.scaleFormat ?? 'A1';
      return this.numToAlphaNum(n, subDivs, fmt);
    }
    return String(n);
  }

  numToAlpha(n: number): string {
    n = Math.round(n);
    if (n <= 0) return 'A';
    let result = '';
    while (n > 0) {
      n--;
      result = String.fromCharCode(65 + (n % 26)) + result;
      n = Math.floor(n / 26);
    }
    return result;
  }

  numToAlphaNum(n: number, subDivs: number, format: string): string {
    n = Math.round(n * 10) / 10;
    const letterIdx = Math.floor((n - 1) / subDivs) + 1;
    const sub       = Math.round(((n - 1) % subDivs) + 1);
    const letter    = this.numToAlpha(letterIdx);
    return (format ?? 'A1') === '1A' ? `${sub}${letter}` : `${letter}${sub}`;
  }

  runEstimate() {
    if (!this.canEstimate) return;
    this.loading = true;
    this.error   = undefined;
    this.estimatorService.estimate({
      coffeeId:        this.selectedCoffeeId!,
      targetGrinderId: this.selectedGrinderId!,
      brewMethod:      this.selectedBrewMethod!,
    }).subscribe({
      next:  r => { this.result = r; this.loading = false; },
      error: e => { this.error = e.message ?? 'Estimation failed.'; this.loading = false; },
    });
  }

  reset() { this.result = undefined; this.error = undefined; }

  getTastingNotes(coffee: Coffee): string[] {
    return coffee.tastingNotes ? coffee.tastingNotes.split(',').map(n => n.trim()) : [];
  }

  roastLabel(level: number): string {
    if (level <= 1.5) return 'Light';
    if (level <= 2.5) return 'Light-Medium';
    if (level <= 3.5) return 'Medium';
    if (level <= 4.0) return 'Medium-Dark';
    return 'Dark';
  }
}
