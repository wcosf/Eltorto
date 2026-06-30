import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { AdminRoutingModule } from './admin-routing.module';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout.component';

import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { CakeListComponent } from './pages/cakes/cake-list/cake-list.component';
import { CategoryListComponent } from './pages/categories/category-list/category-list.component';
import { FillingListComponent } from './pages/fillings/filling-list/filling-list.component';
import { TestimonialListComponent } from './pages/testimonials/testimonial-list/testimonial-list.component';
import { OrderListComponent } from './pages/orders/order-list/order-list.component';
import { PageListComponent } from './pages/pages/page-list/page-list.component';
import { SliderListComponent } from './pages/slider/slider-list/slider-list.component';
import { ContactsEditComponent } from './pages/contacts/contacts-edit/contacts-edit.component';

import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    AdminRoutingModule,

    AdminLayoutComponent,
    DashboardComponent,
    CakeListComponent,
    CategoryListComponent,
    FillingListComponent,
    TestimonialListComponent,
    OrderListComponent,
    PageListComponent,
    SliderListComponent,
    ContactsEditComponent,

    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatDialogModule,
    MatCardModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatChipsModule,
    MatTooltipModule
  ]
})
export class AdminModule { }
