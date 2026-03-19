import { Component, computed, inject, OnInit, signal, HostListener } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './services/services';
import { filter } from 'rxjs/operators';

const SHELL_HIDDEN_ROUTES = ['/', '/login', '/register', '/not-found'];

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <a class="skip-link" href="#main-content">Skip to main content</a>

    <div class="app-shell">
      @if (showShell()) {
        <header class="topbar" role="banner">
          <button
            class="burger-btn"
            [attr.aria-expanded]="sidebarOpen()"
            aria-controls="primary-nav"
            aria-label="Toggle navigation menu"
            (click)="toggleSidebar()">
            <span class="burger-line"></span>
            <span class="burger-line"></span>
            <span class="burger-line"></span>
          </button>
          <span class="topbar-brand" aria-hidden="true">Grind Atlas</span>
          <div class="topbar-user">
            @if (auth.isLoggedIn()) {
              <span class="user-email">{{ auth.currentUser()?.displayName || auth.currentUser()?.email }}</span>
              <button class="logout-btn" (click)="logout()">Sign out</button>
            } @else {
              <a class="topbar-link" routerLink="/login">Sign in</a>
              <a class="topbar-link topbar-link--primary" routerLink="/register">Create account</a>
            }
          </div>
        </header>
      }
      <div class="layout">
        @if (showShell()) {
          <!-- Backdrop for mobile overlay -->
          @if (sidebarOpen()) {
            <div
              class="sidebar-backdrop"
              aria-hidden="true"
              (click)="closeSidebar()">
            </div>
          }
          <nav
            id="primary-nav"
            class="sidebar"
            [class.sidebar--open]="sidebarOpen()"
            role="navigation"
            aria-label="Main navigation">
            <div class="sidebar-section" aria-hidden="true">Navigation</div>
            <a class="sidebar-item" routerLink="/home"         routerLinkActive="active" (click)="closeSidebar()">Home</a>
            <a class="sidebar-item" routerLink="/grind-advisor" routerLinkActive="active" (click)="closeSidebar()">Grind Advisor</a>
            <a class="sidebar-item" routerLink="/coffees"      routerLinkActive="active" (click)="closeSidebar()">Coffees</a>
            <a class="sidebar-item" routerLink="/grinders"     routerLinkActive="active" (click)="closeSidebar()">Grinders</a>
            <a class="sidebar-item" routerLink="/logs"         routerLinkActive="active" (click)="closeSidebar()">My Logs</a>
            <a class="sidebar-item" routerLink="/recipes"      routerLinkActive="active" (click)="closeSidebar()">Recipes</a>
          </nav>
        }
        <main id="main-content" [class]="showShell() ? 'main' : 'main-full'" tabindex="-1">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; height: 100vh; }

    /* Skip link */
    .skip-link {
      position: absolute;
      top: -100%;
      left: 0;
      z-index: 9999;
      background: var(--ink);
      color: var(--paper);
      padding: 8px 16px;
      font-size: 14px;
      text-decoration: none;
      font-weight: 700;
      letter-spacing: 0.05em;
    }
    .skip-link:focus {
      top: 0;
    }

    /* Topbar user area */
    .topbar-user { margin-left: auto; display: flex; align-items: center; gap: 12px; }
    .user-email { font-size: 0.875rem; opacity: 0.85; }
    .logout-btn {
      background: none; border: 1px solid rgba(255,255,255,0.5); color: inherit;
      padding: 4px 12px; cursor: pointer; font-size: 0.875rem;
    }
    .logout-btn:hover { background: rgba(255,255,255,0.1); }
    .topbar-link { color: inherit; text-decoration: none; font-size: 0.875rem; opacity: 0.85; }
    .topbar-link--primary {
      background: rgba(255,255,255,0.2); padding: 4px 12px; opacity: 1;
    }
    .topbar-link--primary:hover { background: rgba(255,255,255,0.3); }
    .main-full { flex: 1; overflow-y: auto; }

    /* Burger button — hidden on desktop */
    .burger-btn {
      display: none;
      flex-direction: column;
      justify-content: center;
      gap: 5px;
      background: none;
      border: none;
      cursor: pointer;
      padding: 6px 8px;
      margin-right: 12px;
      color: inherit;
      flex-shrink: 0;
    }
    .burger-line {
      display: block;
      width: 22px;
      height: 2px;
      background: var(--paper);
    }
    .burger-btn:hover .burger-line { background: rgba(245,243,238,0.75); }

    /* Sidebar backdrop */
    .sidebar-backdrop {
      display: none;
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.5);
      z-index: 99;
    }

    /* Mobile: show burger, hide sidebar by default, overlay when open */
    @media (max-width: 768px) {
      .burger-btn { display: flex; }

      .sidebar-backdrop { display: block; }
    }
  `],
})
export class AppComponent implements OnInit {
  auth = inject(AuthService);
  private router = inject(Router);

  currentUrl = signal('/');
  showShell = computed(() => !SHELL_HIDDEN_ROUTES.includes(this.currentUrl()));
  sidebarOpen = signal(false);

  ngOnInit(): void {
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(e => {
        this.currentUrl.set((e as NavigationEnd).urlAfterRedirects);
        this.sidebarOpen.set(false);
      });
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }

  toggleSidebar(): void {
    this.sidebarOpen.update(v => !v);
  }

  closeSidebar(): void {
    this.sidebarOpen.set(false);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.sidebarOpen()) {
      this.sidebarOpen.set(false);
    }
  }
}
