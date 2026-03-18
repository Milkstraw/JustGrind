import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { RecipeService } from '../../services/services';
import { BrewRecipe, BREW_METHOD_LABELS, BrewMethod } from '../../models/models';

@Component({
  selector: 'app-recipes',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div style="display:flex; align-items:baseline; justify-content:space-between; margin-bottom:24px;">
      <h1 class="page-title" style="margin-bottom:0;">Brew Recipes</h1>
      <a routerLink="/recipes/new" class="btn">New Recipe</a>
    </div>

    <div class="section-label">{{ recipes.length }} recipe{{ recipes.length !== 1 ? 's' : '' }}</div>

    <div class="panel" *ngIf="recipes.length > 0">
      <div style="overflow-x: auto;">
        <table class="table" style="min-width: 700px;">
          <thead>
            <tr>
              <th>Name</th>
              <th>Coffee</th>
              <th>Grinder</th>
              <th>Method</th>
              <th>Steps</th>
              <th>Total Time</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let r of recipes">
              <td><strong>{{ r.name }}</strong></td>
              <td>{{ r.coffee?.name ?? 'Coffee #' + r.coffeeId }}</td>
              <td>{{ r.grinder?.brand ?? '' }} {{ r.grinder?.model ?? '#' + r.grinderId }}</td>
              <td>{{ formatMethod(r.brewMethod) }}</td>
              <td>
                <span class="status-pill s-hold">{{ r.steps.length }} step{{ r.steps.length !== 1 ? 's' : '' }}</span>
              </td>
              <td>{{ formatTotalTime(r) }}</td>
              <td>
                <div style="display:flex; gap:8px;">
                  <a [routerLink]="['/recipes', r.id, 'brew']" class="btn btn-inv btn-sm">Brew →</a>
                  <a [routerLink]="['/recipes', r.id, 'edit']" class="btn btn-sm">Edit</a>
                  <button class="btn btn-sm" (click)="confirmDelete(r)">Delete</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="panel" *ngIf="recipes.length === 0">
      <div class="panel-body" style="text-align:center; color:#999; padding:40px 20px;">
        <div style="font-size:13px; margin-bottom:14px;">No recipes yet. Create your first brew recipe.</div>
        <a routerLink="/recipes/new" class="btn btn-inv">New Recipe</a>
      </div>
    </div>

    <!-- Delete confirmation overlay -->
    <div *ngIf="pendingDelete" style="
      position:fixed; inset:0; background:rgba(10,10,10,0.6);
      display:flex; align-items:center; justify-content:center; z-index:100;">
      <div class="panel" style="width:380px; background:#fff;">
        <div class="panel-head"><span class="panel-title">Confirm Delete</span></div>
        <div class="panel-body">
          <p style="margin:0 0 20px; font-size:13px;">
            Delete <strong>{{ pendingDelete.name }}</strong>? This cannot be undone.
          </p>
          <div style="display:flex; gap:10px;">
            <button class="btn btn-inv btn-sm" (click)="deleteRecipe()">Delete</button>
            <button class="btn btn-sm" (click)="pendingDelete = null">Cancel</button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class RecipesComponent implements OnInit {
  private recipeService = inject(RecipeService);

  recipes: BrewRecipe[] = [];
  pendingDelete: BrewRecipe | null = null;

  ngOnInit() {
    this.recipeService.getAll().subscribe(r => this.recipes = r);
  }

  formatMethod(m: string): string {
    return BREW_METHOD_LABELS[m as BrewMethod] ?? m;
  }

  formatTotalTime(r: BrewRecipe): string {
    const total = r.steps.reduce((sum, s) => sum + s.durationS, 0);
    if (total === 0) return '—';
    const m = Math.floor(total / 60);
    const s = total % 60;
    return m > 0 ? `${m}m ${s > 0 ? s + 's' : ''}`.trim() : `${s}s`;
  }

  confirmDelete(r: BrewRecipe) {
    this.pendingDelete = r;
  }

  deleteRecipe() {
    if (!this.pendingDelete) return;
    const id = this.pendingDelete.id;
    this.pendingDelete = null;
    this.recipeService.delete(id).subscribe(() => {
      this.recipes = this.recipes.filter(r => r.id !== id);
    });
  }
}
