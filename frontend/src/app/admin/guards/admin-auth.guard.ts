import { Injectable } from '@angular/core';

import {
  ActivatedRouteSnapshot,
  CanActivate,
  CanActivateChild,
  CanMatch,
  Route,
  Router,
  RouterStateSnapshot,
  UrlSegment,
  UrlTree
} from '@angular/router';

import { Observable, of } from 'rxjs';

import { AuthService } from '../../core/auth.service';

@Injectable({
  providedIn: 'root'
})

export class AdminAuthGuard implements CanMatch, CanActivate, CanActivateChild {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  private check(): boolean | UrlTree {
    const authenticated = this.authService.isAuthenticated();
    const isAdmin = this.authService.isAdmin();
    const tokenExpired = this.authService.isTokenExpired();

    if (!authenticated || !isAdmin || tokenExpired) {
      if (tokenExpired) {
        this.authService.logout();
      }
      return this.router.parseUrl('/login');
    }

    return true;
  }

  canMatch(
    route: Route,
    segments: UrlSegment[]
  ): Observable<boolean | UrlTree> {

    return of(this.check());
  }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> {

    return of(this.check());
  }

  canActivateChild(
    childRoute: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> {

    return of(this.check());
  }

}
