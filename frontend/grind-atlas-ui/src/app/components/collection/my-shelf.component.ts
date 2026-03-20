import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, of, Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CollectionService, CoffeeService } from '../../services/services';
import {
  UserCoffee,
  Coffee,
  CoffeeBag,
  OpenCoffeeBagRequest,
  FreshnessInfo,
} from '../../models/models';

interface OpenBagForm {
  roastedOn: string;
  bagWeightG: number | null;
  notes: string;
}

@Component({
  selector: 'app-my-shelf',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <h1 class="page-title">My Shelf</h1>

    <!-- Loading state -->
    <div
      *ngIf="loading"
      style="font-size:12px; color:#666; padding:20px 0;"
      role="status"
      aria-live="polite"
      aria-atomic="true">
      Loading your shelf…
    </div>

    <!-- Error state -->
    <div
      *ngIf="error && !loading"
      class="error-msg"
      role="alert"
      aria-live="assertive">
      {{ error }}
    </div>

    <!-- Empty state -->
    <div
      *ngIf="!loading && !error && shelfItems.length === 0"
      class="panel"
      role="status">
      <div class="panel-body" style="text-align:center; padding:40px 20px; color:#666;">
        Your shelf is empty.
        <a routerLink="/coffees" class="btn" style="margin-top:16px; display:inline-block;">Browse Coffees</a>
        to add some.
      </div>
    </div>

    <!-- Shelf grid -->
    <div
      *ngIf="!loading && !error && shelfItems.length > 0"
      class="grid-2"
      role="list"
      aria-label="Your coffee shelf">

      <article
        *ngFor="let item of shelfItems"
        class="panel"
        role="listitem"
        [attr.aria-label]="item.coffee.name + ' by ' + (item.coffee.roaster || 'Unknown Roaster')">

        <!-- Card header -->
        <div class="panel-head">
          <span class="panel-title">{{ item.coffee.name }}</span>
          <button
            class="btn btn-sm"
            (click)="removeFromShelf(item)"
            [disabled]="removingIds.has(item.coffeeId)"
            [attr.aria-label]="'Remove ' + item.coffee.name + ' from shelf'"
            style="min-width:44px; min-height:44px;">
            {{ removingIds.has(item.coffeeId) ? 'Removing…' : 'Remove' }}
          </button>
        </div>

        <div class="panel-body">
          <!-- Coffee metadata -->
          <div style="margin-bottom:14px;">
            <div style="font-size:13px; color:#666; margin-bottom:6px;">
              {{ item.coffee.roaster || 'Unknown Roaster' }}
            </div>
            <div style="display:flex; flex-wrap:wrap; gap:6px; align-items:center;">
              <span class="status-pill" aria-label="Processing method">{{ item.coffee.processingMethod }}</span>
              <span class="status-pill" aria-label="Roast level">{{ roastLabel(item.coffee.roastLevel) }}</span>
            </div>
          </div>

          <!-- Bag tracking section -->
          <section [attr.aria-label]="'Bag tracking for ' + item.coffee.name">
            <div class="section-label">Bags</div>

            <!-- Open bags for this coffee -->
            <ng-container *ngIf="getOpenBagsForCoffee(item.coffeeId).length > 0; else noBags">
              <div
                *ngFor="let bag of getOpenBagsForCoffee(item.coffeeId)"
                style="border:1.5px solid var(--ink); padding:12px; margin-bottom:10px; background:var(--paper);"
                [attr.aria-label]="'Open bag opened on ' + (bag.openedAt | date:'mediumDate')">

                <div style="display:flex; justify-content:space-between; align-items:flex-start; gap:8px; flex-wrap:wrap;">
                  <div>
                    <div style="font-size:12px; color:#666; margin-bottom:6px;">
                      Opened {{ bag.openedAt | date:'mediumDate' }}
                      <span *ngIf="bag.roastedOn"> · Roasted {{ bag.roastedOn | date:'mediumDate' }}</span>
                      <span *ngIf="bag.bagWeightG"> · {{ bag.bagWeightG }}g</span>
                    </div>

                    <!-- Freshness pill -->
                    <ng-container *ngIf="freshnessMap.get(bag.id) as freshness">
                      <span
                        class="status-pill"
                        [class.s-active]="freshness.freshnessStatus === 'Peak'"
                        [class.s-hold]="freshness.freshnessStatus === 'Acceptable'"
                        [attr.aria-label]="'Freshness: ' + freshness.freshnessStatus">
                        {{ freshness.freshnessStatus }}
                      </span>
                      <span
                        *ngIf="freshness.isRunningLow"
                        style="font-size:11px; font-weight:700; color:#c00; margin-left:8px; letter-spacing:0.08em; text-transform:uppercase;"
                        role="alert"
                        aria-live="polite">
                        Running Low
                      </span>
                    </ng-container>
                    <ng-container *ngIf="freshnessLoadingIds.has(bag.id)">
                      <span style="font-size:11px; color:#666;">Loading freshness…</span>
                    </ng-container>
                  </div>

                  <button
                    class="btn btn-sm"
                    (click)="closeBag(bag)"
                    [disabled]="closingBagIds.has(bag.id)"
                    [attr.aria-label]="'Close bag opened on ' + (bag.openedAt | date:'mediumDate')"
                    style="min-width:44px; min-height:44px; flex-shrink:0;">
                    {{ closingBagIds.has(bag.id) ? 'Closing…' : 'Close Bag' }}
                  </button>
                </div>

                <div *ngIf="bag.notes" style="font-size:12px; color:#666; margin-top:8px; font-style:italic;">
                  {{ bag.notes }}
                </div>
              </div>
            </ng-container>

            <!-- No open bag state + open bag button -->
            <ng-template #noBags>
              <p style="font-size:12px; color:#999; margin:0 0 10px;">No open bag for this coffee.</p>
            </ng-template>

            <!-- Open Bag button (shown when no inline form active for this coffee) -->
            <div *ngIf="openBagFormCoffeeId !== item.coffeeId">
              <button
                class="btn btn-sm"
                (click)="showOpenBagForm(item.coffeeId)"
                [attr.aria-label]="'Open a new bag of ' + item.coffee.name"
                style="min-width:44px; min-height:44px;">
                Open Bag
              </button>
            </div>

            <!-- Inline Open Bag form -->
            <div
              *ngIf="openBagFormCoffeeId === item.coffeeId"
              style="border:1.5px solid var(--ink); padding:14px; background:var(--paper); margin-top:8px;"
              role="region"
              [attr.aria-label]="'Open new bag form for ' + item.coffee.name">

              <div class="section-label" style="margin-bottom:10px;">Open New Bag</div>

              <div class="form-group">
                <label class="form-label" [for]="'roasted-on-' + item.coffeeId">Roasted On</label>
                <input
                  [id]="'roasted-on-' + item.coffeeId"
                  type="date"
                  [(ngModel)]="openBagForm.roastedOn"
                  [attr.aria-label]="'Roast date for new bag of ' + item.coffee.name"
                  style="font-size:16px;">
              </div>

              <div class="form-group">
                <label class="form-label" [for]="'bag-weight-' + item.coffeeId">Bag Weight (g)</label>
                <input
                  [id]="'bag-weight-' + item.coffeeId"
                  type="number"
                  [(ngModel)]="openBagForm.bagWeightG"
                  min="1"
                  placeholder="e.g. 250"
                  [attr.aria-label]="'Bag weight in grams for ' + item.coffee.name"
                  style="font-size:16px;">
              </div>

              <div class="form-group">
                <label class="form-label" [for]="'bag-notes-' + item.coffeeId">Notes</label>
                <input
                  [id]="'bag-notes-' + item.coffeeId"
                  type="text"
                  [(ngModel)]="openBagForm.notes"
                  placeholder="Optional notes…"
                  [attr.aria-label]="'Notes for new bag of ' + item.coffee.name"
                  style="font-size:16px;">
              </div>

              <div style="display:flex; gap:8px; flex-wrap:wrap;">
                <button
                  class="btn btn-inv"
                  (click)="submitOpenBag(item.coffeeId)"
                  [disabled]="openingBag"
                  [attr.aria-label]="'Confirm open new bag of ' + item.coffee.name"
                  style="min-width:44px; min-height:44px;">
                  {{ openingBag ? 'Opening…' : 'Open' }}
                </button>
                <button
                  class="btn"
                  (click)="cancelOpenBagForm()"
                  [attr.aria-label]="'Cancel opening new bag of ' + item.coffee.name"
                  style="min-width:44px; min-height:44px;">
                  Cancel
                </button>
              </div>

              <div
                *ngIf="openBagError"
                class="error-msg"
                role="alert"
                style="margin-top:10px;">
                {{ openBagError }}
              </div>
            </div>
          </section>
        </div>
      </article>
    </div>
  `,
})
export class MyShelfComponent implements OnInit {
  private collectionService = inject(CollectionService);
  private coffeeService = inject(CoffeeService);

  shelfItems: UserCoffee[] = [];
  allCoffees: Coffee[] = [];
  bags: CoffeeBag[] = [];
  freshnessMap = new Map<number, FreshnessInfo>();

  loading = true;
  error: string | null = null;

  removingIds = new Set<number>();
  closingBagIds = new Set<number>();
  freshnessLoadingIds = new Set<number>();

  // Open bag inline form state
  openBagFormCoffeeId: number | null = null;
  openBagForm: OpenBagForm = { roastedOn: '', bagWeightG: null, notes: '' };
  openingBag = false;
  openBagError: string | null = null;

  ngOnInit(): void {
    forkJoin({
      shelf: this.collectionService.getShelf().pipe(
        catchError(() => of([] as UserCoffee[]))
      ),
      bags: this.collectionService.getBags().pipe(
        catchError(() => of([] as CoffeeBag[]))
      ),
      coffees: this.coffeeService.getAll().pipe(
        catchError(() => of([] as Coffee[]))
      ),
    }).subscribe({
      next: ({ shelf, bags, coffees }) => {
        this.shelfItems = shelf;
        this.bags = bags;
        this.allCoffees = coffees;
        this.loading = false;
        this.loadFreshnessForAllBags(bags);
      },
      error: () => {
        this.error = 'Failed to load your shelf. Please try again.';
        this.loading = false;
      },
    });
  }

  private loadFreshnessForAllBags(bags: CoffeeBag[]): void {
    if (bags.length === 0) return;

    bags.forEach(bag => this.freshnessLoadingIds.add(bag.id));

    const freshnessRequests = bags.reduce((acc, bag) => {
      acc[bag.id] = this.collectionService.getBagFreshness(bag.id).pipe(
        catchError(() => of(null as FreshnessInfo | null))
      );
      return acc;
    }, {} as Record<number, Observable<FreshnessInfo | null>>);

    forkJoin(freshnessRequests).subscribe(results => {
      Object.entries(results).forEach(([bagIdStr, info]) => {
        const bagId = Number(bagIdStr);
        this.freshnessLoadingIds.delete(bagId);
        if (info) {
          this.freshnessMap.set(bagId, info as FreshnessInfo);
        }
      });
    });
  }

  getOpenBagsForCoffee(coffeeId: number): CoffeeBag[] {
    return this.bags.filter(b => b.coffeeId === coffeeId);
  }

  removeFromShelf(item: UserCoffee): void {
    this.removingIds.add(item.coffeeId);
    this.collectionService.removeFromShelf(item.coffeeId).subscribe({
      next: () => {
        this.shelfItems = this.shelfItems.filter(s => s.coffeeId !== item.coffeeId);
        this.removingIds.delete(item.coffeeId);
      },
      error: () => {
        this.removingIds.delete(item.coffeeId);
        this.error = `Failed to remove ${item.coffee.name} from shelf.`;
      },
    });
  }

  closeBag(bag: CoffeeBag): void {
    this.closingBagIds.add(bag.id);
    this.collectionService.closeBag(bag.id).subscribe({
      next: () => {
        this.bags = this.bags.filter(b => b.id !== bag.id);
        this.freshnessMap.delete(bag.id);
        this.closingBagIds.delete(bag.id);
      },
      error: () => {
        this.closingBagIds.delete(bag.id);
        this.error = 'Failed to close bag. Please try again.';
      },
    });
  }

  showOpenBagForm(coffeeId: number): void {
    this.openBagFormCoffeeId = coffeeId;
    this.openBagForm = { roastedOn: '', bagWeightG: null, notes: '' };
    this.openBagError = null;
  }

  cancelOpenBagForm(): void {
    this.openBagFormCoffeeId = null;
    this.openBagError = null;
  }

  submitOpenBag(coffeeId: number): void {
    this.openingBag = true;
    this.openBagError = null;

    const req: OpenCoffeeBagRequest = {
      coffeeId,
      roastedOn: this.openBagForm.roastedOn || undefined,
      bagWeightG: this.openBagForm.bagWeightG ?? undefined,
      notes: this.openBagForm.notes || undefined,
    };

    this.collectionService.openBag(req).subscribe({
      next: (newBag) => {
        this.bags = [...this.bags, newBag];
        this.openBagFormCoffeeId = null;
        this.openingBag = false;

        // Load freshness for the newly opened bag
        this.freshnessLoadingIds.add(newBag.id);
        this.collectionService.getBagFreshness(newBag.id).pipe(
          catchError(() => of(null as FreshnessInfo | null))
        ).subscribe(info => {
          this.freshnessLoadingIds.delete(newBag.id);
          if (info) {
            this.freshnessMap.set(newBag.id, info);
          }
        });
      },
      error: () => {
        this.openingBag = false;
        this.openBagError = 'Failed to open bag. Please try again.';
      },
    });
  }

  roastLabel(level: number): string {
    if (level <= 1.5) return 'Light';
    if (level <= 2.5) return 'Lt-Med';
    if (level <= 3.5) return 'Medium';
    if (level <= 4.0) return 'Med-Dark';
    return 'Dark';
  }
}
