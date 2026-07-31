import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { PortfolioComponent } from './pages/portfolio/portfolio.component';
import { OrderComponent } from './pages/order/order.component';
import { ReviewsComponent } from './pages/reviews/reviews.component';
import { FillingsComponent } from './pages/fillings/fillings.component';
import { LoginComponent } from './pages/login/login.component';

import { AdminAuthGuard } from './admin/guards/admin-auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'portfolio', component: PortfolioComponent },
  { path: 'order', component: OrderComponent },
  { path: 'reviews', component: ReviewsComponent },
  { path: 'fillings', component: FillingsComponent },
  { path: 'login', component: LoginComponent },

  {
    path: 'admin',
    canMatch: [AdminAuthGuard],
    loadChildren: () =>
      import('./admin/admin.module').then(m => m.AdminModule)
  },

  { path: '**', redirectTo: '' }
];
