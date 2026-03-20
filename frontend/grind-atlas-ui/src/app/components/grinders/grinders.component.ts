import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { GrinderService, CollectionService } from '../../services/services';
import { Grinder } from '../../models/models';

@Component({
  selector: 'app-grinders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div style="display:flex; align-items:baseline; justify-content:space-between; margin-bottom:24px;">
      <h1 class="page-title" style="margin-bottom:0;">Grinders</h1>
      <a routerLink="/grinders/add" class="btn btn-inv">Add Grinder</a>
    </div>

    <div class="section-label" aria-live="polite">{{ grinders.length }} grinders</div>

    <div *ngIf="!grinders.length" style="font-size:12px; color:#666; padding:20px 0;" role="status" aria-live="polite">Loading grinders…</div>

    <div class="grid-2" *ngIf="grinders.length">
      <article class="panel" *ngFor="let g of grinders" [attr.aria-label]="g.brand + ' ' + g.model">
        <div class="panel-head">
          <h2 class="panel-title">{{ g.brand }} {{ g.model }}</h2>
          <span class="status-pill" [attr.aria-label]="'Burr type: ' + g.burrType">{{ g.burrType }}</span>
          <span *ngIf="setupIds.has(g.id)" class="status-pill s-active" style="font-size:9px;" aria-label="In your setup">Setup</span>
        </div>
        <div class="panel-body">
          <dl style="display:flex; gap:24px; margin-bottom: 14px; flex-wrap:wrap;">
            <div>
              <dt class="stat-label">Grind Type</dt>
              <dd style="font-size:12px; margin:0;">{{ g.grindType }}</dd>
            </div>
            <div>
              <dt class="stat-label">Scale</dt>
              <dd style="font-size:13px; margin:0;">{{ formatScale(g) }}</dd>
            </div>
            <div>
              <dt class="stat-label">Scale Type</dt>
              <dd style="font-size:13px; margin:0;">{{ g.scaleType }}</dd>
            </div>
            <div *ngIf="g.burrSizeMm">
              <dt class="stat-label">Burr Size</dt>
              <dd style="font-size:12px; margin:0;">{{ g.burrSizeMm }}mm</dd>
            </div>
            <div>
              <dt class="stat-label">Verified</dt>
              <dd style="font-size:12px; margin:0;">{{ g.isVerified ? 'Yes' : 'No' }}</dd>
            </div>
          </dl>

          <!-- Setup toggle -->
          <div style="margin-bottom:12px;">
            <button
              *ngIf="!setupIds.has(g.id)"
              class="btn btn-sm"
              (click)="addToSetup(g)"
              [disabled]="pendingSetupId === g.id"
              [attr.aria-label]="'Add ' + g.brand + ' ' + g.model + ' to My Setup'">
              + My Setup
            </button>
            <button
              *ngIf="setupIds.has(g.id)"
              class="btn btn-sm"
              (click)="removeFromSetup(g)"
              [disabled]="pendingSetupId === g.id"
              [attr.aria-label]="'Remove ' + g.brand + ' ' + g.model + ' from My Setup'">
              − My Setup
            </button>
          </div>

          <!-- Calibrations -->
          <div *ngIf="g.calibrations?.length">
            <h3 style="font-size:9px; letter-spacing:0.12em; text-transform:uppercase; color:#666; margin-bottom:6px; font-weight:400;" [id]="'cal-heading-' + g.id">
              NGI Calibrations ({{ g.calibrations!.length }})
            </h3>
            <table class="table" [attr.aria-labelledby]="'cal-heading-' + g.id">
              <thead>
                <tr>
                  <th scope="col">Brew Method</th>
                  <th scope="col">Native</th>
                  <th scope="col">NGI</th>
                  <th scope="col">Label</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let c of g.calibrations">
                  <td>{{ formatBrewMethod(c.brewMethod) }}</td>
                  <td>{{ c.nativeSetting }}</td>
                  <td>
                    <span class="status-pill s-hold">{{ c.ngiValue }}</span>
                  </td>
                  <td style="color:#666;">{{ c.anchorLabel || '—' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </article>
    </div>
  `,
})
export class GrindersComponent implements OnInit {
  private grinderService    = inject(GrinderService);
  private collectionService = inject(CollectionService);

  grinders: Grinder[] = [];
  setupIds = new Set<number>();
  pendingSetupId?: number;

  ngOnInit() {
    forkJoin({
      grinders: this.grinderService.getAll(),
      setup:    this.collectionService.getSetupGrinders(),
    }).subscribe({
      next: ({ grinders, setup }) => {
        this.grinders = grinders;
        this.setupIds = new Set(setup.map(s => s.grinderId));
      },
      error: () => {
        this.grinderService.getAll().subscribe(g => this.grinders = g);
      },
    });
  }

  addToSetup(g: Grinder): void {
    this.pendingSetupId = g.id;
    this.collectionService.addGrinderToSetup(g.id).subscribe({
      next:  () => { this.setupIds.add(g.id); this.pendingSetupId = undefined; },
      error: () => { this.pendingSetupId = undefined; },
    });
  }

  removeFromSetup(g: Grinder): void {
    this.pendingSetupId = g.id;
    this.collectionService.removeGrinderFromSetup(g.id).subscribe({
      next:  () => { this.setupIds.delete(g.id); this.pendingSetupId = undefined; },
      error: () => { this.pendingSetupId = undefined; },
    });
  }

  formatBrewMethod(m: string): string {
    return m.replace(/([A-Z])/g, ' $1').trim();
  }

  formatScale(g: Grinder): string {
    if (g.scaleType === 'Alpha') {
      return `${this.numToAlpha(g.scaleMin)} – ${this.numToAlpha(g.scaleMax)}`;
    }
    if (g.scaleType === 'AlphaNumeric') {
      const subDivs = g.scaleSubDivisions ?? 9;
      const fmt     = g.scaleFormat ?? 'A1';
      return `${this.numToAlphaNum(g.scaleMin, subDivs, fmt)} – ${this.numToAlphaNum(g.scaleMax, subDivs, fmt)}`;
    }
    return `${g.scaleMin} – ${g.scaleMax}`;
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
