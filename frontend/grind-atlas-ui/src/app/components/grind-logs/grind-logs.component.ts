import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CoffeeService, GrinderService, GrindLogService } from '../../services/services';
import { Coffee, Grinder, GrindLog, BrewMethod, BREW_METHOD_LABELS, AddGrindLogRequest } from '../../models/models';

@Component({
  selector: 'app-grind-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div style="display:flex; align-items:baseline; justify-content:space-between; margin-bottom:24px;">
      <h1 class="page-title" style="margin-bottom:0;">Grind Logs</h1>
      <button class="btn" (click)="showForm = !showForm">
        {{ showForm ? 'Cancel' : 'Log a Grind' }}
      </button>
    </div>

    <!-- Add Log Form -->
    <div class="panel" *ngIf="showForm" style="margin-bottom: 20px;">
      <div class="panel-head">
        <span class="panel-title">New Grind Log</span>
      </div>
      <div class="panel-body">
        <div class="form-grid-3" style="margin-bottom:14px;">
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Coffee *</label>
            <select [(ngModel)]="newLog.coffeeId">
              <option [ngValue]="undefined">Select coffee…</option>
              <option *ngFor="let c of coffees" [ngValue]="c.id">{{ c.name }}</option>
            </select>
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Grinder *</label>
            <select [(ngModel)]="newLog.grinderId" (ngModelChange)="onGrinderChange()">
              <option [ngValue]="undefined">Select grinder…</option>
              <option *ngFor="let g of grinders" [ngValue]="g.id">{{ g.brand }} {{ g.model }}</option>
            </select>
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Brew Method *</label>
            <select [(ngModel)]="newLog.brewMethod">
              <option [ngValue]="undefined">Select method…</option>
              <option *ngFor="let m of brewMethods" [ngValue]="m.value">{{ m.label }}</option>
            </select>
          </div>
          <ng-container *ngIf="selectedGrinder; else numericFallback">
            <div class="form-group" style="margin-bottom:0;">
              <label class="form-label">Native Setting *</label>
              <div class="slider-wrap">
                <span class="slider-value">{{ sliderLabel }}</span>
                <input type="range"
                  [(ngModel)]="newLog.nativeSetting"
                  [min]="sliderMin" [max]="sliderMax" [step]="sliderStep">
                <div class="slider-bounds">
                  <span>{{ formatBound(sliderMin) }}</span>
                  <span>{{ formatBound(sliderMax) }}</span>
                </div>
              </div>
            </div>
          </ng-container>
          <ng-template #numericFallback>
            <div class="form-group" style="margin-bottom:0;">
              <label class="form-label">Native Setting *</label>
              <input type="number" [(ngModel)]="newLog.nativeSetting" step="0.5" placeholder="Select a grinder first">
            </div>
          </ng-template>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Dose (g)</label>
            <input type="number" [(ngModel)]="newLog.doseG" step="0.1" placeholder="e.g. 18">
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Yield (g)</label>
            <input type="number" [(ngModel)]="newLog.yieldG" step="0.1" placeholder="e.g. 36">
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Extraction Time (s)</label>
            <input type="number" [(ngModel)]="newLog.extractionTimeS" placeholder="e.g. 28">
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Rating (1–5)</label>
            <select [(ngModel)]="newLog.rating">
              <option [ngValue]="undefined">—</option>
              <option [ngValue]="1">1</option>
              <option [ngValue]="2">2</option>
              <option [ngValue]="3">3</option>
              <option [ngValue]="4">4</option>
              <option [ngValue]="5">5</option>
            </select>
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Notes</label>
            <input type="text" [(ngModel)]="newLog.notes" placeholder="Observations…">
          </div>
        </div>
        <button class="btn btn-inv" (click)="submitLog()" [disabled]="!canSubmit">Save Log</button>
      </div>
    </div>

    <!-- Logs table -->
    <div class="section-label">{{ logs.length }} log{{ logs.length !== 1 ? 's' : '' }} — click a row to see notes</div>
    <div class="panel">
      <div style="overflow-x: auto;">
        <table class="table" style="min-width: 800px;">
          <thead>
            <tr>
              <th></th>
              <th>Coffee</th>
              <th>Grinder</th>
              <th>Method</th>
              <th>Setting</th>
              <th>NGI</th>
              <th>Dose</th>
              <th>Rating</th>
              <th>Source</th>
            </tr>
          </thead>
          <tbody>
            <ng-container *ngFor="let l of logs">
              <tr (click)="toggleExpand(l.id)" style="cursor:pointer;">
                <td style="width:24px; padding:9px 8px;">
                  <span style="font-size:10px; color:#999;">{{ expandedLogId === l.id ? '▼' : '▶' }}</span>
                </td>
                <td><strong>{{ l.coffee?.name ?? 'Coffee #' + l.coffeeId }}</strong></td>
                <td>{{ l.grinder?.brand ?? '' }} {{ l.grinder?.model ?? '#' + l.grinderId }}</td>
                <td>{{ formatMethod(l.brewMethod) }}</td>
                <td><strong>{{ formatSetting(l) }}</strong></td>
                <td><span class="status-pill s-hold">{{ l.ngiNormalized }}</span></td>
                <td>{{ l.doseG ? l.doseG + 'g' : '—' }}</td>
                <td>{{ l.rating ?? '—' }}</td>
                <td>
                  <span *ngIf="l.recipeId" class="status-pill s-active" style="font-size:9px;">Recipe</span>
                  <span *ngIf="!l.recipeId" class="status-pill s-done" style="font-size:9px;">Quick Log</span>
                </td>
              </tr>
              <!-- Expanded notes panel -->
              <tr *ngIf="expandedLogId === l.id">
                <td colspan="9" style="background:var(--paper); padding:0;">
                  <div style="padding:14px 18px; border-left:4px solid var(--ink); font-size:13px;">
                    <div style="display:flex; gap:32px; flex-wrap:wrap; margin-bottom:10px;">
                      <div>
                        <span class="form-label">Brew Date</span>
                        <div>{{ l.brewDate ? formatDate(l.brewDate) : formatDate(l.createdAt) }}</div>
                      </div>
                      <div>
                        <span class="form-label">Source</span>
                        <div *ngIf="l.recipe"><strong>Recipe:</strong> {{ l.recipe.name }}</div>
                        <div *ngIf="!l.recipe">Quick Log</div>
                      </div>
                      <div *ngIf="l.yieldG">
                        <span class="form-label">Yield</span>
                        <div>{{ l.yieldG }}g</div>
                      </div>
                      <div *ngIf="l.extractionTimeS">
                        <span class="form-label">Extraction Time</span>
                        <div>{{ l.extractionTimeS }}s</div>
                      </div>
                    </div>
                    <div>
                      <span class="form-label">Notes</span>
                      <div style="margin-top:4px; color:{{ l.notes ? 'var(--ink)' : '#999' }};">
                        {{ l.notes || 'No notes recorded.' }}
                      </div>
                    </div>
                  </div>
                </td>
              </tr>
            </ng-container>
            <tr *ngIf="logs.length === 0">
              <td colspan="9" style="text-align:center; color:#999; padding:20px;">No logs yet.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
})
export class GrindLogsComponent implements OnInit {
  private coffeeService  = inject(CoffeeService);
  private grinderService = inject(GrinderService);
  private logService     = inject(GrindLogService);

  coffees: Coffee[]   = [];
  grinders: Grinder[] = [];
  logs: GrindLog[]    = [];

  showForm = false;
  expandedLogId: number | null = null;
  brewMethods = Object.entries(BREW_METHOD_LABELS).map(([value, label]) => ({ value: value as BrewMethod, label }));
  newLog: Partial<AddGrindLogRequest> = {};

  get canSubmit() {
    return this.newLog.coffeeId && this.newLog.grinderId && this.newLog.brewMethod &&
           this.newLog.nativeSetting != null;
  }

  get selectedGrinder(): Grinder | undefined {
    return this.grinders.find(g => g.id === this.newLog.grinderId);
  }

  get sliderMin(): number { return this.selectedGrinder?.scaleMin ?? 0; }
  get sliderMax(): number { return this.selectedGrinder?.scaleMax ?? 100; }

  get sliderStep(): number {
    const g = this.selectedGrinder;
    if (!g) return 1;
    if (g.scaleType === 'AlphaNumeric') {
      return (g.scaleSubType ?? 'Stepped') === 'Stepless' ? 0.5 : 1;
    }
    return g.grindType === 'Stepless' ? 0.5 : 1;
  }

  get sliderLabel(): string {
    const g = this.selectedGrinder;
    const v = this.newLog.nativeSetting;
    if (v == null || !g) return '—';
    if (g.scaleType === 'Alpha') return this.numToAlpha(Math.round(v));
    if (g.scaleType === 'AlphaNumeric') {
      return this.numToAlphaNum(v, g.scaleSubDivisions ?? 9, g.scaleFormat ?? 'A1');
    }
    return String(v);
  }

  onGrinderChange() {
    const g = this.selectedGrinder;
    this.newLog.nativeSetting = g ? g.scaleMin : undefined;
  }

  ngOnInit() {
    this.coffeeService.getAll().subscribe(c => this.coffees = c);
    this.grinderService.getAll().subscribe(g => this.grinders = g);
    this.logService.getAll().subscribe(l => this.logs = l);
  }

  toggleExpand(id: number) {
    this.expandedLogId = this.expandedLogId === id ? null : id;
  }

  submitLog() {
    if (!this.canSubmit) return;
    this.logService.create(this.newLog as AddGrindLogRequest).subscribe(log => {
      log.coffee  = this.coffees.find(c => c.id === log.coffeeId);
      log.grinder = this.grinders.find(g => g.id === log.grinderId);
      this.logs   = [log, ...this.logs];
      this.newLog = {};
      this.showForm = false;
    });
  }

  formatMethod(m: string): string {
    return BREW_METHOD_LABELS[m as BrewMethod] ?? m;
  }

  formatDate(d: string): string {
    return new Date(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
  }

  formatBound(v: number): string {
    const g = this.selectedGrinder;
    if (!g) return String(v);
    if (g.scaleType === 'Alpha') return this.numToAlpha(Math.round(v));
    if (g.scaleType === 'AlphaNumeric') {
      return this.numToAlphaNum(v, g.scaleSubDivisions ?? 9, g.scaleFormat ?? 'A1');
    }
    return String(v);
  }

  formatSetting(log: GrindLog): string {
    const g = this.grinders.find(gr => gr.id === log.grinderId);
    if (!g) return String(log.nativeSetting);
    if (g.scaleType === 'Alpha') return this.numToAlpha(Math.round(log.nativeSetting));
    if (g.scaleType === 'AlphaNumeric') {
      return this.numToAlphaNum(log.nativeSetting, g.scaleSubDivisions ?? 9, g.scaleFormat ?? 'A1');
    }
    return String(log.nativeSetting);
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
}
