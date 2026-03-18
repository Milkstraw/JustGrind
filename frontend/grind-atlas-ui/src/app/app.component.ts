import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './services/services';
import { filter } from 'rxjs/operators';

const SHELL_HIDDEN_ROUTES = ['/', '/login', '/register', '/not-found'];

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-shell">
      @if (showShell()) {
        <div class="topbar">
          <span class="topbar-brand">Grind Atlas</span>
          <div class="topbar-user">
            @if (auth.isLoggedIn()) {
              <span class="user-email">{{ auth.currentUser()?.displayName || auth.currentUser()?.email }}</span>
              <button class="logout-btn" (click)="logout()">Sign out</button>
            } @else {
              <a class="topbar-link" routerLink="/login">Sign in</a>
              <a class="topbar-link topbar-link--primary" routerLink="/register">Create account</a>
            }
          </div>
        </div>
      }
      <div class="layout">
        @if (showShell()) {
          <nav class="sidebar">
            <div class="sidebar-section">Navigation</div>
            <a class="sidebar-item" routerLink="/home"      routerLinkActive="active">Home</a>
            <a class="sidebar-item" routerLink="/grind-advisor" routerLinkActive="active">Grind Advisor</a>
            <a class="sidebar-item" routerLink="/coffees"   routerLinkActive="active">Coffees</a>
            <a class="sidebar-item" routerLink="/grinders"  routerLinkActive="active">Grinders</a>
            <a class="sidebar-item" routerLink="/logs"      routerLinkActive="active">My Logs</a>
            <a class="sidebar-item" routerLink="/recipes"   routerLinkActive="active">Recipes</a>
          </nav>
        }
        <main [class]="showShell() ? 'main' : 'main-full'">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; height: 100vh; }
    .topbar-user { margin-left: auto; display: flex; align-items: center; gap: 12px; }
    .user-email { font-size: 0.875rem; opacity: 0.85; }
    .logout-btn {
      background: none; border: 1px solid rgba(255,255,255,0.5); color: inherit;
      padding: 4px 12px; border-radius: 4px; cursor: pointer; font-size: 0.875rem;
    }
    .logout-btn:hover { background: rgba(255,255,255,0.1); }
    .topbar-link { color: inherit; text-decoration: none; font-size: 0.875rem; opacity: 0.85; }
    .topbar-link--primary {
      background: rgba(255,255,255,0.2); padding: 4px 12px;
      border-radius: 4px; opacity: 1;
    }
    .topbar-link--primary:hover { background: rgba(255,255,255,0.3); }
    .main-full { flex: 1; overflow-y: auto; }
  `],
})
export class AppComponent implements OnInit {
  auth = inject(AuthService);
  private router = inject(Router);

  currentUrl = signal('/');
  showShell = computed(() => !SHELL_HIDDEN_ROUTES.includes(this.currentUrl()));

  ngOnInit(): void {
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(e => this.currentUrl.set((e as NavigationEnd).urlAfterRedirects));
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
  }
}
