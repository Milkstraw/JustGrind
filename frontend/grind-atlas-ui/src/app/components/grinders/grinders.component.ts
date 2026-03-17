import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { GrinderService } from '../../services/services';
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

    <div class="section-label">{{ grinders.length }} grinders</div>

    <div *ngIf="!grinders.length" style="font-size:12px; color:#666; padding:20px 0;">LOADING...</div>

    <div class="grid-2" *ngIf="grinders.length">
      <div class="panel" *ngFor="let g of grinders">
        <div class="panel-head">
          <span class="panel-title">{{ g.brand }} {{ g.model }}</span>
          <span class="status-pill">{{ g.burrType }}</span>
        </div>
        <div class="panel-body">
          <div style="display:flex; gap:24px; margin-bottom: 14px;">
            <div>
              <div class="stat-label">Grind Type</div>
              <div style="font-size:12px;">{{ g.grindType }}</div>
            </div>
            <div>
              <div class="stat-label">Scale</div>
              <div style="font-size:13px;">{{ formatScale(g) }}</div>
            </div>
            <div>
              <div class="stat-label">Scale Type</div>
              <div style="font-size:13px;">{{ g.scaleType }}</div>
            </div>
            <div *ngIf="g.burrSizeMm">
              <div class="stat-label">Burr Size</div>
              <div style="font-size:12px;">{{ g.burrSizeMm }}mm</div>
            </div>
            <div>
              <div class="stat-label">Verified</div>
              <div style="font-size:12px;">{{ g.isVerified ? 'Yes' : 'No' }}</div>
            </div>
          </div>

          <!-- Calibrations -->
          <div *ngIf="g.calibrations?.length">
            <div style="font-size:9px; letter-spacing:0.12em; text-transform:uppercase; color:#666; margin-bottom:6px;">
              NGI Calibrations ({{ g.calibrations!.length }})
            </div>
            <table class="table">
              <thead>
                <tr>
                  <th>Brew Method</th>
                  <th>Native</th>
                  <th>NGI</th>
                  <th>Label</th>
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
      </div>
    </div>
  `,
})
export class GrindersComponent implements OnInit {
  private grinderService = inject(GrinderService);
  grinders: Grinder[] = [];

  ngOnInit() {
    this.grinderService.getAll().subscribe(g => this.grinders = g);
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
