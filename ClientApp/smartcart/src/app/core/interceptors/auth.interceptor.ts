import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { from, switchMap, catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const auth = inject(AuthService);

  // Skip auth endpoints
  if (req.url.includes('/api/auth/')) {
    return next(req);
  }

  // Skip anonymous user endpoints
  const anonymousEndpoints = ['/api/user/send-otp', '/api/user/verify-otp', '/api/user/upsert'];
  if (anonymousEndpoints.some(e => req.url.includes(e))) {
    return next(req);
  }

  return from(auth.ensureAuthenticated()).pipe(
    switchMap(token => {
      const authReq = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` },
      });
      return next(authReq);
    }),
    catchError((err: HttpErrorResponse) => throwError(() => err))
  );
};
