import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-admin-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="admin-layout">
      <nav class="admin-nav" aria-label="Admin navigation">
        <div class="admin-nav__heading" aria-hidden="true">Admin</div>
        <a class="admin-nav__item" routerLink="/admin/users"     routerLinkActive="active">Users</a>
        <a class="admin-nav__item" routerLink="/admin/logs"      routerLinkActive="active">All Logs</a>
        <a class="admin-nav__item" routerLink="/admin/catalog"   routerLinkActive="active">Catalog</a>
        <a class="admin-nav__item" routerLink="/admin/analytics" routerLinkActive="active">Analytics</a>
        <a class="admin-nav__item" routerLink="/admin/audit-log" routerLinkActive="active">Audit Log</a>
      </nav>
      <main class="admin-main" id="admin-content" tabindex="-1">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    .admin-layout {
      display: flex;
      min-height: calc(100vh - 56px);
      background: var(--paper);
    }

    .admin-nav {
      width: 180px;
      flex-shrink: 0;
      border-right: 1.5px solid var(--ink);
      padding: 24px 0;
      display: flex;
      flex-direction: column;
    }

    .admin-nav__heading {
      font-size: 0.625rem;
      font-weight: 700;
      letter-spacing: 0.12em;
      text-transform: uppercase;
      color: var(--ink);
      opacity: 0.5;
      padding: 0 16px 12px;
      border-bottom: 1.5px solid var(--mid);
      margin-bottom: 8px;
    }

    .admin-nav__item {
      display: block;
      padding: 10px 16px;
      font-size: 0.875rem;
      font-weight: 400;
      color: var(--ink);
      text-decoration: none;
      border-left: 3px solid transparent;
    }
    .admin-nav__item:hover { background: var(--mid); }
    .admin-nav__item.active {
      font-weight: 700;
      border-left-color: var(--ink);
      background: var(--mid);
    }
    .admin-nav__item:focus-visible {
      outline: 2px solid var(--ink);
      outline-offset: -2px;
    }

    .admin-main {
      flex: 1;
      padding: 32px;
      overflow-y: auto;
    }

    @media (max-width: 767px) {
      .admin-layout { flex-direction: column; }

      .admin-nav {
        width: 100%;
        border-right: none;
        border-bottom: 1.5px solid var(--ink);
        flex-direction: row;
        flex-wrap: wrap;
        padding: 8px;
        gap: 4px;
      }

      .admin-nav__heading { display: none; }

      .admin-nav__item {
        border-left: none;
        border-bottom: 3px solid transparent;
        padding: 8px 12px;
        font-size: 0.8125rem;
      }
      .admin-nav__item.active {
        border-left-color: transparent;
        border-bottom-color: var(--ink);
      }

      .admin-main { padding: 16px; }
    }
  `],
})
export class AdminShellComponent {}
