import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { GrinderService } from '../../services/services';
import { GrindType, BurrType, ScaleType } from '../../models/models';

@Component({
  selector: 'app-add-grinder',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <h1 class="page-title">Add Grinder</h1>

    <div class="panel" style="max-width: 640px;">
      <div class="panel-head">
        <span class="panel-title">Grinder Details</span>
        <a routerLink="/grinders" class="btn btn-sm">← Back</a>
      </div>
      <div class="panel-body">

        <!-- Row 1: Brand / Model -->
        <div class="form-grid-2">
          <div class="form-group">
            <label class="form-label">Brand *</label>
            <input type="text" [(ngModel)]="form.brand" placeholder="e.g. Baratza">
          </div>
          <div class="form-group">
            <label class="form-label">Model *</label>
            <input type="text" [(ngModel)]="form.model" placeholder="e.g. Encore">
          </div>
        </div>

        <!-- Row 2: Grind Type / Burr Type -->
        <div class="form-grid-2">
          <div class="form-group">
            <label class="form-label">Grind Type</label>
            <select [(ngModel)]="form.grindType">
              <option value="Stepped">Stepped</option>
              <option value="Stepless">Stepless</option>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">Burr Type</label>
            <select [(ngModel)]="form.burrType">
              <option value="Flat">Flat</option>
              <option value="Conical">Conical</option>
              <option value="Blade">Blade</option>
            </select>
          </div>
        </div>

        <!-- Row 3: Scale Type / Burr Size -->
        <div class="form-grid-2">
          <div class="form-group">
            <label class="form-label">Scale Type</label>
            <select [(ngModel)]="form.scaleType" (ngModelChange)="onScaleTypeChange()">
              <option value="Numeric">Numeric  (1, 2, 3…)</option>
              <option value="Alpha">Alpha  (A, B, C…)</option>
              <option value="AlphaNumeric">Alpha-Numeric  (A1, B3 / A.1, B.3…)</option>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">Burr Size (mm)</label>
            <input type="number" [(ngModel)]="form.burrSizeMm" placeholder="e.g. 40" step="0.5">
          </div>
        </div>

        <!-- AlphaNumeric options (only when AlphaNumeric) -->
        <div class="form-grid-3" *ngIf="form.scaleType === 'AlphaNumeric'">
          <div class="form-group">
            <label class="form-label">Sub-positions per letter</label>
            <input type="number" [(ngModel)]="form.scaleSubDivisions" min="1" max="100" placeholder="e.g. 9">
            <span style="font-size:10px; color:#888; margin-top:3px;">
              How many numbers per letter (e.g. 9 for A1–A9, 10 for A0–A9)
            </span>
          </div>
          <div class="form-group">
            <label class="form-label">Format</label>
            <select [(ngModel)]="form.scaleFormat">
              <option value="A1">A1 (letter first)</option>
              <option value="1A">1A (number first)</option>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">Sub-position Type</label>
            <select [(ngModel)]="form.scaleSubType">
              <option value="Stepped">Stepped</option>
              <option value="Stepless">Stepless</option>
            </select>
          </div>
        </div>

        <!-- Scale Min / Scale Max — always on the same row -->
        <div class="form-grid-2">
          <div class="form-group">
            <label class="form-label">Scale Min *</label>
            <!-- Numeric -->
            <input *ngIf="form.scaleType === 'Numeric'"
              type="number" [(ngModel)]="form.scaleMin" placeholder="e.g. 0">
            <!-- Alpha -->
            <input *ngIf="form.scaleType === 'Alpha'"
              type="text" [(ngModel)]="scaleMinDisplay"
              placeholder="e.g. A" style="text-transform: uppercase;"
              maxlength="3">
            <!-- AlphaNumeric -->
            <input *ngIf="form.scaleType === 'AlphaNumeric'"
              type="text" [(ngModel)]="scaleMinDisplay"
              [placeholder]="alphaNumPlaceholder('min')"
              style="text-transform: uppercase;"
              [style.border-color]="scaleMinDisplay && !isValidScaleInput(scaleMinDisplay) ? 'red' : ''">
          </div>
          <div class="form-group">
            <label class="form-label">Scale Max *</label>
            <!-- Numeric -->
            <input *ngIf="form.scaleType === 'Numeric'"
              type="number" [(ngModel)]="form.scaleMax" placeholder="e.g. 40">
            <!-- Alpha -->
            <input *ngIf="form.scaleType === 'Alpha'"
              type="text" [(ngModel)]="scaleMaxDisplay"
              placeholder="e.g. Z" style="text-transform: uppercase;"
              maxlength="3">
            <!-- AlphaNumeric -->
            <input *ngIf="form.scaleType === 'AlphaNumeric'"
              type="text" [(ngModel)]="scaleMaxDisplay"
              [placeholder]="alphaNumPlaceholder('max')"
              style="text-transform: uppercase;"
              [style.border-color]="scaleMaxDisplay && !isValidScaleInput(scaleMaxDisplay) ? 'red' : ''">
          </div>
        </div>

        <!-- Scale preview -->
        <div *ngIf="showScalePreview" style="margin-bottom: 16px; font-size: 11px; color: #666; letter-spacing: 0.04em;">
          Range encodes as: {{ computeScaleMin() }} – {{ computeScaleMax() }}
          &nbsp;({{ computeScaleMax() - computeScaleMin() + 1 }} positions)
        </div>

        <!-- Notes -->
        <div class="form-group">
          <label class="form-label">Notes</label>
          <textarea [(ngModel)]="form.notes" rows="3" placeholder="Any additional notes…" style="resize: vertical;"></textarea>
        </div>

        <div *ngIf="error" class="error-msg">{{ error }}</div>

        <div style="display: flex; gap: 10px; margin-top: 20px;">
          <button class="btn btn-inv" (click)="submit()" [disabled]="!canSubmit || saving">
            {{ saving ? 'Saving...' : 'Save Grinder' }}
          </button>
          <a routerLink="/grinders" class="btn">Cancel</a>
        </div>
      </div>
    </div>
  `,
})
export class AddGrinderComponent {
  private grinderService = inject(GrinderService);
  private router         = inject(Router);

  form: {
    brand: string;
    model: string;
    grindType: GrindType;
    burrType: BurrType;
    scaleType: ScaleType;
    scaleSubDivisions: number;
    scaleFormat: string;
    scaleSubType: string;
    burrSizeMm?: number;
    scaleMin?: number;
    scaleMax?: number;
    notes: string;
  } = {
    brand: '',
    model: '',
    grindType: 'Stepped',
    burrType: 'Conical',
    scaleType: 'Numeric',
    scaleSubDivisions: 9,
    scaleFormat: 'A1',
    scaleSubType: 'Stepped',
    notes: '',
  };

  // Display strings used for Alpha and AlphaNumeric inputs
  scaleMinDisplay = '';
  scaleMaxDisplay = '';

  saving = false;
  error?: string;

  onScaleTypeChange() {
    this.scaleMinDisplay = '';
    this.scaleMaxDisplay = '';
    this.form.scaleMin   = undefined;
    this.form.scaleMax   = undefined;
  }

  get showScalePreview(): boolean {
    if (this.form.scaleType === 'Alpha') {
      return this.scaleMinDisplay.trim() !== '' && this.scaleMaxDisplay.trim() !== '';
    }
    if (this.form.scaleType === 'AlphaNumeric') {
      return this.scaleMinDisplay.trim() !== '' && this.scaleMaxDisplay.trim() !== '';
    }
    return false;
  }

  alphaNumPlaceholder(end: 'min' | 'max'): string {
    if (this.form.scaleFormat === '1A') {
      return end === 'min' ? 'e.g. 1A' : 'e.g. 9Z';
    }
    return end === 'min' ? 'e.g. A1' : 'e.g. Z9';
  }

  isValidScaleInput(val: string): boolean {
    if (this.form.scaleFormat === '1A') {
      return /^\d+(\.\d+)?[A-Za-z]+$/.test(val);
    }
    return /^[A-Za-z]+\d+(\.\d+)?$/.test(val);
  }

  get canSubmit(): boolean {
    if (!this.form.brand.trim() || !this.form.model.trim()) return false;
    if (this.form.scaleType === 'Numeric') {
      return this.form.scaleMin != null && this.form.scaleMax != null;
    }
    return this.scaleMinDisplay.trim() !== '' && this.scaleMaxDisplay.trim() !== '';
  }

  computeScaleMin(): number {
    if (this.form.scaleType === 'Alpha') return this.alphaToNum(this.scaleMinDisplay);
    if (this.form.scaleType === 'AlphaNumeric') {
      return this.alphaNumToDecimal(this.scaleMinDisplay, this.form.scaleSubDivisions, this.form.scaleFormat);
    }
    return this.form.scaleMin ?? 0;
  }

  computeScaleMax(): number {
    if (this.form.scaleType === 'Alpha') return this.alphaToNum(this.scaleMaxDisplay);
    if (this.form.scaleType === 'AlphaNumeric') {
      return this.alphaNumToDecimal(this.scaleMaxDisplay, this.form.scaleSubDivisions, this.form.scaleFormat);
    }
    return this.form.scaleMax ?? 0;
  }

  alphaToNum(s: string): number {
    s = s.toUpperCase().trim();
    let n = 0;
    for (const ch of s) {
      n = n * 26 + (ch.charCodeAt(0) - 64);
    }
    return n;
  }

  /** "A1" or "1A" → decimal. */
  alphaNumToDecimal(s: string, subDivs: number, format: string): number {
    s = s.toUpperCase().trim();
    if (format === '1A') {
      const match = s.match(/^([\d.]+)([A-Z]+)$/);
      if (!match) return 0;
      return (this.alphaToNum(match[2]) - 1) * subDivs + parseFloat(match[1]);
    }
    // default A1
    const match = s.match(/^([A-Z]+)([\d.]+)$/);
    if (!match) return 0;
    return (this.alphaToNum(match[1]) - 1) * subDivs + parseFloat(match[2]);
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
    const letterIdx = Math.floor((n - 1) / subDivs) + 1;
    const sub       = Math.round(((n - 1) % subDivs) + 1);
    const letter    = this.numToAlpha(letterIdx);
    return format === '1A' ? `${sub}${letter}` : `${letter}${sub}`;
  }

  submit() {
    if (!this.canSubmit) return;
    this.saving = true;
    this.error  = undefined;

    const payload: any = {
      brand:      this.form.brand,
      model:      this.form.model,
      grindType:  this.form.grindType,
      burrType:   this.form.burrType,
      scaleType:  this.form.scaleType,
      scaleMin:   this.computeScaleMin(),
      scaleMax:   this.computeScaleMax(),
      isVerified: false,
    };

    if (this.form.burrSizeMm)  payload.burrSizeMm = this.form.burrSizeMm;
    if (this.form.notes.trim()) payload.notes = this.form.notes;

    if (this.form.scaleType === 'AlphaNumeric') {
      payload.scaleSubDivisions = this.form.scaleSubDivisions;
      payload.scaleFormat       = this.form.scaleFormat;
      payload.scaleSubType      = this.form.scaleSubType;
    }

    this.grinderService.create(payload).subscribe({
      next: () => this.router.navigate(['/grinders']),
      error: e => {
        this.error  = e.message ?? 'Failed to save grinder.';
        this.saving = false;
      },
    });
  }
}
