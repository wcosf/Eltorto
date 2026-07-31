import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from '../core/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private _csrfRetrying = false;

  constructor(private authService: AuthService) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const headers: Record<string, string> = {};
    const token = this.authService.getToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const method = req.method;
    const url = req.url;
    if (['POST', 'PUT', 'DELETE', 'PATCH'].includes(method) &&
      !url.includes('/api/auth/login') &&
      !url.includes('/api/auth/register') &&
      !url.includes('/api/auth/refresh')) {
      const csrf = this.authService.getCsrfToken();
      if (csrf) {
        headers['X-CSRF-Token'] = csrf;
      }
    }

    const cloned = Object.keys(headers).length > 0
      ? req.clone({ setHeaders: headers })
      : req;

    return next.handle(cloned).pipe(
      catchError((error: HttpErrorResponse) => {
        if (
          error.status === 400 &&
          error.error?.error === 'CSRF token validation failed' &&
          !this._csrfRetrying
        ) {
          this._csrfRetrying = true;
          return this.authService.refreshToken().pipe(
            switchMap(() => {
              this._csrfRetrying = false;
              const retryHeaders: Record<string, string> = {};
              const newToken = this.authService.getToken();
              if (newToken) {
                retryHeaders['Authorization'] = `Bearer ${newToken}`;
              }
              const newCsrf = this.authService.getCsrfToken();
              if (newCsrf) {
                retryHeaders['X-CSRF-Token'] = newCsrf;
              }
              return next.handle(req.clone({ setHeaders: retryHeaders }));
            }),
            catchError(() => {
              this._csrfRetrying = false;
              return throwError(() => error);
            })
          );
        }
        return throwError(() => error);
      })
    );
  }
}
