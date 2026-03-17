import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CoffeeService } from '../../services/services';
import { ProcessingMethod, Species } from '../../models/models';

@Component({
  selector: 'app-add-coffee',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <h1 class="page-title">Add Coffee</h1>

    <div class="panel" style="max-width: 700px;">
      <div class="panel-head">
        <span class="panel-title">Coffee Details</span>
        <a routerLink="/coffees" class="btn btn-sm">← Back</a>
      </div>
      <div class="panel-body">
        <div class="form-grid-2">
          <div class="form-group">
            <label class="form-label">Name *</label>
            <input type="text" [(ngModel)]="form.name" placeholder="e.g. Yirgacheffe Natural">
          </div>
          <div class="form-group">
            <label class="form-label">Roaster *</label>
            <input type="text" [(ngModel)]="form.roaster" placeholder="e.g. Onyx Coffee Lab">
          </div>
          <div class="form-group">
            <label class="form-label">Origin Country</label>
            <input type="text" [(ngModel)]="form.originCountry" placeholder="e.g. Ethiopia">
          </div>
          <div class="form-group">
            <label class="form-label">Origin Region</label>
            <input type="text" [(ngModel)]="form.originRegion" placeholder="e.g. Yirgacheffe">
          </div>
          <div class="form-group">
            <label class="form-label">Elevation (masl)</label>
            <input type="number" [(ngModel)]="form.elevationMasl" placeholder="e.g. 1900">
          </div>
          <div class="form-group">
            <label class="form-label">Variety</label>
            <input type="text" [(ngModel)]="form.variety" placeholder="e.g. Geisha, Bourbon">
          </div>
          <div class="form-group">
            <label class="form-label">Processing Method</label>
            <select [(ngModel)]="form.processingMethod">
              <option value="Washed">Washed</option>
              <option value="Natural">Natural</option>
              <option value="Honey">Honey</option>
              <option value="Anaerobic">Anaerobic</option>
              <option value="WetHulled">Wet Hulled</option>
              <option value="Other">Other</option>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">Species</label>
            <select [(ngModel)]="form.species">
              <option value="Arabica">Arabica</option>
              <option value="Robusta">Robusta</option>
              <option value="Liberica">Liberica</option>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">Roast Level (1–5)</label>
            <input type="number" [(ngModel)]="form.roastLevel" min="1" max="5" step="0.1" placeholder="e.g. 2.5">
          </div>
          <div class="form-group">
            <label class="form-label">Roast Date</label>
            <input type="date" [(ngModel)]="form.roastDate">
          </div>
        </div>

        <div class="form-group">
          <label class="form-label">Tasting Notes (comma-separated)</label>
          <input type="text" [(ngModel)]="form.tastingNotes" placeholder="e.g. blueberry, dark chocolate, jasmine">
        </div>

        <div class="form-group">
          <label class="form-label">Notes</label>
          <textarea [(ngModel)]="form.notes" rows="3" placeholder="Any additional notes…" style="resize: vertical;"></textarea>
        </div>

        <div *ngIf="error" class="error-msg">{{ error }}</div>

        <div style="display: flex; gap: 10px; margin-top: 20px;">
          <button class="btn btn-inv" (click)="submit()" [disabled]="!canSubmit || saving">
            {{ saving ? 'Saving...' : 'Save Coffee' }}
          </button>
          <a routerLink="/coffees" class="btn">Cancel</a>
        </div>
      </div>
    </div>
  `,
})
export class AddCoffeeComponent {
  private coffeeService = inject(CoffeeService);
  private router        = inject(Router);

  form: {
    name: string;
    roaster: string;
    originCountry: string;
    originRegion: string;
    elevationMasl?: number;
    processingMethod: ProcessingMethod;
    variety: string;
    species: Species;
    roastLevel?: number;
    roastDate: string;
    tastingNotes: string;
    notes: string;
  } = {
    name: '',
    roaster: '',
    originCountry: '',
    originRegion: '',
    processingMethod: 'Washed',
    variety: '',
    species: 'Arabica',
    roastDate: '',
    tastingNotes: '',
    notes: '',
  };

  saving = false;
  error?: string;

  get canSubmit() {
    return this.form.name.trim() && this.form.roaster.trim();
  }

  submit() {
    if (!this.canSubmit) return;
    this.saving = true;
    this.error  = undefined;

    const payload: any = { ...this.form };
    if (!payload.originCountry)  delete payload.originCountry;
    if (!payload.originRegion)   delete payload.originRegion;
    if (!payload.elevationMasl)  delete payload.elevationMasl;
    if (!payload.variety)        delete payload.variety;
    if (!payload.roastLevel)     delete payload.roastLevel;
    if (!payload.roastDate)      delete payload.roastDate;
    if (!payload.tastingNotes)   delete payload.tastingNotes;
    if (!payload.notes)          delete payload.notes;
    payload.isBlend  = false;
    payload.isActive = true;

    this.coffeeService.create(payload).subscribe({
      next: () => this.router.navigate(['/coffees']),
      error: e => {
        this.error  = e.message ?? 'Failed to save coffee.';
        this.saving = false;
      },
    });
  }
}
