import { Component, OnInit, AfterViewInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ApiService, Cake, Filling, Testimonial } from '../../services/api.service';
import { SanitizeHtmlPipe } from '../../pipes/sanitize-html.pipe';
import { ImagePreviewDialogComponent } from '../../shared/image-preview-dialog/image-preview-dialog.component';

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

  placeholderImage = '/images/placeholder-cake.jpg';

  constructor(public apiService: ApiService, private dialog: MatDialog) { }

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

    Promise.all([
      this.apiService.getFeaturedCakes().toPromise(),
      this.apiService.getAvailableFillings().toPromise(),
      this.apiService.getHomePageTestimonials().toPromise()
    ]).then(results => {
      const [cakes, fillings, testimonials] = results;

      this.featuredCakes = cakes || [];
      this.fillings = fillings || [];
      this.testimonials = testimonials || [];

      this.isLoading = false;
      setTimeout(() => this.checkVisibility(), 100);
    }).catch(error => {
      console.error('Error loading data:', error);
      this.error = 'Не удалось загрузить данные';
      this.isLoading = false;
    });
  }

  getCakeImageUrl(cake: Cake): string {
    return this.apiService.getCakeImageUrl(cake.imageUrl);
  }

  getFillingImageUrl(imageName: string): string {
    return this.apiService.getFillingImageUrl(imageName);
  }

  handleImageError(event: any): void {
    if (event.target.src !== this.placeholderImage) {
      event.target.src = this.placeholderImage;
      event.target.onerror = null;
    }
  }

  openImagePreview(imageUrl: string, alt: string): void {
    if (!imageUrl) return;
    this.dialog.open(ImagePreviewDialogComponent, {
      data: { imageUrl, alt },
      panelClass: 'image-preview-dialog'
    });
  }
}
