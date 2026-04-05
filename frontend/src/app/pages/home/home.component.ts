import { Component, OnInit, AfterViewInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService, Cake, Filling, Testimonial } from '../../services/api.service';
import { SanitizeHtmlPipe } from '../../pipes/sanitize-html.pipe';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, SanitizeHtmlPipe],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit, AfterViewInit {
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

  ngAfterViewInit(): void {
    setTimeout(() => this.checkVisibility(), 100);
  }

  @HostListener('window:scroll', ['$event'])
  onScroll(): void {
    this.checkVisibility();
  }

  checkVisibility(): void {
    const elements = document.querySelectorAll('.animate-on-scroll');
    const windowHeight = window.innerHeight;
    const triggerPoint = 100;

    elements.forEach(element => {
      const rect = element.getBoundingClientRect();
      if (rect.top < windowHeight - triggerPoint) {
        element.classList.add('visible');
      }
    });
  }

  loadData(): void {
    this.isLoading = true;
    this.error = null;

    const testimonialIds = [20, 11, 7];

    Promise.all([
      this.apiService.getFeaturedCakes().toPromise(),
      this.apiService.getAvailableFillings().toPromise(),
      ...testimonialIds.map(id => this.apiService.getTestimonialById(id).toPromise())
    ]).then(results => {
      const [cakes, fillings, ...testimonials] = results;

      this.featuredCakes = cakes || [];
      this.fillings = fillings || [];
      this.testimonials = testimonials.filter(Boolean) as Testimonial[];

      this.isLoading = false;
      setTimeout(() => this.checkVisibility(), 100);
    }).catch(error => {
      console.error('Error loading data:', error);
      this.error = 'Не удалось загрузить данные';
      this.isLoading = false;
    });
  }

  getCakeImageUrl(cake: Cake): string {
    if (cake.imageUrl) {
      if (cake.imageUrl.startsWith('http') || cake.imageUrl.startsWith('/')) {
        return cake.imageUrl;
      }
      return `/images/portfolio/${cake.imageUrl}`;
    }
    if (cake.thumbnailUrl) {
      if (cake.thumbnailUrl.startsWith('http') || cake.thumbnailUrl.startsWith('/')) {
        return cake.thumbnailUrl;
      }
      return `/images/portfolio/${cake.thumbnailUrl}`;
    }
    return this.imagePaths.placeholder;
  }

  getFillingImageUrl(imageName: string): string {
    if (!imageName) {
      return this.imagePaths.placeholder;
    }
    return `${this.imagePaths.fillings}${imageName}`;
  }

  handleImageError(event: any): void {
    event.target.src = this.imagePaths.placeholder;
  }
}
