# ☕ Grind Atlas

A full-stack coffee grind tracker and estimator. Log your grind settings across different coffees and grinders, and let the estimator predict where to start dialing in on any coffee/grinder combination using a normalized grind index (NGI).

## Stack
- **Backend**: .NET 8 Web API · EF Core InMemory · Swagger
- **Frontend**: Angular 17 · Angular Material · Standalone Components

## Quick Start

### Backend
```bash
cd backend/GrindAtlas.API
dotnet run
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend
```bash
cd frontend/grind-atlas-ui
npm install
ng serve
# App: http://localhost:4200
```

## Features
- 📋 **Coffee Catalog** — 60 real-world coffees with origin, processing, elevation, tasting notes
- ⚙️ **Grinder Catalog** — 10 grinders with NGI calibration anchor points
- 🔢 **Grind Estimator** — 4-layer inference pipeline using weighted coffee similarity
- 📝 **Grind Logs** — Log and browse all your dial-in sessions
- 🔄 **NGI Bridge** — Cross-grinder normalization so any setting can be translated to any grinder

## Project Structure
```
grind-atlas/
├── CLAUDE.md                          ← Claude Code context file
├── README.md
├── backend/
│   └── GrindAtlas.API/
│       ├── GrindAtlas.API.csproj
│       ├── Program.cs
│       ├── Controllers/Controllers.cs
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── SeedData.cs            ← 60 coffees, 10 grinders, 102 logs
│       ├── DTOs/Dtos.cs
│       ├── Models/
│       │   ├── Coffee.cs
│       │   ├── Enums.cs
│       │   ├── GrindEstimate.cs
│       │   ├── GrindLog.cs
│       │   └── Grinder.cs
│       └── Services/
│           └── GrindEstimatorService.cs
└── frontend/
    └── grind-atlas-ui/
        ├── angular.json
        ├── package.json
        ├── tsconfig.json
        └── src/
            ├── index.html
            ├── main.ts
            ├── styles.scss
            └── app/
                ├── app.component.ts
                ├── app.config.ts
                ├── app.routes.ts
                ├── components/
                │   ├── coffees/
                │   ├── estimator/
                │   ├── grind-logs/
                │   └── grinders/
                ├── models/models.ts
                └── services/services.ts
```
