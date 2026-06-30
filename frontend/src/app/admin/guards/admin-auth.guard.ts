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
export class AdminAuthGuard
  implements CanMatch, CanActivate, CanActivateChild {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  private check(): boolean | UrlTree {

    const authenticated = this.authService.isAuthenticated();
    const admin = this.authService.isAdmin();

    console.log('----- ADMIN GUARD -----');
    console.log('Authenticated:', authenticated);
    console.log('Admin:', admin);

    if (!authenticated || !admin) {
      console.log('Redirect -> /login');
      return this.router.parseUrl('/login');
    }

    console.log('Access granted');

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
