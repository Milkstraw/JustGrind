# Grind Atlas

## Project Overview
A full-stack coffee grind tracking and estimation web app.
- **Backend**: .NET 8 Web API with EF Core InMemory — `backend/GrindAtlas.API/`
- **Frontend**: Angular 17 standalone components with Angular Material — `frontend/grind-atlas-ui/`

## Run Commands
```bash
# Backend (port 5000)
cd backend/GrindAtlas.API
dotnet run

# Frontend (port 4200)
cd frontend/grind-atlas-ui
npm install
ng serve

# Swagger UI
open http://localhost:5000/swagger
```

## Architecture

### Backend
| File | Purpose |
|------|---------|
| `Models/Enums.cs` | ProcessingMethod, BrewMethod, BurrType, GrindType, Species |
| `Models/Coffee.cs` | Coffee entity with all origin/roast fields |
| `Models/Grinder.cs` | Grinder + GrinderCalibration entities |
| `Models/GrindLog.cs` | GrindLog + BrewRecipe entities |
| `Models/GrindEstimate.cs` | Estimator audit log |
| `Data/AppDbContext.cs` | EF Core InMemory context |
| `Data/SeedData.cs` | 60 coffees, 10 grinders, calibrations, 102 grind logs |
| `Services/GrindEstimatorService.cs` | 4-layer inference engine |
| `Controllers/Controllers.cs` | All 4 controllers in one file |
| `DTOs/Dtos.cs` | Request/response records |

### Frontend
| File | Purpose |
|------|---------|
| `src/app/models/models.ts` | All TypeScript interfaces and enums |
| `src/app/services/services.ts` | CoffeeService, GrinderService, GrindLogService, EstimatorService |
| `src/app/app.routes.ts` | Lazy-loaded routes |
| `src/app/components/estimator/` | Main estimator UI — the flagship feature |
| `src/app/components/coffees/` | Coffee catalog with filters |
| `src/app/components/grinders/` | Grinder catalog with NGI calibration table |
| `src/app/components/grind-logs/` | Log list + add-log form |

## Key Concepts

### NGI (Normalized Grind Index)
A 0–100 internal scale that bridges all grinder native settings.
- 0–15: Espresso range
- 30–45: AeroPress / Moka
- 45–60: Pour Over / V60 / Chemex
- 75–85: French Press
- 88–95: Cold Brew

Conversion between NGI and native settings uses linear interpolation over the `grinder_calibrations` anchor points in `GrindEstimatorService.NgiToNative()` and `NativeToNgi()`.

### Estimator — 4-Layer Inference Pipeline
Located in `Services/GrindEstimatorService.cs`:
1. **Layer 1** — Direct hit: exact coffee + grinder + brew method in logs. Confidence: ~0.95
2. **Layer 2** — Cross-grinder: same coffee on other grinders, bridged via NGI. Confidence: ~0.82
3. **Layer 3** — Similar coffee, same grinder. Confidence: 0.50–0.80 depending on similarity
4. **Layer 4** — Similar coffee, any grinder. Confidence: 0.30–0.55

### Coffee Similarity Weights
| Factor | Weight |
|--------|--------|
| Roast Level | 35% |
| Processing Method | 20% |
| Origin Region | 15% |
| Elevation (masl) | 15% |
| Variety/Species | 10% |
| Roast Freshness | 5% |

## API Endpoints
```
GET    /api/coffees?search=&processing=&minRoast=&maxRoast=&origin=
GET    /api/coffees/{id}
POST   /api/coffees

GET    /api/grinders
GET    /api/grinders/{id}
GET    /api/grinders/{id}/calibrations

GET    /api/grindlogs?coffeeId=&grinderId=
POST   /api/grindlogs

POST   /api/estimator/estimate        { coffeeId, targetGrinderId, brewMethod }
POST   /api/estimator/estimate/{id}/confirm  { confirmedSetting }
GET    /api/estimator/similarity?coffeeAId=&coffeeBId=
```

## Conventions
- All new backend models go in `Models/`
- All new controllers go in `Controllers/Controllers.cs` (or their own file)
- Angular services use `inject()` pattern, NOT constructor injection
- BrewMethod is always a string enum in both C# and TypeScript
- NGI values should always be stored with 2 decimal places
- When adding new coffees to seed data, compute NGI via interpolation from the calibration table

### Accessibility & Responsiveness (required on every component)
All frontend components must follow **WCAG 2.1 AA** and be usable on both desktop and mobile. Full rules are in `STYLE.md` §17–18. The non-negotiable minimums:
- Use semantic HTML (`<button>`, `<nav>`, `<main>`, `<label>`) — never `<div>`/`<span>` as interactive targets
- Every interactive element needs an accessible name (`aria-label` for icon-only actions, `<label>` for all inputs)
- Add `aria-expanded`, `aria-live`, `role="progressbar"` where appropriate (timers, toggles, overlays)
- Restore focus visibility: `:focus-visible { outline: 2px solid var(--ink); outline-offset: 2px; }`
- Minimum touch target size: **44×44px** — pad `.btn-sm` elements on mobile
- Form inputs must be `font-size: 16px` on mobile to prevent iOS auto-zoom
- All grids (`.stats-row`, `.grid-2`, `.form-grid-3`) collapse to 1–2 columns on `≤ 767px`
- Sidebar becomes a fixed bottom nav bar on mobile (see STYLE.md §18)
- Never use hover-only interactions — all affordances must work on touch

## Pending / Next Steps
- Angular Material theme is coffee-brown (`#5d4037`) primary
- Estimator feedback loop: after user dials in the real setting, call `POST /api/estimator/estimate/{id}/confirm`
- Add a similarity explorer page (compare any two coffees side-by-side)
- Add brew recipe CRUD (the BrewRecipe model is seeded but no UI exists yet)
- Consider adding Chart.js for NGI distribution visualization on the grinder detail page
