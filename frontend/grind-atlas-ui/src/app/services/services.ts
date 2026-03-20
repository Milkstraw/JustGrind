import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import {
  Coffee, Grinder, GrindLog, EstimateRequest, EstimateResponse,
  AddGrindLogRequest, BrewMethod, ProcessingMethod,
  AuthUser, LoginRequest, RegisterRequest,
  BrewRecipe, CreateBrewRecipeRequest,
  UserCoffee, UserGrinder, UserBrewMethod, CoffeeBag, OpenCoffeeBagRequest, FreshnessInfo,
} from '../models/models';

const BASE = 'https://grindatlas.onrender.com/api';

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

// ── Recipe Service ────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class RecipeService {
  private http = inject(HttpClient);

  getAll(): Observable<BrewRecipe[]> {
    return this.http.get<BrewRecipe[]>(`${BASE}/recipes`);
  }

  getById(id: number): Observable<BrewRecipe> {
    return this.http.get<BrewRecipe>(`${BASE}/recipes/${id}`);
  }

  create(req: CreateBrewRecipeRequest): Observable<BrewRecipe> {
    return this.http.post<BrewRecipe>(`${BASE}/recipes`, req);
  }

  update(id: number, req: CreateBrewRecipeRequest): Observable<BrewRecipe> {
    return this.http.put<BrewRecipe>(`${BASE}/recipes/${id}`, req);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${BASE}/recipes/${id}`);
  }
}

// ── Collection Service ────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class CollectionService {
  private http = inject(HttpClient);

  // My Shelf — coffees
  getShelf(): Observable<UserCoffee[]> {
    return this.http.get<UserCoffee[]>(`${BASE}/collection/coffees`);
  }

  addToShelf(coffeeId: number): Observable<void> {
    return this.http.post<void>(`${BASE}/collection/coffees/${coffeeId}`, {});
  }

  removeFromShelf(coffeeId: number): Observable<void> {
    return this.http.delete<void>(`${BASE}/collection/coffees/${coffeeId}`);
  }

  // My Setup — grinders
  getSetupGrinders(): Observable<UserGrinder[]> {
    return this.http.get<UserGrinder[]>(`${BASE}/collection/grinders`);
  }

  addGrinderToSetup(grinderId: number): Observable<void> {
    return this.http.post<void>(`${BASE}/collection/grinders/${grinderId}`, {});
  }

  removeGrinderFromSetup(grinderId: number): Observable<void> {
    return this.http.delete<void>(`${BASE}/collection/grinders/${grinderId}`);
  }

  // My Setup — brew methods
  getSetupBrewMethods(): Observable<UserBrewMethod[]> {
    return this.http.get<UserBrewMethod[]>(`${BASE}/collection/brewmethods`);
  }

  addBrewMethodToSetup(method: BrewMethod): Observable<void> {
    return this.http.post<void>(`${BASE}/collection/brewmethods/${method}`, {});
  }

  removeBrewMethodFromSetup(method: BrewMethod): Observable<void> {
    return this.http.delete<void>(`${BASE}/collection/brewmethods/${method}`);
  }

  // Bag Tracking
  getBags(): Observable<CoffeeBag[]> {
    return this.http.get<CoffeeBag[]>(`${BASE}/collection/bags`);
  }

  openBag(req: OpenCoffeeBagRequest): Observable<CoffeeBag> {
    return this.http.post<CoffeeBag>(`${BASE}/collection/bags`, req);
  }

  closeBag(bagId: number): Observable<void> {
    return this.http.delete<void>(`${BASE}/collection/bags/${bagId}`);
  }

  getBagFreshness(bagId: number): Observable<FreshnessInfo> {
    return this.http.get<FreshnessInfo>(`${BASE}/collection/bags/${bagId}/freshness`);
  }
}

// ── Grind Advisor Service ─────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class GrindAdvisorService {
  private http = inject(HttpClient);

  estimate(req: EstimateRequest): Observable<EstimateResponse> {
    return this.http.post<EstimateResponse>(`${BASE}/grind-advisor/estimate`, req);
  }

  getSimilarity(coffeeAId: number, coffeeBId: number): Observable<{
    coffeeA: string; coffeeB: string; similarityScore: number;
  }> {
    const params = new HttpParams()
      .set('coffeeAId', coffeeAId)
      .set('coffeeBId', coffeeBId);
    return this.http.get<any>(`${BASE}/grind-advisor/similarity`, { params });
  }

  confirmEstimate(estimateId: number, confirmedSetting: number): Observable<any> {
    return this.http.post(`${BASE}/grind-advisor/estimate/${estimateId}/confirm`, {
      confirmedSetting,
    });
  }
}
