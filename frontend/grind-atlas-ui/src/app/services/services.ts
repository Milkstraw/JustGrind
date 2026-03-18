import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import {
  Coffee, Grinder, GrindLog, EstimateRequest, EstimateResponse,
  AddGrindLogRequest, BrewMethod, ProcessingMethod,
  AuthUser, LoginRequest, RegisterRequest,
} from '../models/models';

const BASE = 'http://localhost:5000/api';

// ── Coffee Service ────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class CoffeeService {
  private http = inject(HttpClient);

  getAll(filters?: {
    search?: string;
    processing?: ProcessingMethod;
    minRoast?: number;
    maxRoast?: number;
    origin?: string;
  }): Observable<Coffee[]> {
    let params = new HttpParams();
    if (filters?.search)     params = params.set('search', filters.search);
    if (filters?.processing) params = params.set('processing', filters.processing);
    if (filters?.minRoast != null) params = params.set('minRoast', filters.minRoast);
    if (filters?.maxRoast != null) params = params.set('maxRoast', filters.maxRoast);
    if (filters?.origin)     params = params.set('origin', filters.origin);
    return this.http.get<Coffee[]>(`${BASE}/coffees`, { params });
  }

  getById(id: number): Observable<Coffee> {
    return this.http.get<Coffee>(`${BASE}/coffees/${id}`);
  }

  create(coffee: Partial<Coffee>): Observable<Coffee> {
    return this.http.post<Coffee>(`${BASE}/coffees`, coffee);
  }

  update(id: number, coffee: Partial<Coffee>): Observable<Coffee> {
    return this.http.put<Coffee>(`${BASE}/coffees/${id}`, coffee);
  }
}

// ── Grinder Service ───────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class GrinderService {
  private http = inject(HttpClient);

  getAll(): Observable<Grinder[]> {
    return this.http.get<Grinder[]>(`${BASE}/grinders`);
  }

  getById(id: number): Observable<Grinder> {
    return this.http.get<Grinder>(`${BASE}/grinders/${id}`);
  }

  create(grinder: Partial<Grinder>): Observable<Grinder> {
    return this.http.post<Grinder>(`${BASE}/grinders`, grinder);
  }
}

// ── GrindLog Service ──────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class GrindLogService {
  private http = inject(HttpClient);

  getAll(filters?: { coffeeId?: number; grinderId?: number }): Observable<GrindLog[]> {
    let params = new HttpParams();
    if (filters?.coffeeId)  params = params.set('coffeeId', filters.coffeeId);
    if (filters?.grinderId) params = params.set('grinderId', filters.grinderId);
    return this.http.get<GrindLog[]>(`${BASE}/grindlogs`, { params });
  }

  create(req: AddGrindLogRequest): Observable<GrindLog> {
    return this.http.post<GrindLog>(`${BASE}/grindlogs`, req);
  }
}

// ── Auth Service ──────────────────────────────────────────────────────────────
const AUTH_KEY = 'grindatlas_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);

  private _user = signal<AuthUser | null>(
    JSON.parse(localStorage.getItem(AUTH_KEY) ?? 'null')
  );

  readonly currentUser = this._user.asReadonly();
  readonly isLoggedIn = computed(() => this._user() !== null);
  readonly token = computed(() => this._user()?.token ?? null);

  login(req: LoginRequest): Observable<AuthUser> {
    return this.http.post<AuthUser>(`${BASE}/auth/login`, req).pipe(
      tap(user => this.persist(user))
    );
  }

  register(req: RegisterRequest): Observable<AuthUser> {
    return this.http.post<AuthUser>(`${BASE}/auth/register`, req).pipe(
      tap(user => this.persist(user))
    );
  }

  logout(): void {
    localStorage.removeItem(AUTH_KEY);
    this._user.set(null);
  }

  private persist(user: AuthUser): void {
    localStorage.setItem(AUTH_KEY, JSON.stringify(user));
    this._user.set(user);
  }
}

// ── Estimator Service ─────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class EstimatorService {
  private http = inject(HttpClient);

  estimate(req: EstimateRequest): Observable<EstimateResponse> {
    return this.http.post<EstimateResponse>(`${BASE}/estimator/estimate`, req);
  }

  getSimilarity(coffeeAId: number, coffeeBId: number): Observable<{
    coffeeA: string; coffeeB: string; similarityScore: number;
  }> {
    const params = new HttpParams()
      .set('coffeeAId', coffeeAId)
      .set('coffeeBId', coffeeBId);
    return this.http.get<any>(`${BASE}/estimator/similarity`, { params });
  }

  confirmEstimate(estimateId: number, confirmedSetting: number): Observable<any> {
    return this.http.post(`${BASE}/estimator/estimate/${estimateId}/confirm`, {
      confirmedSetting,
    });
  }
}
