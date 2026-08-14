import { Component, HostListener, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenav } from '@angular/material/sidenav';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { AdminStateService } from '../../shared/services/admin-state.service';
import { ScrollToTopComponent } from '../../../shared/scroll-to-top/scroll-to-top.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    ScrollToTopComponent
  ],
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss']
})
export class AdminLayoutComponent implements OnInit, OnDestroy {

  menuItems = [
    { path: '/admin/cakes', icon: 'cake', label: 'Торты' },
    { path: '/admin/categories', icon: 'category', label: 'Категории' },
    { path: '/admin/fillings', icon: 'layers', label: 'Начинки' },
    { path: '/admin/testimonials', icon: 'comment', label: 'Отзывы' },
    { path: '/admin/orders', icon: 'shopping_cart', label: 'Заказы' },
    { path: '/admin/contacts', icon: 'contact_phone', label: 'Контакты' },
    { path: '/admin/security', icon: 'lock', label: 'Безопасность' }
  ];

  private destroy$ = new Subject<void>();

  @ViewChild('scrollContent', { read: ElementRef, static: true })
  private scrollContent?: ElementRef<HTMLElement>;

  scrollContainer?: HTMLElement;

  constructor(
    private router: Router,
    private stateService: AdminStateService
  ) { }

  ngOnInit(): void {
    this.scrollContainer = this.scrollContent?.nativeElement;

    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      takeUntil(this.destroy$)
    ).subscribe(e => {
      const navEnd = e as NavigationEnd;
      if (navEnd.url.startsWith('/admin/') && navEnd.url !== '/admin') {
        this.stateService.lastVisitedRoute = navEnd.url;
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  isMobile = window.innerWidth < 992;

  onMenuItemClick(sidenav: MatSidenav): void {
    if (this.isMobile) {
      sidenav.close();
    }
  }

  @HostListener('window:resize')
  onResize(): void {
    this.isMobile = window.innerWidth < 992;
  }

}
