import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, Cake, Filling, Testimonial } from '../../services/api.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  featuredCakes: Cake[] = [];
  fillings: Filling[] = [];
  testimonials: Testimonial[] = [];
  isLoading = true;
  error: string | null = null;

  imagePaths = {
    portfolio: '/images/portfolio/',
    fillings: '/images/fillings/',
    placeholder: '/images/placeholder-cake.jpg'
  };

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;

    Promise.all([
      this.apiService.getFeaturedCakes().toPromise(),
      this.apiService.getAvailableFillings().toPromise(),
      this.apiService.getLatestTestimonials(3).toPromise()
    ]).then(([cakes, fillings, testimonials]) => {
      this.featuredCakes = cakes || [];
      this.fillings = fillings || [];
      this.testimonials = testimonials || [];
      this.isLoading = false;
    }).catch(error => {
      console.error('Error loading data:', error);
      this.error = 'Не удалось загрузить данные';
      this.isLoading = false;
    });
  }

  getCakeImageUrl(cake: Cake): string {
    const imageName = cake.thumbnailUrl || cake.imageUrl;
    if (!imageName) {
      return this.imagePaths.placeholder;
    }
    return `${this.imagePaths.portfolio}${imageName}`;
  }

  getFillingImageUrl(imageName: string): string {
    if (!imageName) {
      return this.imagePaths.placeholder;
    }
    return `${this.imagePaths.fillings}${imageName}`;
  }

  handleImageError(event: any): void {
    if (event.target.src === this.imagePaths.placeholder) {
      return;
    }
    event.target.src = this.imagePaths.placeholder;
    event.target.onerror = null;
  }
}
