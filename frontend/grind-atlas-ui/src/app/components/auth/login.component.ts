import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/services';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <h2>Sign in to Grind Atlas</h2>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="form-group">
            <label class="form-label">Email</label>
            <input type="email" formControlName="email" autocomplete="email" placeholder="you@example.com" />
          </div>

          <div class="form-group">
            <label class="form-label">Password</label>
            <input type="password" formControlName="password" autocomplete="current-password" placeholder="••••••••" />
          </div>

          @if (error) {
            <p class="auth-error">{{ error }}</p>
          }

          <button class="btn btn-inv auth-submit" type="submit" [disabled]="form.invalid || loading">
            {{ loading ? 'Signing in…' : 'Sign in' }}
          </button>
        </form>
        <p class="auth-link">Don't have an account? <a routerLink="/register">Create one</a></p>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      display: flex; justify-content: center; align-items: center;
      min-height: 80vh; padding: 40px 24px;
    }
    .auth-card {
      width: min(420px, 100%);
      display: flex; flex-direction: column; gap: 0;
    }
    h2 { margin-bottom: 28px; font-size: 1.4rem; }
    .auth-submit { width: 100%; margin-top: 8px; padding: 14px; font-size: 0.9rem; }
    .auth-error { color: #c62828; font-size: 0.875rem; margin: 0 0 12px; }
    .auth-link { margin-top: 20px; font-size: 0.875rem; }
    .auth-link a { color: var(--ink); }
    input { font-size: 1rem; padding: 13px 14px; }
  `],
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  error = '';
  loading = false;

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/home']),
      error: () => {
        this.error = 'Invalid email or password.';
        this.loading = false;
      },
    });
  }
}
