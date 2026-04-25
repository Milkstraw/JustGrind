import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../services/services';
import { AdminAuditLog } from '../../models/models';

@Component({
  selector: 'app-audit-log',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="audit-page">
      <header class="audit-header">
        <h1 class="audit-title">Audit Log</h1>
        <p class="audit-subtitle">All admin actions, most recent first.</p>
      </header>

      <!-- Filters -->
      <section class="filters" aria-label="Filter audit log">
        <div class="filter-row">
          <div class="field">
            <label class="field__label" for="filter-actor">Actor</label>
            <input id="filter-actor" class="field__input" type="text"
              placeholder="email contains…"
              [(ngModel)]="filterActor"
              (ngModelChange)="onFilterChange()" />
          </div>
          <div class="field">
            <label class="field__label" for="filter-action">Action</label>
            <input id="filter-action" class="field__input" type="text"
              placeholder="action contains…"
              [(ngModel)]="filterAction"
              (ngModelChange)="onFilterChange()" />
          </div>
          <div class="field">
            <label class="field__label" for="filter-entity">Entity Type</label>
            <select id="filter-entity" class="field__input"
              [(ngModel)]="filterEntityType"
              (ngModelChange)="onFilterChange()">
              <option value="">All</option>
              <option value="User">User</option>
              <option value="GrindLog">GrindLog</option>
              <option value="Coffee">Coffee</option>
              <option value="Grinder">Grinder</option>
            </select>
          </div>
          <div class="field">
            <label class="field__label" for="filter-from">From</label>
            <input id="filter-from" class="field__input" type="date"
              [(ngModel)]="filterFrom"
              (ngModelChange)="onFilterChange()" />
          </div>
          <div class="field">
            <label class="field__label" for="filter-to">To</label>
            <input id="filter-to" class="field__input" type="date"
              [(ngModel)]="filterTo"
              (ngModelChange)="onFilterChange()" />
          </div>
          <button class="btn btn--ghost" type="button" (click)="resetFilters()">
            Clear
          </button>
        </div>
      </section>

      <!-- State: loading -->
      @if (loading()) {
        <p class="state-msg" role="status" aria-live="polite">Loading…</p>
      }

      <!-- State: error -->
      @if (error()) {
        <p class="state-msg state-msg--error" role="alert">{{ error() }}</p>
      }

      <!-- Table -->
      @if (!loading() && !error()) {
        <div class="table-wrap" role="region" aria-label="Audit log entries" tabindex="0">
          <table class="audit-table">
            <caption class="sr-only">Admin audit log — {{ total() }} entries</caption>
            <thead>
              <tr>
                <th scope="col">Timestamp</th>
                <th scope="col">Actor</th>
                <th scope="col">Action</th>
                <th scope="col">Entity</th>
                <th scope="col">Entity ID</th>
                <th scope="col">Notes</th>
              </tr>
            </thead>
            <tbody>
              @for (entry of entries(); track entry.id) {
                <tr>
                  <td class="mono">{{ formatTs(entry.timestamp) }}</td>
                  <td>{{ entry.actorEmail }}</td>
                  <td><span class="pill">{{ entry.action }}</span></td>
                  <td>{{ entry.entityType }}</td>
                  <td class="mono">{{ entry.entityId ?? '—' }}</td>
                  <td>{{ entry.notes ?? '—' }}</td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="6" class="empty-row">No entries match the current filters.</td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        @if (total() > pageSize) {
          <nav class="pagination" aria-label="Audit log pagination">
            <button class="btn btn--ghost" type="button"
              [disabled]="currentPage() === 1"
              (click)="goToPage(currentPage() - 1)"
              aria-label="Previous page">
              &larr; Prev
            </button>
            <span class="pagination__info" aria-live="polite">
              Page {{ currentPage() }} of {{ totalPages() }}
              ({{ total() }} total)
            </span>
            <button class="btn btn--ghost" type="button"
              [disabled]="currentPage() === totalPages()"
              (click)="goToPage(currentPage() + 1)"
              aria-label="Next page">
              Next &rarr;
            </button>
          </nav>
        }
      }
    </div>
  `,
  styles: [`
    .audit-page { max-width: 1200px; }

    .audit-header { margin-bottom: 24px; }
    .audit-title  { font-size: 1.5rem; font-weight: 700; margin: 0 0 4px; }
    .audit-subtitle { font-size: 0.875rem; opacity: 0.6; margin: 0; }

    /* Filters */
    .filters { margin-bottom: 20px; }
    .filter-row {
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
      align-items: flex-end;
    }
    .field { display: flex; flex-direction: column; gap: 4px; }
    .field__label {
      font-size: 0.6875rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }
    .field__input {
      font-family: 'Courier New', monospace;
      font-size: 0.875rem;
      padding: 6px 10px;
      border: 1.5px solid var(--ink);
      border-right-width: 4px;
      border-bottom-width: 4px;
      background: var(--paper);
      color: var(--ink);
      min-width: 160px;
    }
    .field__input:focus-visible {
      outline: 2px solid var(--ink);
      outline-offset: 2px;
    }

    /* Buttons */
    .btn {
      font-family: 'Courier New', monospace;
      font-size: 0.875rem;
      font-weight: 700;
      padding: 7px 16px;
      cursor: pointer;
      border: 1.5px solid var(--ink);
      border-right-width: 4px;
      border-bottom-width: 4px;
    }
    .btn--ghost { background: var(--paper); color: var(--ink); }
    .btn--ghost:hover { background: var(--mid); }
    .btn:disabled { opacity: 0.4; cursor: not-allowed; }
    .btn:focus-visible { outline: 2px solid var(--ink); outline-offset: 2px; }

    /* Table */
    .table-wrap {
      overflow-x: auto;
      border: 1.5px solid var(--ink);
      border-right-width: 4px;
      border-bottom-width: 4px;
      margin-bottom: 16px;
    }
    .audit-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
    .audit-table th {
      background: var(--ink);
      color: var(--paper);
      text-align: left;
      padding: 10px 12px;
      font-size: 0.6875rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      white-space: nowrap;
    }
    .audit-table td {
      padding: 9px 12px;
      border-bottom: 1px solid var(--mid);
      vertical-align: top;
    }
    .audit-table tr:last-child td { border-bottom: none; }
    .audit-table tr:hover td { background: var(--mid); }

    .mono { font-family: 'Courier New', monospace; font-size: 0.8125rem; }

    .pill {
      display: inline-block;
      padding: 2px 8px;
      border: 1.5px solid var(--ink);
      font-size: 0.75rem;
      font-weight: 700;
      white-space: nowrap;
    }

    .empty-row { text-align: center; padding: 24px; opacity: 0.5; }

    /* State messages */
    .state-msg { padding: 16px 0; opacity: 0.6; }
    .state-msg--error { color: var(--ink); font-weight: 700; opacity: 1; }

    /* Pagination */
    .pagination {
      display: flex;
      align-items: center;
      gap: 16px;
    }
    .pagination__info { font-size: 0.875rem; opacity: 0.7; }

    /* Accessibility */
    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      overflow: hidden;
      clip: rect(0,0,0,0);
      white-space: nowrap;
    }

    @media (max-width: 767px) {
      .filter-row { flex-direction: column; }
      .field__input { min-width: unset; width: 100%; font-size: 16px; }
    }
  `],
})
export class AuditLogComponent {
  private adminService = inject(AdminService);

  entries    = signal<AdminAuditLog[]>([]);
  total      = signal(0);
  loading    = signal(false);
  error      = signal<string | null>(null);
  currentPage = signal(1);
  readonly pageSize = 50;

  totalPages = computed(() => Math.max(1, Math.ceil(this.total() / this.pageSize)));

  filterActor      = '';
  filterAction     = '';
  filterEntityType = '';
  filterFrom       = '';
  filterTo         = '';

  constructor() {
    this.load();
  }

  onFilterChange(): void {
    this.currentPage.set(1);
    this.load();
  }

  goToPage(page: number): void {
    this.currentPage.set(page);
    this.load();
  }

  resetFilters(): void {
    this.filterActor      = '';
    this.filterAction     = '';
    this.filterEntityType = '';
    this.filterFrom       = '';
    this.filterTo         = '';
    this.currentPage.set(1);
    this.load();
  }

  formatTs(ts: string): string {
    return new Date(ts).toLocaleString(undefined, {
      dateStyle: 'short',
      timeStyle: 'short',
    });
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.adminService.getAuditLog({
      actor:      this.filterActor      || undefined,
      action:     this.filterAction     || undefined,
      entityType: this.filterEntityType || undefined,
      from:       this.filterFrom       || undefined,
      to:         this.filterTo         || undefined,
      page:       this.currentPage(),
      pageSize:   this.pageSize,
    }).subscribe({
      next: page => {
        this.entries.set(page.items);
        this.total.set(page.total);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load audit log. Please try again.');
        this.loading.set(false);
      },
    });
  }
}
