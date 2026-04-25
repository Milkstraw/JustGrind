import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { retry, timer } from 'rxjs';
import { logError } from './utils/logger';

// Retries GET requests up to 3 times with exponential backoff (2 s, 4 s, 6 s).
// Covers Render free-tier cold starts without risking duplicate writes on mutations.
export const retryInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.method !== 'GET') return next(req);

  return next(req).pipe(
    retry({
      count: 3,
      delay: (error, attempt) => {
        const status  = error instanceof HttpErrorResponse ? error.status : undefined;
        const isRetryable = !(status && status >= 400 && status < 500); // don't retry 4xx
        if (!isRetryable) throw error;
        logError('retryInterceptor', error, { url: req.urlWithParams, status });
        return timer(attempt * 2000);
      },
    })
  );
};
