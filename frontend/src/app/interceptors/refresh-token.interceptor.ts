import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { catchError, filter, take, switchMap } from 'rxjs/operators';
import { AuthService } from '../core/auth.service';

@Injectable()
export class RefreshTokenInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshSubject = new BehaviorSubject<boolean>(false);

  constructor(private authService: AuthService) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.url.includes('/api/auth/refresh') || req.url.includes('/api/auth/login')) {
      return next.handle(req);
    }

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          return this.handle401Error(req, next);
        }
        return throwError(() => error);
      })
    );
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshSubject.next(false);

      return this.authService.refreshToken().pipe(
        switchMap(() => {
          this.isRefreshing = false;
          this.refreshSubject.next(true);
          const token = this.authService.getToken();
          const headers: Record<string, string> = {};
          if (token) {
            headers['Authorization'] = `Bearer ${token}`;
          }
          return next.handle(request.clone({ setHeaders: headers }));
        }),
        catchError((err) => {
          this.isRefreshing = false;
          this.refreshSubject.next(false);
          this.authService.clearAuthState();
          return throwError(() => err);
        })
      );
    } else {
      return this.refreshSubject.pipe(
        filter(v => v),
        take(1),
        switchMap(() => {
          const token = this.authService.getToken();
          const headers: Record<string, string> = {};
          if (token) {
            headers['Authorization'] = `Bearer ${token}`;
          }
          return next.handle(request.clone({ setHeaders: headers }));
        })
      );
    }
  }
}
