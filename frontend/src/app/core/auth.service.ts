import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';

interface UserSession {
  accessToken: string;
  expiration: string;
  userName: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/auth';

  private _accessToken: string | null = null;
  private _userName: string | null = null;
  private _roles: string[] = [];

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) { }

  login(credentials: { userName: string; password: string }): Observable<UserSession> {
    return this.http.post<UserSession>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => this.setSession(response))
    );
  }

  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}).subscribe({
      next: () => {
        this.clearAuthState();
        this.router.navigate(['/login'], { replaceUrl: true });
      },
      error: () => {
        this.clearAuthState();
        this.router.navigate(['/login'], { replaceUrl: true });
      }
    });
  }

  refreshToken(): Observable<UserSession> {
    return this.http.post<UserSession>(`${this.apiUrl}/refresh`, {}).pipe(
      tap(response => this.setSession(response))
    );
  }

  private setSession(session: UserSession): void {
    this._accessToken = session.accessToken;
    this._userName = session.userName;
    this._roles = session.roles;
    this.isAuthenticatedSubject.next(true);
  }

  clearAuthState(): void {
    this._accessToken = null;
    this._userName = null;
    this._roles = [];
    this.isAuthenticatedSubject.next(false);
  }

  getToken(): string | null {
    return this._accessToken;
  }

  getRoles(): string[] {
    return this._roles;
  }

  getUsername(): string | null {
    return this._userName;
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  isAdmin(): boolean {
    return this._roles.includes('Admin');
  }

  getCsrfToken(): string | null {
    const match = document.cookie.match(/(?:^|;\s*)XSRF-TOKEN=([^;]*)/);
    return match ? decodeURIComponent(match[1]) : null;
  }

  validateToken(): Observable<boolean> {
    if (!this._accessToken) {
      return this.refreshToken().pipe(
        map(() => true),
        catchError(() => {
          this.clearAuthState();
          return of(false);
        })
      );
    }

    return this.http.get<{ userName: string; roles: string[] }>(`${this.apiUrl}/me`).pipe(
      map(() => true),
      catchError(() => {
        return this.refreshToken().pipe(
          map(() => true),
          catchError(() => {
            this.clearAuthState();
            return of(false);
          })
        );
      })
    );
  }

  initializeAuthState(): void {
    this.refreshToken().subscribe({
      next: () => { },
      error: () => this.clearAuthState()
    });
  }

  changePassword(data: { currentPassword: string; newPassword: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, data);
  }

  changeUserName(data: { newUserName: string; password: string }): Observable<UserSession> {
    return this.http.post<UserSession>(`${this.apiUrl}/change-username`, data).pipe(
      tap(response => this.setSession(response))
    );
  }
}
