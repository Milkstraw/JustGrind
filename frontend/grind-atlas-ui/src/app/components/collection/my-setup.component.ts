import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CollectionService, GrinderService } from '../../services/services';
import {
  UserGrinder,
  UserBrewMethod,
  Grinder,
  BrewMethod,
  BREW_METHOD_LABELS,
} from '../../models/models';

const ALL_BREW_METHODS: BrewMethod[] = [
  'Espresso',
  'MokaPot',
  'AeropressFine',
  'AeropressCoarse',
  'PourOver',
  'Chemex',
  'FrenchPress',
  'ColdBrew',
  'Siphon',
];

@Component({
  selector: 'app-my-setup',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <h1 class="page-title">My Setup</h1>

    <!-- Loading state -->
    <div
      *ngIf="loading"
      style="font-size:12px; color:#666; padding:20px 0;"
      role="status"
      aria-live="polite"
      aria-atomic="true">
      Loading your setup…
    </div>

    <!-- Error state -->
    <div
      *ngIf="error && !loading"
      class="error-msg"
      role="alert"
      aria-live="assertive">
      {{ error }}
    </div>

    <div *ngIf="!loading">

      <!-- ── My Grinders ─────────────────────────────────────────── -->
      <section aria-labelledby="grinders-heading" style="margin-bottom:32px;">
        <div style="display:flex; align-items:baseline; justify-content:space-between; margin-bottom:14px;">
          <h2 class="section-label" id="grinders-heading" style="margin-bottom:0;">My Grinders</h2>
        </div>

        <!-- Empty state -->
        <p
          *ngIf="setupGrinders.length === 0"
          style="font-size:13px; color:#999; margin:0 0 16px;">
          No grinders in your setup.
        </p>

        <!-- Grinder cards -->
        <div
          *ngIf="setupGrinders.length > 0"
          class="grid-2"
          style="margin-bottom:16px;"
          role="list"
          aria-label="Your grinders">

          <article
            *ngFor="let ug of setupGrinders"
            class="panel"
            role="listitem"
            [attr.aria-label]="ug.grinder.brand + ' ' + ug.grinder.model">

            <div class="panel-head">
              <span class="panel-title">{{ ug.grinder.brand }} {{ ug.grinder.model }}</span>
              <button
                class="btn btn-sm"
                (click)="removeGrinder(ug)"
                [disabled]="removingGrinderIds.has(ug.grinderId)"
                [attr.aria-label]="'Remove ' + ug.grinder.brand + ' ' + ug.grinder.model + ' from setup'"
                style="min-width:44px; min-height:44px;">
                {{ removingGrinderIds.has(ug.grinderId) ? 'Removing…' : 'Remove' }}
              </button>
            </div>

            <div class="panel-body">
              <div style="display:flex; gap:6px; flex-wrap:wrap;">
                <span
                  class="status-pill"
                  aria-label="Grind type">
                  {{ ug.grinder.grindType }}
                </span>
                <span
                  class="status-pill"
                  aria-label="Burr type">
                  {{ ug.grinder.burrType }}
                </span>
                <span
                  *ngIf="ug.grinder.burrSizeMm"
                  class="status-pill"
                  aria-label="Burr size">
                  {{ ug.grinder.burrSizeMm }}mm
                </span>
              </div>
            </div>
          </article>
        </div>

        <!-- Add grinder row -->
        <div
          class="panel"
          role="region"
          aria-label="Add a grinder to your setup">
          <div class="panel-head">
            <span class="panel-title">Add Grinder</span>
          </div>
          <div class="panel-body" style="display:flex; gap:10px; flex-wrap:wrap; align-items:flex-end;">
            <div class="form-group" style="margin-bottom:0; flex:1; min-width:200px;">
              <label class="form-label" for="grinder-select">Select Grinder</label>
              <select
                id="grinder-select"
                [(ngModel)]="selectedGrinderId"
                [disabled]="availableGrinders.length === 0"
                aria-label="Choose a grinder to add to your setup"
                style="font-size:16px;">
                <option [ngValue]="null">
                  {{ availableGrinders.length === 0 ? 'All grinders added' : 'Select grinder…' }}
                </option>
                <option *ngFor="let g of availableGrinders" [ngValue]="g.id">
                  {{ g.brand }} {{ g.model }}
                </option>
              </select>
            </div>
            <button
              class="btn btn-inv"
              (click)="addGrinder()"
              [disabled]="selectedGrinderId === null || addingGrinder"
              aria-label="Add selected grinder to your setup"
              style="min-width:44px; min-height:44px;">
              {{ addingGrinder ? 'Adding…' : 'Add' }}
            </button>
          </div>
          <div
            *ngIf="grinderError"
            class="error-msg"
            role="alert"
            style="margin:0 18px 16px;">
            {{ grinderError }}
          </div>
        </div>
      </section>

      <!-- ── My Brew Methods ─────────────────────────────────────── -->
      <section aria-labelledby="brew-methods-heading">
        <div style="margin-bottom:14px;">
          <h2 class="section-label" id="brew-methods-heading" style="margin-bottom:0;">My Brew Methods</h2>
        </div>

        <div
          *ngIf="brewMethodsLoading"
          style="font-size:12px; color:#666;"
          role="status"
          aria-live="polite">
          Loading brew methods…
        </div>

        <div
          class="panel"
          *ngIf="!brewMethodsLoading">
          <div class="panel-head">
            <span class="panel-title">Toggle Brew Methods</span>
            <span
              class="section-label"
              style="margin-bottom:0;"
              aria-live="polite"
              aria-atomic="true">
              {{ selectedBrewMethods.size }} selected
            </span>
          </div>
          <div
            class="panel-body"
            style="display:flex; flex-wrap:wrap; gap:10px;"
            role="group"
            aria-labelledby="brew-methods-heading">

            <button
              *ngFor="let method of allBrewMethods"
              class="btn"
              [class.btn-inv]="selectedBrewMethods.has(method)"
              (click)="toggleBrewMethod(method)"
              [disabled]="togglingBrewMethods.has(method)"
              [attr.aria-pressed]="selectedBrewMethods.has(method)"
              [attr.aria-label]="getBrewMethodLabel(method) + (selectedBrewMethods.has(method) ? ' (selected, click to remove)' : ' (click to add)')"
              style="min-width:44px; min-height:44px;">
              {{ getBrewMethodLabel(method) }}
            </button>
          </div>

          <div
            *ngIf="brewMethodError"
            class="error-msg"
            role="alert"
            style="margin:0 18px 16px;">
            {{ brewMethodError }}
          </div>
        </div>
      </section>

    </div>
  `,
})
export class MySetupComponent implements OnInit {
  private collectionService = inject(CollectionService);
  private grinderService = inject(GrinderService);

  setupGrinders: UserGrinder[] = [];
  allGrinders: Grinder[] = [];
  selectedBrewMethods = new Set<BrewMethod>();

  loading = true;
  brewMethodsLoading = false;
  error: string | null = null;
  grinderError: string | null = null;
  brewMethodError: string | null = null;

  selectedGrinderId: number | null = null;
  addingGrinder = false;
  removingGrinderIds = new Set<number>();
  togglingBrewMethods = new Set<BrewMethod>();

  readonly allBrewMethods: BrewMethod[] = ALL_BREW_METHODS;
  readonly brewMethodLabels = BREW_METHOD_LABELS;

  get availableGrinders(): Grinder[] {
    const ownedIds = new Set(this.setupGrinders.map(ug => ug.grinderId));
    return this.allGrinders.filter(g => !ownedIds.has(g.id));
  }

  ngOnInit(): void {
    forkJoin({
      setupGrinders: this.collectionService.getSetupGrinders().pipe(
        catchError(() => of([] as UserGrinder[]))
      ),
      allGrinders: this.grinderService.getAll().pipe(
        catchError(() => of([] as Grinder[]))
      ),
      brewMethods: this.collectionService.getSetupBrewMethods().pipe(
        catchError(() => of([] as UserBrewMethod[]))
      ),
    }).subscribe({
      next: ({ setupGrinders, allGrinders, brewMethods }) => {
        this.setupGrinders = setupGrinders;
        this.allGrinders = allGrinders;
        this.selectedBrewMethods = new Set(brewMethods.map(bm => bm.brewMethod));
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load your setup. Please try again.';
        this.loading = false;
      },
    });
  }

  addGrinder(): void {
    if (this.selectedGrinderId === null) return;

    const grinderId = this.selectedGrinderId;
    this.addingGrinder = true;
    this.grinderError = null;

    this.collectionService.addGrinderToSetup(grinderId).subscribe({
      next: () => {
        const grinder = this.allGrinders.find(g => g.id === grinderId);
        if (grinder) {
          const newUserGrinder: UserGrinder = {
            id: Date.now(),
            userId: '',
            grinderId: grinder.id,
            addedAt: new Date().toISOString(),
            grinder,
          };
          this.setupGrinders = [...this.setupGrinders, newUserGrinder];
        }
        this.selectedGrinderId = null;
        this.addingGrinder = false;
      },
      error: () => {
        this.addingGrinder = false;
        this.grinderError = 'Failed to add grinder. Please try again.';
      },
    });
  }

  removeGrinder(ug: UserGrinder): void {
    this.removingGrinderIds.add(ug.grinderId);
    this.grinderError = null;

    this.collectionService.removeGrinderFromSetup(ug.grinderId).subscribe({
      next: () => {
        this.setupGrinders = this.setupGrinders.filter(g => g.grinderId !== ug.grinderId);
        this.removingGrinderIds.delete(ug.grinderId);
      },
      error: () => {
        this.removingGrinderIds.delete(ug.grinderId);
        this.grinderError = `Failed to remove ${ug.grinder.brand} ${ug.grinder.model}. Please try again.`;
      },
    });
  }

  toggleBrewMethod(method: BrewMethod): void {
    const wasSelected = this.selectedBrewMethods.has(method);
    this.brewMethodError = null;

    // Optimistic update
    if (wasSelected) {
      this.selectedBrewMethods.delete(method);
    } else {
      this.selectedBrewMethods.add(method);
    }
    this.togglingBrewMethods.add(method);

    const request$ = wasSelected
      ? this.collectionService.removeBrewMethodFromSetup(method)
      : this.collectionService.addBrewMethodToSetup(method);

    request$.subscribe({
      next: () => {
        this.togglingBrewMethods.delete(method);
      },
      error: () => {
        // Rollback optimistic update
        if (wasSelected) {
          this.selectedBrewMethods.add(method);
        } else {
          this.selectedBrewMethods.delete(method);
        }
        this.togglingBrewMethods.delete(method);
        this.brewMethodError = `Failed to ${wasSelected ? 'remove' : 'add'} ${BREW_METHOD_LABELS[method]}. Please try again.`;
      },
    });
  }

  getBrewMethodLabel(method: BrewMethod): string {
    return BREW_METHOD_LABELS[method];
  }
}
