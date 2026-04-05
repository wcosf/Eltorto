import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, Testimonial } from '../../services/api.service';
import { SanitizeHtmlPipe } from '../../pipes/sanitize-html.pipe';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule, SanitizeHtmlPipe],
  templateUrl: './reviews.component.html',
  styleUrls: ['./reviews.component.scss']
})
export class ReviewsComponent implements OnInit, AfterViewInit {
  testimonials: Testimonial[] = [];
  isLoading = true;
  error: string | null = null;

  private observer!: IntersectionObserver;

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.initObserver();
    this.loadReviews();
  }

  ngAfterViewInit() {
    this.observeElements();
  }

  initObserver() {
    this.observer = new IntersectionObserver(
      (entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
          }
        });
      },
      { threshold: 0.1 }
    );
  }

  observeElements() {
    setTimeout(() => {
      const elements = document.querySelectorAll('.animate-on-scroll');
      elements.forEach(el => this.observer.observe(el));
    });
  }

  loadReviews(): void {
    this.isLoading = true;

    this.apiService.getApprovedTestimonials().subscribe({
      next: (reviews: Testimonial[]) => {
        this.testimonials = reviews;
        this.isLoading = false;

        this.observeElements();
      },
      error: () => {
        this.error = 'Не удалось загрузить отзывы';
        this.isLoading = false;
      }
    });
  }
}
