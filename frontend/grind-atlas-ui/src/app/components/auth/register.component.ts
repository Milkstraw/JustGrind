import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../services/services';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, RouterLink],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <h2>Create your account</h2>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <mat-form-field appearance="outline">
            <mat-label>Display name (optional)</mat-label>
            <input matInput formControlName="displayName" autocomplete="name" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" autocomplete="email" />
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password (min 6 characters)</mat-label>
            <input matInput type="password" formControlName="password" autocomplete="new-password" />
          </mat-form-field>

          @if (errors.length) {
            <ul class="auth-errors">
              @for (e of errors; track e) { <li>{{ e }}</li> }
            </ul>
          }

          <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid || loading">
            {{ loading ? 'Creating account…' : 'Create account' }}
          </button>
        </form>
        <p class="auth-link">Already have an account? <a routerLink="/login">Sign in</a></p>
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
    .auth-errors { color: #c62828; font-size: 0.875rem; margin: 4px 0; padding-left: 16px; }
    .auth-link { margin-top: 12px; font-size: 0.875rem; }
  `],
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.nonNullable.group({
    displayName: [''],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  errors: string[] = [];
  loading = false;

  submit(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.errors = [];
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/logs']),
      error: (err) => {
        this.errors = Array.isArray(err.error) ? err.error : ['Registration failed. Please try again.'];
        this.loading = false;
      },
    });
  }
}
