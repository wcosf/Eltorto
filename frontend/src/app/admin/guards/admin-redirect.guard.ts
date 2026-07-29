import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AdminStateService } from '../shared/services/admin-state.service';

@Injectable({
  providedIn: 'root'
})
export class AdminRedirectGuard implements CanActivate {
  constructor(
    private router: Router,
    private stateService: AdminStateService
  ) { }

  canActivate(): UrlTree {
    const lastRoute = this.stateService.lastVisitedRoute;
    return this.router.parseUrl(lastRoute || '/admin/cakes');
  }
}
