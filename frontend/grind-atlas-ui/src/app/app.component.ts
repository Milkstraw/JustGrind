import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app-shell">
      <div class="topbar">
        <span class="topbar-brand">Grind Atlas</span>
      </div>
      <div class="layout">
        <nav class="sidebar">
          <div class="sidebar-section">Navigation</div>
          <a class="sidebar-item" routerLink="/home"     routerLinkActive="active">Home</a>
          <a class="sidebar-item" routerLink="/estimator" routerLinkActive="active">Estimator</a>
          <a class="sidebar-item" routerLink="/coffees"  routerLinkActive="active">Coffees</a>
          <a class="sidebar-item" routerLink="/grinders" routerLinkActive="active">Grinders</a>
          <a class="sidebar-item" routerLink="/logs"     routerLinkActive="active">Logs</a>
          <a class="sidebar-item" routerLink="/recipes"  routerLinkActive="active">Recipes</a>
        </nav>
        <main class="main">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; height: 100vh; }
  `],
})
export class AppComponent {}
