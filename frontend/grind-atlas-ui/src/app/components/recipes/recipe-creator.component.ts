import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CoffeeService, GrinderService, RecipeService } from '../../services/services';
import {
  Coffee, Grinder, BrewMethod, BREW_METHOD_LABELS,
  CreateBrewRecipeRequest, BrewRecipe
} from '../../models/models';

interface StepForm {
  instruction: string;
  durationDisplay: string; // "MM:SS" displayed in input
  durationS: number;       // actual seconds
  pourWaterG: number | undefined;
}

@Component({
  selector: 'app-recipe-creator',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div style="display:flex; align-items:baseline; justify-content:space-between; margin-bottom:24px;">
      <h1 class="page-title" style="margin-bottom:0;">{{ isEditMode ? 'Edit Recipe' : 'New Recipe' }}</h1>
      <a routerLink="/recipes" class="btn">Cancel</a>
    </div>

    <!-- Recipe header fields -->
    <div class="panel" style="margin-bottom:20px;">
      <div class="panel-head"><span class="panel-title">Recipe Details</span></div>
      <div class="panel-body">
        <div class="form-group">
          <label class="form-label">Recipe Name *</label>
          <input type="text" [(ngModel)]="name" placeholder="e.g. Morning V60 Light Roast" maxlength="100">
        </div>
        <div class="form-grid-3" style="margin-bottom:14px;">
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Coffee *</label>
            <select [(ngModel)]="coffeeId">
              <option [ngValue]="undefined">Select coffee…</option>
              <option *ngFor="let c of coffees" [ngValue]="c.id">{{ c.name }}</option>
            </select>
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Grinder *</label>
            <select [(ngModel)]="grinderId" (ngModelChange)="onGrinderChange()">
              <option [ngValue]="undefined">Select grinder…</option>
              <option *ngFor="let g of grinders" [ngValue]="g.id">{{ g.brand }} {{ g.model }}</option>
            </select>
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Brew Method *</label>
            <select [(ngModel)]="brewMethod">
              <option [ngValue]="undefined">Select method…</option>
              <option *ngFor="let m of brewMethods" [ngValue]="m.value">{{ m.label }}</option>
            </select>
          </div>
        </div>

        <!-- Grind setting -->
        <div class="form-grid-3" style="margin-bottom:14px;">
          <ng-container *ngIf="selectedGrinder; else numericFallback">
            <div class="form-group" style="margin-bottom:0;">
              <label class="form-label">Grind Setting</label>
              <div class="slider-wrap">
                <span class="slider-value">{{ sliderLabel }}</span>
                <input type="range" [(ngModel)]="nativeSetting"
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
              <label class="form-label">Grind Setting</label>
              <input type="number" [(ngModel)]="nativeSetting" step="0.5" placeholder="Select a grinder first">
            </div>
          </ng-template>

          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Dose (g)</label>
            <input type="number" [(ngModel)]="doseG" step="0.1" placeholder="e.g. 15">
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Water (g)</label>
            <input type="number" [(ngModel)]="waterG" step="1" placeholder="e.g. 250">
          </div>
          <div class="form-group" style="margin-bottom:0;">
            <label class="form-label">Water Temp (°C)</label>
            <input type="number" [(ngModel)]="waterTempC" step="0.5" placeholder="e.g. 93">
          </div>
        </div>

        <div class="form-group" style="margin-bottom:0;">
          <label class="form-label">Technique Notes</label>
          <textarea [(ngModel)]="techniqueNotes" rows="2" placeholder="Pouring technique, agitation, etc."></textarea>
        </div>
      </div>
    </div>

    <!-- Steps section -->
    <div class="panel" style="margin-bottom:20px;">
      <div class="panel-head">
        <span class="panel-title">Steps</span>
        <div style="display:flex; align-items:center; gap:16px;">
          <span style="font-size:12px; color:#666;" *ngIf="steps.length > 0">
            Total: {{ formatTotalTime() }}
          </span>
          <button class="btn btn-sm" (click)="addStep()">+ Add Step</button>
        </div>
      </div>
      <div class="panel-body">
        <div *ngIf="steps.length === 0" style="text-align:center; color:#999; padding:24px 0; font-size:13px;">
          No steps yet — add at least one step to save this recipe.
        </div>

        <div *ngFor="let step of steps; let i = index"
          style="border-bottom: 1px solid var(--mid); padding:14px 0;"
          [style.border-bottom]="i === steps.length - 1 ? 'none' : '1px solid var(--mid)'">
          <div style="display:flex; align-items:flex-start; gap:12px;">
            <!-- Step number -->
            <div style="
              min-width:28px; height:28px; border: var(--b-thin);
              display:flex; align-items:center; justify-content:center;
              font-weight:700; font-size:12px; flex-shrink:0; margin-top:2px;">
              {{ i + 1 }}
            </div>

            <!-- Fields -->
            <div style="flex:1; display:grid; grid-template-columns:1fr auto auto; gap:12px; align-items:start;">
              <div class="form-group" style="margin-bottom:0;">
                <label class="form-label">Instruction</label>
                <input type="text" [(ngModel)]="step.instruction"
                  placeholder="e.g. Bloom — pour 50g water">
              </div>
              <div class="form-group" style="margin-bottom:0; min-width:100px;">
                <label class="form-label">Duration (MM:SS)</label>
                <input type="text" [ngModel]="step.durationDisplay"
                  (ngModelChange)="step.durationDisplay = $event"
                  (blur)="parseDuration(step)"
                  placeholder="0:30"
                  style="font-variant-numeric: tabular-nums;">
              </div>
              <div class="form-group" style="margin-bottom:0; min-width:90px;">
                <label class="form-label">Pour (g)</label>
                <input type="number" [(ngModel)]="step.pourWaterG" step="1" placeholder="—">
              </div>
            </div>

            <!-- Reorder + remove -->
            <div style="display:flex; flex-direction:column; gap:4px; flex-shrink:0; margin-top:18px;">
              <button class="btn btn-sm" (click)="moveStep(i, -1)" [disabled]="i === 0"
                style="padding:3px 8px; font-size:10px;">↑</button>
              <button class="btn btn-sm" (click)="moveStep(i, 1)" [disabled]="i === steps.length - 1"
                style="padding:3px 8px; font-size:10px;">↓</button>
              <button class="btn btn-sm" (click)="removeStep(i)"
                style="padding:3px 8px; font-size:10px;">✕</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Error -->
    <div class="error-msg" *ngIf="error" style="margin-bottom:16px;">{{ error }}</div>

    <!-- Save -->
    <button class="btn btn-inv" (click)="save()" [disabled]="!canSave">
      {{ isEditMode ? 'Save Changes' : 'Create Recipe' }}
    </button>
  `,
})
export class RecipeCreatorComponent implements OnInit {
  private coffeeService  = inject(CoffeeService);
  private grinderService = inject(GrinderService);
  private recipeService  = inject(RecipeService);
  private route          = inject(ActivatedRoute);
  private router         = inject(Router);

  coffees: Coffee[]   = [];
  grinders: Grinder[] = [];
  brewMethods = Object.entries(BREW_METHOD_LABELS).map(([value, label]) => ({ value: value as BrewMethod, label }));

  // form fields
  name        = '';
  coffeeId?: number;
  grinderId?: number;
  brewMethod?: BrewMethod;
  nativeSetting?: number;
  doseG?: number;
  waterG?: number;
  waterTempC?: number;
  techniqueNotes = '';
  steps: StepForm[] = [];

  isEditMode = false;
  editId?: number;
  error = '';

  get canSave(): boolean {
    return !!this.name.trim() && !!this.coffeeId && !!this.grinderId && !!this.brewMethod &&
           this.steps.length > 0 && this.steps.every(s => !!s.instruction.trim() && s.durationS > 0);
  }

  get selectedGrinder(): Grinder | undefined {
    return this.grinders.find(g => g.id === this.grinderId);
  }

  get sliderMin(): number { return this.selectedGrinder?.scaleMin ?? 0; }
  get sliderMax(): number { return this.selectedGrinder?.scaleMax ?? 100; }

  get sliderStep(): number {
    const g = this.selectedGrinder;
    if (!g) return 1;
    if (g.scaleType === 'AlphaNumeric') return (g.scaleSubType ?? 'Stepped') === 'Stepless' ? 0.5 : 1;
    return g.grindType === 'Stepless' ? 0.5 : 1;
  }

  get sliderLabel(): string {
    const g = this.selectedGrinder;
    const v = this.nativeSetting;
    if (v == null || !g) return '—';
    if (g.scaleType === 'Alpha') return this.numToAlpha(Math.round(v));
    if (g.scaleType === 'AlphaNumeric') return this.numToAlphaNum(v, g.scaleSubDivisions ?? 9, g.scaleFormat ?? 'A1');
    return String(v);
  }

  formatBound(v: number): string {
    const g = this.selectedGrinder;
    if (!g) return String(v);
    if (g.scaleType === 'Alpha') return this.numToAlpha(Math.round(v));
    if (g.scaleType === 'AlphaNumeric') return this.numToAlphaNum(v, g.scaleSubDivisions ?? 9, g.scaleFormat ?? 'A1');
    return String(v);
  }

  onGrinderChange() {
    const g = this.selectedGrinder;
    this.nativeSetting = g ? g.scaleMin : undefined;
  }

  ngOnInit() {
    this.coffeeService.getAll().subscribe(c => this.coffees = c);
    this.grinderService.getAll().subscribe(g => this.grinders = g);

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.editId = +id;
      this.recipeService.getById(this.editId).subscribe(r => this.loadRecipe(r));
    }
  }

  loadRecipe(r: BrewRecipe) {
    this.name          = r.name;
    this.coffeeId      = r.coffeeId;
    this.grinderId     = r.grinderId;
    this.brewMethod    = r.brewMethod;
    this.nativeSetting = r.nativeSetting;
    this.doseG         = r.doseG;
    this.waterG        = r.waterG;
    this.waterTempC    = r.waterTempC;
    this.techniqueNotes = r.techniqueNotes ?? '';
    this.steps = r.steps.map(s => ({
      instruction:     s.instruction,
      durationS:       s.durationS,
      durationDisplay: this.formatDuration(s.durationS),
      pourWaterG:      s.pourWaterG,
    }));
  }

  addStep() {
    this.steps.push({ instruction: '', durationS: 0, durationDisplay: '0:30', pourWaterG: undefined });
    // Parse the default display value
    this.parseDuration(this.steps[this.steps.length - 1]);
  }

  removeStep(i: number) {
    this.steps.splice(i, 1);
  }

  moveStep(i: number, dir: -1 | 1) {
    const j = i + dir;
    if (j < 0 || j >= this.steps.length) return;
    [this.steps[i], this.steps[j]] = [this.steps[j], this.steps[i]];
  }

  parseDuration(step: StepForm) {
    const raw = step.durationDisplay.trim();
    const parts = raw.split(':');
    let seconds = 0;
    if (parts.length === 2) {
      seconds = (parseInt(parts[0], 10) || 0) * 60 + (parseInt(parts[1], 10) || 0);
    } else {
      seconds = parseInt(raw, 10) || 0;
    }
    step.durationS = Math.max(0, seconds);
    step.durationDisplay = this.formatDuration(step.durationS);
  }

  formatDuration(s: number): string {
    const m = Math.floor(s / 60);
    const sec = s % 60;
    return `${m}:${sec.toString().padStart(2, '0')}`;
  }

  formatTotalTime(): string {
    const total = this.steps.reduce((sum, s) => sum + s.durationS, 0);
    const m = Math.floor(total / 60);
    const s = total % 60;
    if (m > 0 && s > 0) return `${m}m ${s}s`;
    if (m > 0) return `${m}m`;
    return `${s}s`;
  }

  save() {
    if (!this.canSave) return;
    this.error = '';

    const req: CreateBrewRecipeRequest = {
      name:           this.name.trim(),
      coffeeId:       this.coffeeId!,
      grinderId:      this.grinderId!,
      brewMethod:     this.brewMethod!,
      nativeSetting:  this.nativeSetting,
      doseG:          this.doseG,
      waterG:         this.waterG,
      waterTempC:     this.waterTempC,
      techniqueNotes: this.techniqueNotes.trim() || undefined,
      steps:          this.steps.map((s, i) => ({
        stepOrder:   i + 1,
        instruction: s.instruction.trim(),
        durationS:   s.durationS,
        pourWaterG:  s.pourWaterG,
      })),
    };

    const op = this.isEditMode
      ? this.recipeService.update(this.editId!, req)
      : this.recipeService.create(req);

    op.subscribe({
      next:  () => this.router.navigate(['/recipes']),
      error: (e) => { this.error = e?.error ?? 'Failed to save recipe.'; },
    });
  }

  numToAlpha(n: number): string {
    n = Math.round(n);
    if (n <= 0) return 'A';
    let result = '';
    while (n > 0) { n--; result = String.fromCharCode(65 + (n % 26)) + result; n = Math.floor(n / 26); }
    return result;
  }

  numToAlphaNum(n: number, subDivs: number, format: string): string {
    n = Math.round(n * 10) / 10;
    const letterIdx = Math.floor((n - 1) / subDivs) + 1;
    const sub = Math.round(((n - 1) % subDivs) + 1);
    const letter = this.numToAlpha(letterIdx);
    return (format ?? 'A1') === '1A' ? `${sub}${letter}` : `${letter}${sub}`;
  }
}
