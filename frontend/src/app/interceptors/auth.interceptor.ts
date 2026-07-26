import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../core/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
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
      !url.includes('/api/auth/register')) {
      const csrf = this.authService.getCsrfToken();
      if (csrf) {
        headers['X-CSRF-Token'] = csrf;
      }
    }

    if (Object.keys(headers).length > 0) {
      const cloned = req.clone({ setHeaders: headers });
      return next.handle(cloned);
    }

    return next.handle(req);
  }
}
