import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';

interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiration: string;
  userName: string;
  roles: string[];
}

interface DecodedToken {
  exp: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'access_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly ROLES_KEY = 'user_roles';
  private readonly USERNAME_KEY = 'user_name';

  private apiUrl = '/api/auth';

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  private logoutTimer: any = null;

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: { userName: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        this.setSession(response);
      })
    );
  }

  logout(): void {
    this.stopLogoutTimer();
    const refreshToken = this.getRefreshToken();

    if (refreshToken) {
      this.http.post(`${this.apiUrl}/logout`, { refreshToken })
        .subscribe({
          next: () => {
            this.clearLocalData();
            this.router.navigate(['/login'], { replaceUrl: true });
          },
          error: () => {
            this.clearLocalData();
            this.router.navigate(['/login'], { replaceUrl: true });
          }
        });
    } else {
      this.clearLocalData();
      this.router.navigate(['/login'], { replaceUrl: true });
    }
  }

  setSession(authResult: any): void {
    localStorage.setItem(this.TOKEN_KEY, authResult.accessToken);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, authResult.refreshToken);
    localStorage.setItem(this.ROLES_KEY, JSON.stringify(authResult.roles));
    localStorage.setItem(this.USERNAME_KEY, authResult.userName);
    this.isAuthenticatedSubject.next(true);
    this.startLogoutTimer();
  }

  private clearLocalData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.ROLES_KEY);
    localStorage.removeItem(this.USERNAME_KEY);
    this.isAuthenticatedSubject.next(false);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getRoles(): string[] {
    const roles = localStorage.getItem(this.ROLES_KEY);
    return roles ? JSON.parse(roles) : [];
  }

  isAuthenticated(): boolean {
    return this.hasToken() && !this.isTokenExpired();
  }

  isAdmin(): boolean {
    return this.getRoles().includes('Admin');
  }

  getUsername(): string | null {
    return localStorage.getItem(this.USERNAME_KEY);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(this.TOKEN_KEY);
  }

  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh`, { refreshToken }).pipe(
      tap(response => {
        this.setSession(response);
      })
    );
  }

  validateToken(): Observable<boolean> {
    const token = this.getToken();
    if (!token || this.isTokenExpired()) {
      return of(false);
    }
    return this.http.get<boolean>(`${this.apiUrl}/auth/validate`).pipe(
      map(() => true),
      catchError(() => {
        this.clearLocalData();
        return of(false);
      })
    );
  }

  getTokenExpiration(): Date | null {
    const token = this.getToken();
    if (!token) return null;
    try {
      const decoded: DecodedToken = jwtDecode(token);
      return new Date(decoded.exp * 1000);
    } catch {
      return null;
    }
  }

  isTokenExpired(): boolean {
    const exp = this.getTokenExpiration();
    if (!exp) return true;
    return exp.getTime() < Date.now();
  }

  startLogoutTimer(): void {
    this.stopLogoutTimer();
    const exp = this.getTokenExpiration();
    if (!exp) return;
    const timeUntilExpiry = exp.getTime() - Date.now();
    const timeout = Math.max(timeUntilExpiry, 0);
    this.logoutTimer = setTimeout(() => {
      if (this.isAuthenticated() && this.isTokenExpired()) {
        this.logout();
      }
    }, timeout);
  }

  stopLogoutTimer(): void {
    if (this.logoutTimer) {
      clearTimeout(this.logoutTimer);
      this.logoutTimer = null;
    }
  }

  isTokenExpiringSoon(minutes: number = 5): boolean {
    const exp = this.getTokenExpiration();
    if (!exp) return true;
    const timeLeft = exp.getTime() - Date.now();
    return timeLeft < minutes * 60 * 1000;
  }
}
