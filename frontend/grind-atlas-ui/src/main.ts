// src/main.ts
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';
import { logError } from './app/utils/logger';

window.addEventListener('unhandledrejection', (event) => {
  logError('unhandledRejection', event.reason);
});

bootstrapApplication(AppComponent, appConfig).catch((err) =>
  logError('bootstrapApplication', err)
);
