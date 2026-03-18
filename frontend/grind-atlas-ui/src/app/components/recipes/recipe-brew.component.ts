import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink, CanDeactivateFn } from '@angular/router';
import { RecipeService, GrindLogService, GrinderService } from '../../services/services';
import {
  BrewRecipe, BrewRecipeStep, Grinder, BREW_METHOD_LABELS, BrewMethod,
  AddGrindLogRequest
} from '../../models/models';

export const brewGuard: CanDeactivateFn<RecipeBrewComponent> = (component) => {
  if (component.isRunning) {
    return confirm('A brew is in progress. Are you sure you want to leave?');
  }
  return true;
};

@Component({
  selector: 'app-recipe-brew',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div *ngIf="!recipe" style="text-align:center; padding:60px; color:#999;">Loading recipe…</div>

    <ng-container *ngIf="recipe">
      <!-- Header bar -->
      <div style="display:flex; align-items:baseline; justify-content:space-between; margin-bottom:24px;">
        <h1 class="page-title" style="margin-bottom:0;">{{ recipe.name }}</h1>
        <a routerLink="/recipes" class="btn">← Recipes</a>
      </div>

      <!-- Recipe params reference strip -->
      <div class="panel" style="margin-bottom:20px;">
        <div class="panel-body" style="padding:12px 18px;">
          <div style="display:flex; flex-wrap:wrap; gap:24px; font-size:12px;">
            <div>
              <span class="form-label" style="display:block;">Coffee</span>
              <strong>{{ recipe.coffee?.name ?? 'Coffee #' + recipe.coffeeId }}</strong>
            </div>
            <div>
              <span class="form-label" style="display:block;">Grinder</span>
              <strong>{{ recipe.grinder?.brand }} {{ recipe.grinder?.model }}</strong>
            </div>
            <div>
              <span class="form-label" style="display:block;">Method</span>
              <strong>{{ formatMethod(recipe.brewMethod) }}</strong>
            </div>
            <div *ngIf="recipe.nativeSetting != null">
              <span class="form-label" style="display:block;">Grind Setting</span>
              <strong>{{ formatSetting(recipe.nativeSetting) }}</strong>
            </div>
            <div *ngIf="recipe.waterTempC != null">
              <span class="form-label" style="display:block;">Water Temp</span>
              <strong>{{ recipe.waterTempC }}°C</strong>
            </div>
            <div *ngIf="recipe.doseG != null">
              <span class="form-label" style="display:block;">Dose</span>
              <strong>{{ recipe.doseG }}g</strong>
            </div>
            <div *ngIf="recipe.waterG != null">
              <span class="form-label" style="display:block;">Water</span>
              <strong>{{ recipe.waterG }}g</strong>
            </div>
          </div>
          <div *ngIf="recipe.techniqueNotes" style="margin-top:10px; font-size:12px; color:#555; border-top:1px solid var(--mid); padding-top:10px;">
            {{ recipe.techniqueNotes }}
          </div>
        </div>
      </div>

      <!-- Timer display -->
      <div class="panel" style="margin-bottom:20px;" *ngIf="!isComplete">
        <div class="panel-body" style="padding:20px 18px;">
          <div style="display:flex; align-items:center; justify-content:space-between; flex-wrap:wrap; gap:16px;">
            <div>
              <div class="form-label">Elapsed</div>
              <div style="font-size:48px; font-weight:700; letter-spacing:-0.04em; line-height:1; font-variant-numeric:tabular-nums;">
                {{ formatTime(elapsedTotal) }}
              </div>
            </div>
            <div style="display:flex; gap:12px; align-items:center;">
              <button class="btn btn-inv" (click)="start()" *ngIf="!isRunning" [disabled]="isComplete">
                {{ isPaused ? 'Resume' : 'Start' }}
              </button>
              <button class="btn" (click)="pause()" *ngIf="isRunning">Pause</button>
              <button class="btn" (click)="reset()" *ngIf="isStarted">Reset</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Steps list -->
      <div class="panel" style="margin-bottom:20px;" *ngIf="!isComplete">
        <div class="panel-head"><span class="panel-title">Steps</span></div>
        <div class="panel-body" style="padding:0;">
          <div *ngFor="let step of recipe.steps; let i = index"
            [style.border-bottom]="i < recipe.steps.length - 1 ? '1px solid var(--mid)' : 'none'"
            [style.background]="isActiveStep(i) ? 'var(--paper)' : isCompletedStep(i) ? '#f9f9f9' : '#fff'"
            style="padding:16px 18px; transition:background 0.2s;">

            <div style="display:flex; align-items:flex-start; gap:16px;">
              <!-- Step number / check -->
              <div style="
                min-width:32px; height:32px; border: var(--b-mid);
                display:flex; align-items:center; justify-content:center;
                font-weight:700; font-size:12px; flex-shrink:0;
                background:{{ isCompletedStep(i) ? 'var(--ink)' : isActiveStep(i) ? 'var(--ink)' : 'transparent' }};
                color:{{ isCompletedStep(i) || isActiveStep(i) ? 'var(--paper)' : 'var(--ink)' }};">
                {{ isCompletedStep(i) ? '✓' : i + 1 }}
              </div>

              <!-- Content -->
              <div style="flex:1;">
                <div style="display:flex; align-items:baseline; justify-content:space-between; gap:16px; flex-wrap:wrap;">
                  <div [style.color]="isCompletedStep(i) ? '#999' : 'var(--ink)'"
                    [style.font-weight]="isActiveStep(i) ? '700' : '400'"
                    style="font-size:14px;">
                    {{ step.instruction }}
                  </div>
                  <div style="display:flex; gap:12px; align-items:baseline; flex-shrink:0;">
                    <span *ngIf="step.pourWaterG" style="font-size:12px; color:#666;">
                      {{ step.pourWaterG }}g
                    </span>
                    <span style="font-size:13px; font-variant-numeric:tabular-nums;"
                      [style.color]="isCompletedStep(i) ? '#999' : 'var(--ink)'"
                      [style.font-weight]="isActiveStep(i) ? '700' : '400'">
                      {{ formatTime(step.durationS) }}
                    </span>
                  </div>
                </div>

                <!-- Active step progress -->
                <div *ngIf="isActiveStep(i)" style="margin-top:10px;">
                  <div style="display:flex; justify-content:space-between; font-size:11px; color:#666; margin-bottom:5px;">
                    <span>Step {{ i + 1 }} of {{ recipe.steps.length }}</span>
                    <span style="font-weight:700; font-variant-numeric:tabular-nums;">
                      {{ formatTime(stepRemaining) }} remaining
                    </span>
                  </div>
                  <div class="bar-track">
                    <div class="bar-fill" [style.width.%]="stepProgress"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Brew complete + review form -->
      <div *ngIf="isComplete" class="panel" style="margin-bottom:20px;">
        <div class="panel-head">
          <span class="panel-title">Brew Complete — {{ formatTime(elapsedTotal) }}</span>
        </div>
        <div class="panel-body">
          <p style="margin:0 0 20px; font-size:13px; color:#555;">
            Log your results. At least one field is required to save.
          </p>
          <div class="form-grid-3" style="margin-bottom:14px;">
            <div class="form-group" style="margin-bottom:0;">
              <label class="form-label">Dose (g)</label>
              <input type="number" [(ngModel)]="review.doseG" step="0.1" placeholder="e.g. 15">
            </div>
            <div class="form-group" style="margin-bottom:0;">
              <label class="form-label">Yield (g)</label>
              <input type="number" [(ngModel)]="review.yieldG" step="0.1" placeholder="e.g. 250">
            </div>
            <div class="form-group" style="margin-bottom:0;">
              <label class="form-label">Extraction Time (s)</label>
              <input type="number" [(ngModel)]="review.extractionTimeS" placeholder="e.g. 210">
            </div>
            <div class="form-group" style="margin-bottom:0;">
              <label class="form-label">Rating (1–5)</label>
              <select [(ngModel)]="review.rating">
                <option [ngValue]="undefined">—</option>
                <option [ngValue]="1">1</option>
                <option [ngValue]="2">2</option>
                <option [ngValue]="3">3</option>
                <option [ngValue]="4">4</option>
                <option [ngValue]="5">5</option>
              </select>
            </div>
            <div class="form-group" style="margin-bottom:0; grid-column:span 2;">
              <label class="form-label">Notes</label>
              <input type="text" [(ngModel)]="review.notes" placeholder="Observations from this brew…">
            </div>
          </div>

          <div style="display:flex; gap:12px; align-items:center;">
            <button class="btn btn-inv" (click)="saveReview()" [disabled]="!canSaveReview || saving">
              {{ saving ? 'Saving…' : 'Save Session' }}
            </button>
            <a routerLink="/recipes" class="btn btn-sm">Skip</a>
          </div>
          <div *ngIf="!canSaveReview" style="font-size:11px; color:#999; margin-top:8px;">
            Fill in at least one field to save.
          </div>
        </div>
      </div>
    </ng-container>
  `,
})
export class RecipeBrewComponent implements OnInit, OnDestroy {
  private recipeService  = inject(RecipeService);
  private logService     = inject(GrindLogService);
  private grinderService = inject(GrinderService);
  private route          = inject(ActivatedRoute);
  private router         = inject(Router);

  recipe?: BrewRecipe;
  grinder?: Grinder;

  // Timer state
  isStarted  = false;
  isRunning  = false;
  isPaused   = false;
  isComplete = false;
  elapsedMs        = 0;  // total milliseconds since start
  stepElapsedMs    = 0;  // milliseconds elapsed in current step
  currentStepIndex = 0;
  private timerHandle?: ReturnType<typeof setInterval>;

  get elapsedTotal(): number { return Math.floor(this.elapsedMs / 1000); }

  // Review form
  review: { doseG?: number; yieldG?: number; extractionTimeS?: number; rating?: number; notes?: string } = {};
  saving = false;

  get stepRemaining(): number {
    const s = this.recipe?.steps[this.currentStepIndex];
    return s ? Math.max(0, Math.ceil((s.durationS * 1000 - this.stepElapsedMs) / 1000)) : 0;
  }

  get stepProgress(): number {
    const s = this.recipe?.steps[this.currentStepIndex];
    if (!s || s.durationS === 0) return 100;
    return Math.min(100, (this.stepElapsedMs / (s.durationS * 1000)) * 100);
  }

  get canSaveReview(): boolean {
    return !!(this.review.doseG || this.review.yieldG || this.review.extractionTimeS ||
              this.review.rating || (this.review.notes && this.review.notes.trim()));
  }

  ngOnInit() {
    const id = +this.route.snapshot.paramMap.get('id')!;
    this.recipeService.getById(id).subscribe(r => {
      this.recipe = r;
      if (r.grinderId) {
        this.grinderService.getById(r.grinderId).subscribe(g => this.grinder = g);
      }
    });
  }

  ngOnDestroy() {
    this.stopTimer();
  }

  isActiveStep(i: number): boolean {
    return this.isStarted && !this.isComplete && i === this.currentStepIndex;
  }

  isCompletedStep(i: number): boolean {
    return this.isStarted && i < this.currentStepIndex;
  }

  start() {
    if (!this.recipe || this.isComplete) return;
    this.isStarted = true;
    this.isRunning = true;
    this.isPaused  = false;
    this.timerHandle = setInterval(() => this.tick(), 100);
  }

  pause() {
    this.isRunning = false;
    this.isPaused  = true;
    this.stopTimer();
  }

  reset() {
    this.stopTimer();
    this.isStarted        = false;
    this.isRunning        = false;
    this.isPaused         = false;
    this.isComplete       = false;
    this.elapsedMs        = 0;
    this.stepElapsedMs    = 0;
    this.currentStepIndex = 0;
  }

  private tick() {
    if (!this.recipe) return;
    this.elapsedMs     += 100;
    this.stepElapsedMs += 100;
    const step = this.recipe.steps[this.currentStepIndex];
    if (step && this.stepElapsedMs >= step.durationS * 1000) {
      if (this.currentStepIndex < this.recipe.steps.length - 1) {
        this.currentStepIndex++;
        this.stepElapsedMs = 0;
      } else {
        this.stopTimer();
        this.isRunning  = false;
        this.isComplete = true;
      }
    }
  }

  private stopTimer() {
    if (this.timerHandle) {
      clearInterval(this.timerHandle);
      this.timerHandle = undefined;
    }
  }

  saveReview() {
    if (!this.canSaveReview || !this.recipe || this.saving) return;
    this.saving = true;
    const req: AddGrindLogRequest = {
      coffeeId:        this.recipe.coffeeId,
      grinderId:       this.recipe.grinderId,
      brewMethod:      this.recipe.brewMethod,
      nativeSetting:   this.recipe.nativeSetting ?? 0,
      doseG:           this.review.doseG,
      yieldG:          this.review.yieldG,
      extractionTimeS: this.review.extractionTimeS,
      rating:          this.review.rating,
      notes:           this.review.notes?.trim() || undefined,
      recipeId:        this.recipe.id,
    };
    this.logService.create(req).subscribe({
      next:  () => this.router.navigate(['/logs']),
      error: () => { this.saving = false; },
    });
  }

  formatTime(seconds: number): string {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  }

  formatMethod(m: string): string {
    return BREW_METHOD_LABELS[m as BrewMethod] ?? m;
  }

  formatSetting(native: number): string {
    const g = this.grinder ?? this.recipe?.grinder;
    if (!g) return String(native);
    if (g.scaleType === 'Alpha') return this.numToAlpha(Math.round(native));
    if (g.scaleType === 'AlphaNumeric') return this.numToAlphaNum(native, g.scaleSubDivisions ?? 9, g.scaleFormat ?? 'A1');
    return String(native);
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
