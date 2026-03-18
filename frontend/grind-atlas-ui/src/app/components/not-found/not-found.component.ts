import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/services';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="panel" style="max-width: 480px; margin: 80px auto;">
      <h2 style="margin-bottom: 8px;">404 — Page Not Found</h2>
      <p style="margin-bottom: 24px;">The page you're looking for doesn't exist.</p>
      <a class="btn btn-inv" routerLink="/home">Back to Home</a>
    </div>
  `,
})
export class NotFoundComponent implements OnInit {
  private auth = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    if (!this.auth.isLoggedIn()) {
      this.router.navigate(['/']);
    }
  }
}
