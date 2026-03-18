import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../services/services';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <h2>Sign in to Grind Atlas</h2>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="current-password" />
          </mat-form-field>

          @if (error) {
            <p class="auth-error">{{ error }}</p>
          }

          <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || loading">
            {{ loading ? 'Signing in…' : 'Sign in' }}
          </button>
        </form>
        <p class="auth-link">Don't have an account? <a routerLink="/register">Create one</a></p>
      </div>
    </div>
  `,
  styles: [`
    .auth-page { display: flex; justify-content: center; align-items: center; min-height: 60vh; }
    .auth-card { width: 360px; display: flex; flex-direction: column; gap: 8px; }
    h2 { margin-bottom: 8px; }
    form { display: flex; flex-direction: column; gap: 4px; }
    mat-form-field { width: 100%; }
    button { margin-top: 8px; }
    .auth-error { color: #c62828; font-size: 0.875rem; margin: 0; }
    .auth-link { margin-top: 12px; font-size: 0.875rem; }
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
      next: () => this.router.navigate(['/logs']),
      error: () => {
        this.error = 'Invalid email or password.';
        this.loading = false;
      },
    });
  }
}
