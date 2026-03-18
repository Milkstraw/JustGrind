import { Component, inject } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './services/services';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-shell">
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
      <div class="layout">
        <nav class="sidebar">
          <div class="sidebar-section">Navigation</div>
          <a class="sidebar-item" routerLink="/home"      routerLinkActive="active">Home</a>
          <a class="sidebar-item" routerLink="/estimator" routerLinkActive="active">Estimator</a>
          <a class="sidebar-item" routerLink="/coffees"   routerLinkActive="active">Coffees</a>
          <a class="sidebar-item" routerLink="/grinders"  routerLinkActive="active">Grinders</a>
          <a class="sidebar-item" routerLink="/logs" routerLinkActive="active">My Logs</a>
        </nav>
        <main class="main">
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
  `],
})
export class AppComponent {
  auth = inject(AuthService);
  private router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/home']);
  }
}
