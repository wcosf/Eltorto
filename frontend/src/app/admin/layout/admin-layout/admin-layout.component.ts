import { Component, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenav } from '@angular/material/sidenav';

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
    MatButtonModule
  ],
  templateUrl: './admin-layout.component.html',
  styleUrls: ['./admin-layout.component.scss']
})
export class AdminLayoutComponent {

  menuItems = [
    { path: '/admin/dashboard', icon: 'dashboard', label: 'Дашборд' },
    { path: '/admin/cakes', icon: 'cake', label: 'Торты' },
    { path: '/admin/categories', icon: 'category', label: 'Категории' },
    { path: '/admin/fillings', icon: 'layers', label: 'Начинки' },
    { path: '/admin/testimonials', icon: 'comment', label: 'Отзывы' },
    { path: '/admin/orders', icon: 'shopping_cart', label: 'Заказы' },
    { path: '/admin/pages', icon: 'description', label: 'Страницы' },
    { path: '/admin/slider', icon: 'slideshow', label: 'Слайдер' },
    { path: '/admin/contacts', icon: 'contact_phone', label: 'Контакты' }
  ];

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
