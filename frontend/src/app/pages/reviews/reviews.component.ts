import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, Testimonial } from '../../services/api.service';
import { RecaptchaService } from '../../services/recaptcha.service';
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
  isSubmitting = false;
  submitted = false;
  errorMessage = '';
  totalCount = 0;
  page = 1;
  pageSize = 9;

  formModel = { author: '', text: '', rating: null as number | null };

  stars = [1, 2, 3, 4, 5];
  hoverRating: number | null = null;

  private observer!: IntersectionObserver;

  constructor(private apiService: ApiService, private recaptchaService: RecaptchaService) {}

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

    this.apiService.getTestimonialsPaged(this.page, this.pageSize).subscribe({
      next: (response) => {
        this.testimonials = response.items;
        this.totalCount = response.totalCount;
        this.isLoading = false;

        this.observeElements();
      },
      error: () => {
        this.error = 'Не удалось загрузить отзывы';
        this.isLoading = false;
      }
    });
  }

  loadMore(): void {
    const nextPage = this.page + 1;

    this.apiService.getTestimonialsPaged(nextPage, this.pageSize).subscribe({
      next: (response) => {
        this.testimonials = [...this.testimonials, ...response.items];
        this.totalCount = response.totalCount;
        this.page = nextPage;

        this.observeElements();
      },
      error: () => {
        this.error = 'Не удалось загрузить отзывы';
      }
    });
  }

  get hasMore(): boolean {
    return this.totalCount > this.testimonials.length;
  }

  setRating(rating: number): void {
    this.formModel.rating = rating;
  }

  onHover(rating: number | null): void {
    this.hoverRating = rating;
  }

  get displayRating(): number {
    return this.hoverRating ?? this.formModel.rating ?? 0;
  }

  async submit(): Promise<void> {
    const name = this.formModel.author.trim();
    const text = this.formModel.text.trim();

    this.errorMessage = '';

    if (!name) {
      this.errorMessage = 'Пожалуйста, укажите ваше имя';
      return;
    }

    if (text.length < 3) {
      this.errorMessage = 'Отзыв должен содержать не менее 3 символов';
      return;
    }

    this.isSubmitting = true;

    try {
      const recaptchaToken = await this.recaptchaService.getToken('review');

      const payload: Partial<Testimonial> & { recaptchaToken?: string } = {
        author: name,
        text,
        rating: this.formModel.rating ?? undefined,
        recaptchaToken
      };

      this.apiService.createTestimonial(payload).subscribe({
        next: () => {
          this.isSubmitting = false;
          this.submitted = true;
          this.errorMessage = '';
          this.formModel = { author: '', text: '', rating: null };
          this.hoverRating = null;
          setTimeout(() => {
            this.submitted = false;
          }, 5000);
        },
        error: () => {
          this.isSubmitting = false;
          this.errorMessage = 'Не удалось отправить отзыв. Попробуйте ещё раз';
        }
      });
    } catch {
      this.isSubmitting = false;
      this.errorMessage = 'Не удалось проверить капчу. Попробуйте ещё раз';
    }
  }
}
