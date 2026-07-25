import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { CakeListComponent } from './pages/cakes/cake-list/cake-list.component';
import { CategoryListComponent } from './pages/categories/category-list/category-list.component';
import { FillingListComponent } from './pages/fillings/filling-list/filling-list.component';
import { TestimonialListComponent } from './pages/testimonials/testimonial-list/testimonial-list.component';
import { OrderListComponent } from './pages/orders/order-list/order-list.component';
import { ContactsEditComponent } from './pages/contacts/contacts-edit/contacts-edit.component';
import { SecuritySettingsComponent } from './pages/security/security-settings/security-settings.component';
import { AdminAuthGuard } from './guards/admin-auth.guard';

const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,

    canActivate: [AdminAuthGuard],
    canActivateChild: [AdminAuthGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'cakes', component: CakeListComponent },
      { path: 'categories', component: CategoryListComponent },
      { path: 'fillings', component: FillingListComponent },
      { path: 'testimonials', component: TestimonialListComponent },
      { path: 'orders', component: OrderListComponent },
      { path: 'contacts', component: ContactsEditComponent },
      { path: 'security', component: SecuritySettingsComponent },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule {}
