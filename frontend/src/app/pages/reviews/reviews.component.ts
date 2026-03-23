import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, Testimonial } from '../../services/api.service';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reviews.component.html',
  styleUrls: ['./reviews.component.scss']
})
export class ReviewsComponent implements OnInit {
  testimonials: Testimonial[] = [];
  newReview = {
    author: '',
    email: '',
    text: ''
  };
  isLoading = true;
  isSubmitting = false;
  error: string | null = null;
  successMessage = '';

  constructor(private apiService: ApiService) { }

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.isLoading = true;
    this.apiService.getApprovedTestimonials().subscribe({
      next: (reviews: Testimonial[]) => {
        this.testimonials = reviews;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading reviews:', error);
        this.error = 'Не удалось загрузить отзывы';
        this.isLoading = false;
      }
    });
  }

  onSubmitReview(): void {
    if (!this.newReview.author || !this.newReview.text) {
      this.error = 'Пожалуйста, заполните имя и текст отзыва';
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    this.apiService.createTestimonial(this.newReview).subscribe({
      next: () => {
        this.successMessage = 'Спасибо за ваш отзыв! Он будет опубликован после проверки.';
        this.newReview = { author: '', email: '', text: '' };
        this.isSubmitting = false;

        setTimeout(() => {
          this.successMessage = '';
        }, 5000);
      },
      error: (error: any) => {
        console.error('Error submitting review:', error);
        this.error = 'Произошла ошибка. Пожалуйста, попробуйте позже.';
        this.isSubmitting = false;
      }
    });
  }
}
