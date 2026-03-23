import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { PortfolioComponent } from './pages/portfolio/portfolio.component';
import { OrderComponent } from './pages/order/order.component';
import { ReviewsComponent } from './pages/reviews/reviews.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'portfolio', component: PortfolioComponent },
  { path: 'order', component: OrderComponent },
  { path: 'reviews', component: ReviewsComponent },
  { path: '**', redirectTo: '' }
];
